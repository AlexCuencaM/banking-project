using ClientesAPI.DTOs.Clientes;
using ClientesAPI.Models;
using ClientesAPI.Repositories.Interfaces;
using ClientesAPI.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using ClientesAPI.Messaging.Contracts;
using ClientesAPI.Messaging.Outbox;
using System.Text.Json;
namespace ClientesAPI.Services;

public sealed class ClienteService : IClienteService
{
    private readonly IClienteRepository _repository;
    private readonly IPasswordHasher<Cliente> _passwordHasher;

    public ClienteService(
        IClienteRepository repository,
        IPasswordHasher<Cliente> passwordHasher)
    {
        _repository = repository;
        _passwordHasher = passwordHasher;
    }
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);
    public async Task<IReadOnlyList<ClienteResponse>> ObtenerTodosAsync(
        CancellationToken cancellationToken = default)
    {
        var clientes =
            await _repository.ObtenerTodosAsync(cancellationToken);

        return clientes.Select(Mapear).ToList();
    }

    public async Task<ClienteResponse> ObtenerPorIdAsync(
        int clienteId,
        CancellationToken cancellationToken = default)
    {
        var cliente = await _repository.ObtenerPorIdAsync(
            clienteId,
            cancellationToken);

        if (cliente is null)
        {
            throw new KeyNotFoundException(
                $"No se encontró el cliente {clienteId}.");
        }

        return Mapear(cliente);
    }

    public async Task<ClienteResponse> CrearAsync(
        CrearClienteRequest request,
        CancellationToken cancellationToken = default)
    {
        var identificacion = request.Identificacion.Trim();

        if (await _repository.ExisteIdentificacionAsync(
                identificacion,
                cancellationToken: cancellationToken))
        {
            throw new InvalidOperationException(
                $"Ya existe un cliente con la identificación {identificacion}.");
        }

        var cliente = new Cliente
        {
            Nombre = request.Nombre.Trim(),
            Edad = request.Edad,
            Identificacion = identificacion,
            Direccion = request.Direccion.Trim(),
            Telefono = request.Telefono.Trim(),
            Estado = request.Estado,
            FechaCreacion = DateTime.UtcNow
        };

        cliente.ContrasenaHash =
            _passwordHasher.HashPassword(
                cliente,
                request.Contrasena);

        await _repository.CrearConEventoAsync(
            cliente,
            clienteGuardado => CrearEvento(
                clienteGuardado,
                ClienteEventTypes.Creado,
                ClienteRoutingKeys.Creado),
            cancellationToken);

        return Mapear(cliente);
    }

    public async Task ActualizarAsync(
        int clienteId,
        ActualizarClienteRequest request,
        CancellationToken cancellationToken = default)
    {
        var cliente = await _repository.ObtenerPorIdAsync(
            clienteId,
            cancellationToken);

        if (cliente is null)
        {
            throw new KeyNotFoundException(
                $"No se encontró el cliente {clienteId}.");
        }

        var identificacion = request.Identificacion.Trim();

        if (await _repository.ExisteIdentificacionAsync(
                identificacion,
                clienteId,
                cancellationToken))
        {
            throw new InvalidOperationException(
                $"Ya existe otro cliente con la identificación {identificacion}.");
        }

        cliente.Nombre = request.Nombre.Trim();
        cliente.Edad = request.Edad;
        cliente.Identificacion = identificacion;
        cliente.Direccion = request.Direccion.Trim();
        cliente.Telefono = request.Telefono.Trim();
        cliente.Estado = request.Estado;

        if (!string.IsNullOrWhiteSpace(request.Contrasena))
        {
            cliente.ContrasenaHash =
                _passwordHasher.HashPassword(
                    cliente,
                    request.Contrasena);
        }

        _repository.Actualizar(cliente);
        var evento = CrearEvento(
            cliente,
            ClienteEventTypes.Actualizado,
            ClienteRoutingKeys.Actualizado);

                _repository.Actualizar(cliente);

                await _repository.AgregarEventoAsync(
                    evento,
                    cancellationToken);

        await _repository.GuardarCambiosAsync(cancellationToken);
    }

    public async Task EliminarAsync(
        int clienteId,
        CancellationToken cancellationToken = default)
    {
        var cliente = await _repository.ObtenerPorIdAsync(
            clienteId,
            cancellationToken);

        if (cliente is null)
        {
            throw new KeyNotFoundException(
                $"No se encontró el cliente {clienteId}.");
        }
        var evento = CrearEvento(
            cliente,
            ClienteEventTypes.Eliminado,
            ClienteRoutingKeys.Eliminado,
            estado: false);

        _repository.Eliminar(cliente);

        await _repository.AgregarEventoAsync(
            evento,
            cancellationToken);

        await _repository.GuardarCambiosAsync(cancellationToken);
    }
    private static OutboxMessage CrearEvento(
        Cliente cliente,
        string eventType,
        string routingKey,
        bool? estado = null)
    {
        var eventId = Guid.NewGuid();
        var occurredAt = DateTime.UtcNow;

        var evento = new ClienteIntegrationEvent(
            EventId: eventId,
            EventType: eventType,
            Version: 1,
            OccurredAt: occurredAt,
            Data: new ClienteEventData(
                ClienteId: cliente.Id,
                Nombre: cliente.Nombre,
                Identificacion: cliente.Identificacion,
                Estado: estado ?? cliente.Estado));

        return new OutboxMessage
        {
            Id = eventId,
            EventType = eventType,
            RoutingKey = routingKey,
            Payload = JsonSerializer.Serialize(
                evento,
                JsonOptions),
            OccurredAt = occurredAt
        };
    }
    private static ClienteResponse Mapear(Cliente cliente)
    {
        return new ClienteResponse
        {
            ClienteId = cliente.Id,
            Nombre = cliente.Nombre,
            Edad = cliente.Edad,
            Identificacion = cliente.Identificacion,
            Direccion = cliente.Direccion,
            Telefono = cliente.Telefono,
            Estado = cliente.Estado,
            FechaCreacion = cliente.FechaCreacion
        };
    }
}