using CuentasAPI.Models;

namespace CuentasAPI.Repositories.Interfaces;

public interface IMovimientoRepository
{
    Task<IReadOnlyList<Movimiento>> ObtenerTodosAsync(
        int? cuentaId = null,
        CancellationToken cancellationToken = default);

    Task<Movimiento?> ObtenerPorIdAsync(
        int movimientoId,
        CancellationToken cancellationToken = default);

    Task<Cuenta?> ObtenerCuentaAsync(
        int cuentaId,
        CancellationToken cancellationToken = default);

    Task<Movimiento?> ObtenerParaActualizarAsync(
        int movimientoId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Movimiento>> ObtenerPosterioresAsync(
        int cuentaId,
        DateTime fecha,
        int movimientoId,
        CancellationToken cancellationToken = default);

    Task AgregarAsync(
        Movimiento movimiento,
        CancellationToken cancellationToken = default);

    Task<int> GuardarCambiosAsync(
        CancellationToken cancellationToken = default);
}