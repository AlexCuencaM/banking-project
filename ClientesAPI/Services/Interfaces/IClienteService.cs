using ClientesAPI.DTOs.Clientes;

namespace ClientesAPI.Services.Interfaces;

public interface IClienteService
{
    Task<IReadOnlyList<ClienteResponse>> ObtenerTodosAsync(
        CancellationToken cancellationToken = default);

    Task<ClienteResponse> ObtenerPorIdAsync(
        int clienteId,
        CancellationToken cancellationToken = default);

    Task<ClienteResponse> CrearAsync(
        CrearClienteRequest request,
        CancellationToken cancellationToken = default);

    Task ActualizarAsync(
        int clienteId,
        ActualizarClienteRequest request,
        CancellationToken cancellationToken = default);

    Task EliminarAsync(
        int clienteId,
        CancellationToken cancellationToken = default);
}