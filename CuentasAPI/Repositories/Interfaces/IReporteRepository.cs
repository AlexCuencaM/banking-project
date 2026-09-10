using CuentasAPI.Models;

namespace CuentasAPI.Repositories.Interfaces;

public interface IReporteRepository
{
    Task<IReadOnlyList<ReporteEstadoCuentaItem>>
        ObtenerEstadoCuentaAsync(
            int clienteId,
            DateTime fechaInicio,
            DateTime fechaFinExclusiva,
            CancellationToken cancellationToken = default);
}