using CuentasAPI.Models;

namespace CuentasAPI.Repositories.Interfaces;

public interface ICuentaRepository
{
    Task<IReadOnlyList<Cuenta>> ObtenerTodasAsync(
        CancellationToken cancellationToken = default);

    Task<Cuenta?> ObtenerPorIdAsync(
        int cuentaId,
        CancellationToken cancellationToken = default);

    Task<bool> ExisteNumeroCuentaAsync(
        string numeroCuenta,
        int? cuentaIdExcluir = null,
        CancellationToken cancellationToken = default);

    Task AgregarAsync(
        Cuenta cuenta,
        CancellationToken cancellationToken = default);

    void Actualizar(Cuenta cuenta);

    Task<int> GuardarCambiosAsync(
        CancellationToken cancellationToken = default);
}