using CuentasAPI.DTOs.Reportes;
using CuentasAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CuentasAPI.Controllers;

[ApiController]
[Route("reportes")]
public sealed class ReportesController : ControllerBase
{
    private readonly IReporteService _service;

    public ReportesController(IReporteService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(IReadOnlyList<ReporteEstadoCuentaResponse>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<
        ActionResult<IReadOnlyList<ReporteEstadoCuentaResponse>>>
        ObtenerEstadoCuenta(
            [FromQuery] ReporteEstadoCuentaRequest request,
            CancellationToken cancellationToken)
    {
        var reporte =
            await _service.ObtenerEstadoCuentaAsync(
                request,
                cancellationToken);

        return Ok(reporte);
    }
}