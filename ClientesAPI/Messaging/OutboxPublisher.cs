using System.Text;
using ClientesAPI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace ClientesAPI.Messaging;

public sealed class OutboxPublisher : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RabbitMqOptions _options;
    private readonly ILogger<OutboxPublisher> _logger;

    private IConnection? _connection;
    private IChannel? _channel;

    public OutboxPublisher(
        IServiceScopeFactory scopeFactory,
        IOptions<RabbitMqOptions> options,
        ILogger<OutboxPublisher> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var encontroMensajes =
                    await PublicarPendientesAsync(stoppingToken);

                var espera = encontroMensajes
                    ? TimeSpan.FromMilliseconds(250)
                    : TimeSpan.FromSeconds(2);

                await Task.Delay(espera, stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogWarning(
                    exception,
                    "No fue posible publicar el Outbox.");

                await ReiniciarConexionAsync();

                await Task.Delay(
                    TimeSpan.FromSeconds(5),
                    stoppingToken);
            }
        }
    }

    private async Task<bool> PublicarPendientesAsync(
        CancellationToken cancellationToken)
    {
        await using var scope =
            _scopeFactory.CreateAsyncScope();

        var context =
            scope.ServiceProvider
                .GetRequiredService<ClientesDbContext>();

        var mensajes = await context.OutboxMessages
            .Where(x => x.PublishedAt == null)
            .OrderBy(x => x.OccurredAt)
            .Take(20)
            .ToListAsync(cancellationToken);

        if (mensajes.Count == 0)
            return false;

        foreach (var mensaje in mensajes)
        {
            try
            {
                await PublicarAsync(
                    mensaje,
                    cancellationToken);

                mensaje.PublishedAt = DateTime.UtcNow;
                mensaje.LastError = null;

                await context.SaveChangesAsync(
                    cancellationToken);
            }
            catch (Exception exception)
            {
                mensaje.Attempts++;
                mensaje.LastError =
                    LimitarError(exception.Message);

                await context.SaveChangesAsync(
                    cancellationToken);

                throw;
            }
        }

        return true;
    }

    private async Task PublicarAsync(
        Outbox.OutboxMessage mensaje,
        CancellationToken cancellationToken)
    {
        await AsegurarConexionAsync(cancellationToken);

        var properties = new BasicProperties
        {
            AppId = "ClientesAPI",
            ContentType = "application/json",
            Type = mensaje.EventType,
            MessageId = mensaje.Id.ToString(),
            Persistent = true
        };

        var body = Encoding.UTF8.GetBytes(
            mensaje.Payload);

        await _channel!.BasicPublishAsync(
            exchange: _options.Exchange,
            routingKey: mensaje.RoutingKey,
            mandatory: true,
            basicProperties: properties,
            body: body,
            cancellationToken: cancellationToken);
    }

    private async Task AsegurarConexionAsync(
        CancellationToken cancellationToken)
    {
        if (_connection?.IsOpen == true &&
            _channel?.IsOpen == true)
        {
            return;
        }

        await ReiniciarConexionAsync();

        var factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            Port = _options.Port,
            UserName = _options.UserName,
            Password = _options.Password,
            VirtualHost = _options.VirtualHost,
            AutomaticRecoveryEnabled = true,
            TopologyRecoveryEnabled = true,
            RequestedHeartbeat = TimeSpan.FromSeconds(30)
        };

        _connection = await factory.CreateConnectionAsync(
            "clientes-api-outbox",
            cancellationToken);

        var channelOptions = new CreateChannelOptions(
            publisherConfirmationsEnabled: true,
            publisherConfirmationTrackingEnabled: true);

        _channel = await _connection.CreateChannelAsync(
            channelOptions,
            cancellationToken);

        await _channel.ExchangeDeclareAsync(
            exchange: _options.Exchange,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);
    }

    private async Task ReiniciarConexionAsync()
    {
        if (_channel is not null)
        {
            try
            {
                await _channel.DisposeAsync();
            }
            catch
            {
                // La conexión ya puede estar cerrada.
            }

            _channel = null;
        }

        if (_connection is not null)
        {
            try
            {
                await _connection.DisposeAsync();
            }
            catch
            {
                // La conexión ya puede estar cerrada.
            }

            _connection = null;
        }
    }

    private static string LimitarError(string error)
    {
        return error.Length <= 2000
            ? error
            : error[..2000];
    }

    public override async Task StopAsync(
        CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken);
        await ReiniciarConexionAsync();
    }
}