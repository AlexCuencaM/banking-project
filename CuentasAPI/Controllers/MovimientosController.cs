using CuentasAPI.DTOs.Movimientos;
using CuentasAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CuentasAPI.Controllers;

[ApiController]
[Route("movimientos")]
public sealed class MovimientosController : ControllerBase
{
    private readonly IMovimientoService _service;

    public MovimientosController(IMovimientoService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<MovimientoResponse>>>
        ObtenerTodos(
            [FromQuery] int? cuentaId,
            CancellationToken cancellationToken)
    {
        var movimientos = await _service.ObtenerTodosAsync(
            cuentaId,
            cancellationToken);

        return Ok(movimientos);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<MovimientoResponse>> ObtenerPorId(
        int id,
        CancellationToken cancellationToken)
    {
        return Ok(await _service.ObtenerPorIdAsync(
            id,
            cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<MovimientoResponse>> Crear(
        CrearMovimientoRequest request,
        CancellationToken cancellationToken)
    {
        var movimiento = await _service.CrearAsync(
            request,
            cancellationToken);

        return CreatedAtAction(
            nameof(ObtenerPorId),
            new { id = movimiento.MovimientoId },
            movimiento);
    }

    [HttpPatch("{id:int}")]
    public async Task<ActionResult<MovimientoResponse>> ActualizarParcial(
        int id,
        ActualizarMovimientoRequest request,
        CancellationToken cancellationToken)
    {
        var movimiento =
            await _service.ActualizarParcialAsync(
                id,
                request,
                cancellationToken);

        return Ok(movimiento);
    }
}