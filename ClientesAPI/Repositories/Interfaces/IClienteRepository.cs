using ClientesAPI.Models;
using ClientesAPI.Messaging.Outbox;
namespace ClientesAPI.Repositories.Interfaces;

public interface IClienteRepository
{
    Task<IReadOnlyList<Cliente>> ObtenerTodosAsync(
        CancellationToken cancellationToken = default);

    Task<Cliente?> ObtenerPorIdAsync(
        int clienteId,
        CancellationToken cancellationToken = default);

    Task<bool> ExisteIdentificacionAsync(
        string identificacion,
        int? clienteIdExcluir = null,
        CancellationToken cancellationToken = default);

    Task CrearConEventoAsync(
        Cliente cliente,
        Func<Cliente, OutboxMessage> construirEvento,
        CancellationToken cancellationToken = default);

    Task AgregarEventoAsync(
        OutboxMessage mensaje,
        CancellationToken cancellationToken = default);

    void Actualizar(Cliente cliente);

    void Eliminar(Cliente cliente);

    Task<int> GuardarCambiosAsync(
        CancellationToken cancellationToken = default);
}
