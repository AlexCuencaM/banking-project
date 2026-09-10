using CuentasAPI.DTOs.Reportes;

namespace CuentasAPI.Services.Interfaces;

public interface IReporteService
{
    Task<IReadOnlyList<ReporteEstadoCuentaResponse>>
        ObtenerEstadoCuentaAsync(
            ReporteEstadoCuentaRequest request,
            CancellationToken cancellationToken = default);
}