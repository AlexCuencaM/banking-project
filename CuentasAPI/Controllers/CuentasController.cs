using CuentasAPI.DTOs.Cuentas;
using CuentasAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CuentasAPI.Controllers;

[ApiController]
[Route("cuentas")]
public sealed class CuentasController : ControllerBase
{
    private readonly ICuentaService _service;

    public CuentasController(ICuentaService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(IReadOnlyList<CuentaResponse>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CuentaResponse>>>
        ObtenerTodas(CancellationToken cancellationToken)
    {
        var cuentas =
            await _service.ObtenerTodasAsync(cancellationToken);

        return Ok(cuentas);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(
        typeof(CuentaResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CuentaResponse>> ObtenerPorId(
        int id,
        CancellationToken cancellationToken)
    {
        var cuenta =
            await _service.ObtenerPorIdAsync(id, cancellationToken);

        return Ok(cuenta);
    }

    [HttpPost]
    [ProducesResponseType(
        typeof(CuentaResponse),
        StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CuentaResponse>> Crear(
        CrearCuentaRequest request,
        CancellationToken cancellationToken)
    {
        var cuenta =
            await _service.CrearAsync(request, cancellationToken);

        return CreatedAtAction(
            nameof(ObtenerPorId),
            new { id = cuenta.CuentaId },
            cuenta);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Actualizar(
        int id,
        ActualizarCuentaRequest request,
        CancellationToken cancellationToken)
    {
        await _service.ActualizarAsync(
            id,
            request,
            cancellationToken);

        return NoContent();
    }
}