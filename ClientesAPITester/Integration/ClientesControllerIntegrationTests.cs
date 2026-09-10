using ClientesAPI.Data;
using ClientesAPI.DTOs.Clientes;
using ClientesAPI.Messaging.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace ClientesAPITester.Integration;

public sealed class ClientesControllerIntegrationTests
    : IClassFixture<ClientesApiFactory>
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    private readonly ClientesApiFactory _factory;
    private readonly HttpClient _client;

    public ClientesControllerIntegrationTests(
        ClientesApiFactory factory)
    {
        _factory = factory;

        _client = factory.CreateClient(
            new()
            {
                BaseAddress =
                    new Uri("https://localhost")
            });
    }

    [Fact]
    public async Task PostClientes_SolicitudValida_RetornaCreatedYGuardaClienteConOutbox()
    {
        // Arrange
        var request = new CrearClienteRequest
        {
            Nombre = "Carlos Lema",
            Edad = 35,
            Identificacion = "0912345608",
            Direccion = "Otavalo sn y principal",
            Telefono = "098254685",
            Contrasena = "1234",
            Estado = true
        };

    // Act
    var response = await _client.PostAsJsonAsync(
        "/clientes",
        request);

    // Assert HTTP
    Assert.Equal(
        HttpStatusCode.Created,
        response.StatusCode);

        var clienteCreado =
            await response.Content
                .ReadFromJsonAsync<ClienteResponse>(
                    JsonOptions);

    Assert.NotNull(clienteCreado);
        Assert.True(clienteCreado.ClienteId > 0);
        Assert.Equal("Carlos Lema", clienteCreado.Nombre);
        Assert.Equal(
            "0912345608",
            clienteCreado.Identificacion);
        Assert.True(clienteCreado.Estado);

        // Assert persistencia
        await using var scope =
            _factory.Services.CreateAsyncScope();

        var context = scope.ServiceProvider
            .GetRequiredService<ClientesDbContext>();

    var clienteGuardado = await context.Clientes
        .AsNoTracking()
        .SingleAsync(
            cliente =>
                cliente.Id ==
                clienteCreado.ClienteId);

        Assert.Equal(
            request.Identificacion,
            clienteGuardado.Identificacion);

        Assert.NotEmpty(clienteGuardado.ContrasenaHash);

        Assert.NotEqual(
            request.Contrasena,
            clienteGuardado.ContrasenaHash);

        // Assert Outbox
        var mensajeOutbox =
            await context.OutboxMessages
                .AsNoTracking()
                .SingleAsync();

        Assert.Equal(
            ClienteEventTypes.Creado,
            mensajeOutbox.EventType);

        Assert.Equal(
            ClienteRoutingKeys.Creado,
            mensajeOutbox.RoutingKey);

        Assert.Null(mensajeOutbox.PublishedAt);

        var evento =
            JsonSerializer.Deserialize<
                ClienteIntegrationEvent>(
                    mensajeOutbox.Payload,
                    JsonOptions);

        Assert.NotNull(evento);

        Assert.Equal(
            mensajeOutbox.Id,
            evento.EventId);

        Assert.Equal(
            clienteCreado.ClienteId,
            evento.Data.ClienteId);

        Assert.Equal(
            clienteCreado.Nombre,
            evento.Data.Nombre);

        Assert.Equal(
            clienteCreado.Identificacion,
            evento.Data.Identificacion);

        Assert.True(evento.Data.Estado);
    }
}