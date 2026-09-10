using CuentasAPI.DTOs.Movimientos;

namespace CuentasAPI.Services.Interfaces;

public interface IMovimientoService
{
    Task<IReadOnlyList<MovimientoResponse>> ObtenerTodosAsync(
        int? cuentaId = null,
        CancellationToken cancellationToken = default);

    Task<MovimientoResponse> ObtenerPorIdAsync(
        int movimientoId,
        CancellationToken cancellationToken = default);

    Task<MovimientoResponse> CrearAsync(
        CrearMovimientoRequest request,
        CancellationToken cancellationToken = default);

    Task<MovimientoResponse> ActualizarParcialAsync(
        int movimientoId,
        ActualizarMovimientoRequest request,
        CancellationToken cancellationToken = default);
}