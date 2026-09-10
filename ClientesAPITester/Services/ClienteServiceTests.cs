using System.Text.Json;
using ClientesAPI.DTOs.Clientes;
using ClientesAPI.Messaging.Contracts;
using ClientesAPI.Messaging.Outbox;
using ClientesAPI.Models;
using ClientesAPI.Repositories.Interfaces;
using ClientesAPI.Services;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace ClientesAPITester.Services;

public sealed class ClienteServiceTests
{
    [Fact]
    public async Task CrearAsync_IdentificacionNueva_CreaClienteYEventoOutbox()
    {
        // Arrange
        var repository =
            new Mock<IClienteRepository>(MockBehavior.Strict);

        var passwordHasher =
            new Mock<IPasswordHasher<Cliente>>(MockBehavior.Strict);

        var request = CrearRequest();

        Cliente? clienteGuardado = null;
        OutboxMessage? mensajeOutbox = null;

        repository
            .Setup(x => x.ExisteIdentificacionAsync(
                request.Identificacion,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        passwordHasher
            .Setup(x => x.HashPassword(
                It.IsAny<Cliente>(),
                request.Contrasena))
            .Returns("HASH_GENERADO");

        repository
            .Setup(x => x.CrearConEventoAsync(
                It.IsAny<Cliente>(),
                It.IsAny<Func<Cliente, OutboxMessage>>(),
                It.IsAny<CancellationToken>()))
            .Callback<
                Cliente,
                Func<Cliente, OutboxMessage>,
                CancellationToken>(
                (cliente, construirEvento, _) =>
                {
                    // Simula el Id generado por SQL Server.
                    cliente.Id = 10;
                    clienteGuardado = cliente;
                    mensajeOutbox = construirEvento(cliente);
                })
            .Returns(Task.CompletedTask);

        var service = new ClienteService(
            repository.Object,
            passwordHasher.Object);

        // Act
        var resultado = await service.CrearAsync(request);

        // Assert: respuesta
        Assert.Equal(10, resultado.ClienteId);
        Assert.Equal("Jose Lema", resultado.Nombre);
        Assert.Equal("0912345678", resultado.Identificacion);
        Assert.True(resultado.Estado);

        // Assert: entidad
        Assert.NotNull(clienteGuardado);
        Assert.Equal("HASH_GENERADO", clienteGuardado.ContrasenaHash);
        Assert.NotEqual(request.Contrasena, clienteGuardado.ContrasenaHash);

        // Assert: Outbox
        Assert.NotNull(mensajeOutbox);
        Assert.Equal(
            ClienteEventTypes.Creado,
            mensajeOutbox.EventType);

        Assert.Equal(
            ClienteRoutingKeys.Creado,
            mensajeOutbox.RoutingKey);

        var evento =
            JsonSerializer.Deserialize<ClienteIntegrationEvent>(
                mensajeOutbox.Payload,
                new JsonSerializerOptions(
                    JsonSerializerDefaults.Web));

        Assert.NotNull(evento);
        Assert.Equal(mensajeOutbox.Id, evento.EventId);
        Assert.Equal(10, evento.Data.ClienteId);
        Assert.Equal("Jose Lema", evento.Data.Nombre);
        Assert.Equal("0912345678", evento.Data.Identificacion);
        Assert.True(evento.Data.Estado);

        repository.Verify(
            x => x.CrearConEventoAsync(
                It.IsAny<Cliente>(),
                It.IsAny<Func<Cliente, OutboxMessage>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        passwordHasher.Verify(
            x => x.HashPassword(
                It.IsAny<Cliente>(),
                request.Contrasena),
            Times.Once);
    }

    [Fact]
    public async Task CrearAsync_IdentificacionDuplicada_LanzaConflicto()
    {
        // Arrange
        var repository =
            new Mock<IClienteRepository>(MockBehavior.Strict);

        var passwordHasher =
            new Mock<IPasswordHasher<Cliente>>(MockBehavior.Strict);

        var request = CrearRequest();

        repository
            .Setup(x => x.ExisteIdentificacionAsync(
                request.Identificacion,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var service = new ClienteService(
            repository.Object,
            passwordHasher.Object);

        // Act
        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.CrearAsync(request));

        // Assert
        Assert.Contains(
            "Ya existe un cliente",
            exception.Message);

        repository.Verify(
            x => x.CrearConEventoAsync(
                It.IsAny<Cliente>(),
                It.IsAny<Func<Cliente, OutboxMessage>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        passwordHasher.Verify(
            x => x.HashPassword(
                It.IsAny<Cliente>(),
                It.IsAny<string>()),
            Times.Never);
    }

    private static CrearClienteRequest CrearRequest()
    {
        return new CrearClienteRequest
        {
            Nombre = "Jose Lema",
            Edad = 35,
            Identificacion = "0912345678",
            Direccion = "Otavalo sn y principal",
            Telefono = "098254785",
            Contrasena = "1234",
            Estado = true
        };
    }
}