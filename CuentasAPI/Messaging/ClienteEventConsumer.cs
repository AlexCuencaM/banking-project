using System.Text.Json;
using CuentasAPI.Data;
using CuentasAPI.Messaging.Contracts;
using CuentasAPI.Messaging.Inbox;
using CuentasAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CuentasAPI.Messaging;

public sealed class ClienteEventConsumer : BackgroundService
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RabbitMqOptions _options;
    private readonly ILogger<ClienteEventConsumer> _logger;

    private IConnection? _connection;
    private IChannel? _channel;

    public ClienteEventConsumer(
        IServiceScopeFactory scopeFactory,
        IOptions<RabbitMqOptions> options,
        ILogger<ClienteEventConsumer> logger)
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
                await ConectarAsync(stoppingToken);

                while (!stoppingToken.IsCancellationRequested &&
                       _connection?.IsOpen == true &&
                       _channel?.IsOpen == true)
                {
                    await Task.Delay(
                        TimeSpan.FromSeconds(1),
                        stoppingToken);
                }
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Error en el consumidor de clientes.");
            }
            finally
            {
                await ReiniciarConexionAsync();
            }

            if (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(
                    TimeSpan.FromSeconds(5),
                    stoppingToken);
            }
        }
    }

    private async Task ConectarAsync(
        CancellationToken cancellationToken)
    {
        await ReiniciarConexionAsync();

        var factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            Port = _options.Port,
            UserName = _options.UserName,
            Password = _options.Password,
            VirtualHost = _options.VirtualHost,
            AutomaticRecoveryEnabled = false,
            RequestedHeartbeat = TimeSpan.FromSeconds(30)
        };

        _connection = await factory.CreateConnectionAsync(
            "cuentas-api-clientes-consumer",
            cancellationToken);

        _channel = await _connection.CreateChannelAsync(
            cancellationToken: cancellationToken);

        await DeclararTopologiaAsync(
            _channel,
            cancellationToken);

        await _channel.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: _options.PrefetchCount,
            global: false,
            cancellationToken: cancellationToken);

        var channel = _channel;
        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += (_, eventArgs) =>
            ProcesarMensajeAsync(channel, eventArgs);

        await channel.BasicConsumeAsync(
            queue: _options.Queue,
            autoAck: false,
            consumer: consumer,
            cancellationToken: cancellationToken);

        _logger.LogInformation(
            "Consumidor conectado a la cola {Queue}.",
            _options.Queue);
    }

    private async Task DeclararTopologiaAsync(
        IChannel channel,
        CancellationToken cancellationToken)
    {
        await channel.ExchangeDeclareAsync(
            exchange: _options.Exchange,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        await channel.ExchangeDeclareAsync(
            exchange: _options.DeadLetterExchange,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            queue: _options.DeadLetterQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            queue: _options.DeadLetterQueue,
            exchange: _options.DeadLetterExchange,
            routingKey: _options.DeadLetterRoutingKey,
            arguments: null,
            cancellationToken: cancellationToken);

        var queueArguments =
            new Dictionary<string, object?>
            {
                ["x-queue-type"] = "quorum",
                ["x-delivery-limit"] = _options.DeliveryLimit,
                ["x-dead-letter-exchange"] =
                    _options.DeadLetterExchange,
                ["x-dead-letter-routing-key"] =
                    _options.DeadLetterRoutingKey
            };

        await channel.QueueDeclareAsync(
            queue: _options.Queue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: queueArguments,
            cancellationToken: cancellationToken);

        var routingKeys = new[]
        {
            ClienteRoutingKeys.Creado,
            ClienteRoutingKeys.Actualizado,
            ClienteRoutingKeys.Eliminado
        };

        foreach (var routingKey in routingKeys)
        {
            await channel.QueueBindAsync(
                queue: _options.Queue,
                exchange: _options.Exchange,
                routingKey: routingKey,
                arguments: null,
                cancellationToken: cancellationToken);
        }
    }

    private async Task ProcesarMensajeAsync(
        IChannel channel,
        BasicDeliverEventArgs eventArgs)
    {
        // RabbitMQ indica que el cuerpo debe copiarse
        // antes de terminar ReceivedAsync.
        var body = eventArgs.Body.ToArray();

        try
        {
            var evento =
                JsonSerializer.Deserialize<ClienteIntegrationEvent>(
                    body,
                    JsonOptions)
                ?? throw new JsonException(
                    "El evento de cliente está vacío.");

            await AplicarEventoAsync(
                evento,
                eventArgs.CancellationToken);

            await channel.BasicAckAsync(
                deliveryTag: eventArgs.DeliveryTag,
                multiple: false,
                cancellationToken:
                    eventArgs.CancellationToken);
        }
        catch (OperationCanceledException)
            when (eventArgs.CancellationToken.IsCancellationRequested)
        {
            // Al cerrar el canal, RabbitMQ volverá a encolar
            // cualquier mensaje que no tenga confirmación.
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "No se pudo procesar el mensaje {MessageId}.",
                eventArgs.BasicProperties.MessageId);

            if (channel.IsOpen)
            {
                await channel.BasicNackAsync(
                    deliveryTag: eventArgs.DeliveryTag,
                    multiple: false,
                    requeue: true,
                    cancellationToken: CancellationToken.None);
            }
        }
    }

    private async Task AplicarEventoAsync(
        ClienteIntegrationEvent evento,
        CancellationToken cancellationToken)
    {
        if (evento.Version != 1)
        {
            throw new NotSupportedException(
                $"Versión de evento no soportada: {evento.Version}.");
        }

        if (evento.EventType is not
            (ClienteEventTypes.Creado or
             ClienteEventTypes.Actualizado or
             ClienteEventTypes.Eliminado))
        {
            throw new InvalidOperationException(
                $"Evento no soportado: {evento.EventType}.");
        }

        await using var scope =
            _scopeFactory.CreateAsyncScope();

        var context = scope.ServiceProvider
            .GetRequiredService<CuentasDbContext>();

        var yaProcesado = await context.InboxMessages
            .AnyAsync(
                x => x.EventId == evento.EventId,
                cancellationToken);

        if (yaProcesado)
            return;

        var cliente = await context.ClientesProyeccion
            .SingleOrDefaultAsync(
                x => x.ClienteId == evento.Data.ClienteId,
                cancellationToken);

        var estado = evento.EventType ==
                     ClienteEventTypes.Eliminado
            ? false
            : evento.Data.Estado;

        if (cliente is null)
        {
            cliente = new ClienteProyeccion
            {
                ClienteId = evento.Data.ClienteId,
                Nombre = evento.Data.Nombre,
                Identificacion = evento.Data.Identificacion,
                Estado = estado,
                UltimoEventoEn = evento.OccurredAt
            };

            await context.ClientesProyeccion.AddAsync(
                cliente,
                cancellationToken);
        }
        else if (evento.OccurredAt >= cliente.UltimoEventoEn)
        {
            cliente.Nombre = evento.Data.Nombre;
            cliente.Identificacion =
                evento.Data.Identificacion;
            cliente.Estado = estado;
            cliente.UltimoEventoEn = evento.OccurredAt;
        }

        await context.InboxMessages.AddAsync(
            new InboxMessage
            {
                EventId = evento.EventId,
                EventType = evento.EventType,
                ProcessedAt = DateTime.UtcNow
            },
            cancellationToken);

        // La proyección y el Inbox se confirman
        // dentro de la misma transacción de SaveChanges.
        await context.SaveChangesAsync(cancellationToken);
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
                // El canal puede estar cerrado.
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
                // La conexión puede estar cerrada.
            }

            _connection = null;
        }
    }

    public override async Task StopAsync(
        CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken);
        await ReiniciarConexionAsync();
    }
}