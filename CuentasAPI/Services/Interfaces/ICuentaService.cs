using CuentasAPI.DTOs.Cuentas;

namespace CuentasAPI.Services.Interfaces;

public interface ICuentaService
{
    Task<IReadOnlyList<CuentaResponse>> ObtenerTodasAsync(
        CancellationToken cancellationToken = default);

    Task<CuentaResponse> ObtenerPorIdAsync(
        int cuentaId,
        CancellationToken cancellationToken = default);

    Task<CuentaResponse> CrearAsync(
        CrearCuentaRequest request,
        CancellationToken cancellationToken = default);

    Task ActualizarAsync(
        int cuentaId,
        ActualizarCuentaRequest request,
        CancellationToken cancellationToken = default);
}