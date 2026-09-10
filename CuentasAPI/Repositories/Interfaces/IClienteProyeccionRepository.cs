using CuentasAPI.Models;

namespace CuentasAPI.Repositories.Interfaces;

public interface IClienteProyeccionRepository
{
    Task<ClienteProyeccion?> ObtenerPorIdAsync(
        int clienteId,
        CancellationToken cancellationToken = default);
}