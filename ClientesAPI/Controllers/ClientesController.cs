using ClientesAPI.DTOs.Clientes;
using ClientesAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ClientesAPI.Controllers;

[ApiController]
[Route("clientes")]
public sealed class ClientesController : ControllerBase
{
    private readonly IClienteService _service;

    public ClientesController(IClienteService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(IReadOnlyList<ClienteResponse>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ClienteResponse>>>
        ObtenerTodos(CancellationToken cancellationToken)
    {
        var clientes =
            await _service.ObtenerTodosAsync(cancellationToken);

        return Ok(clientes);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(
        typeof(ClienteResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClienteResponse>> ObtenerPorId(
        int id,
        CancellationToken cancellationToken)
    {
        var cliente =
            await _service.ObtenerPorIdAsync(id, cancellationToken);

        return Ok(cliente);
    }

    [HttpPost]
    [ProducesResponseType(
        typeof(ClienteResponse),
        StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ClienteResponse>> Crear(
        CrearClienteRequest request,
        CancellationToken cancellationToken)
    {
        var cliente =
            await _service.CrearAsync(request, cancellationToken);

        return CreatedAtAction(
            nameof(ObtenerPorId),
            new { id = cliente.ClienteId },
            cliente);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Actualizar(
        int id,
        ActualizarClienteRequest request,
        CancellationToken cancellationToken)
    {
        await _service.ActualizarAsync(
            id,
            request,
            cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Eliminar(
        int id,
        CancellationToken cancellationToken)
    {
        await _service.EliminarAsync(id, cancellationToken);

        return NoContent();
    }
}