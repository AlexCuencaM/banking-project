using CuentasAPI.DTOs.Movimientos;
using CuentasAPI.Exceptions;
using CuentasAPI.Models;
using CuentasAPI.Repositories.Interfaces;
using CuentasAPI.Services.Interfaces;

namespace CuentasAPI.Services;

public sealed class MovimientoService : IMovimientoService
{
    private const decimal SaldoMaximo =
        9999999999999999.99m;

    private readonly IMovimientoRepository _repository;

    public MovimientoService(IMovimientoRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<MovimientoResponse>> ObtenerTodosAsync(
        int? cuentaId = null,
        CancellationToken cancellationToken = default)
    {
        var movimientos = await _repository.ObtenerTodosAsync(
            cuentaId,
            cancellationToken);

        return movimientos.Select(Mapear).ToList();
    }

    public async Task<MovimientoResponse> ObtenerPorIdAsync(
        int movimientoId,
        CancellationToken cancellationToken = default)
    {
        var movimiento = await _repository.ObtenerPorIdAsync(
            movimientoId,
            cancellationToken);

        if (movimiento is null)
        {
            throw new KeyNotFoundException(
                $"No se encontró el movimiento {movimientoId}.");
        }

        return Mapear(movimiento);
    }

    public async Task<MovimientoResponse> CrearAsync(
        CrearMovimientoRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidarValor(request.Valor);

        var cuenta = await _repository.ObtenerCuentaAsync(
            request.CuentaId,
            cancellationToken);

        if (cuenta is null)
        {
            throw new KeyNotFoundException(
                $"No se encontró la cuenta {request.CuentaId}.");
        }

        if (!cuenta.Estado)
        {
            throw new InvalidOperationException(
                "No se pueden registrar movimientos en una cuenta inactiva.");
        }

        var nuevoSaldo = cuenta.SaldoDisponible + request.Valor;
        ValidarSaldo(nuevoSaldo);

        cuenta.SaldoDisponible = nuevoSaldo;

        var movimiento = new Movimiento
        {
            CuentaId = cuenta.CuentaId,
            Fecha = DateTime.UtcNow,
            TipoMovimiento = ObtenerTipo(request.Valor),
            Valor = request.Valor,
            Saldo = nuevoSaldo
        };

        await _repository.AgregarAsync(
            movimiento,
            cancellationToken);

        // EF guarda la actualización de Cuenta y el Movimiento
        // dentro de una misma transacción.
        await _repository.GuardarCambiosAsync(cancellationToken);

        return Mapear(movimiento);
    }

    public async Task<MovimientoResponse> ActualizarParcialAsync(
        int movimientoId,
        ActualizarMovimientoRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!request.Valor.HasValue)
        {
            throw new ArgumentException(
                "Debe enviar el campo valor.");
        }

        var nuevoValor = request.Valor.Value;
        ValidarValor(nuevoValor);

        var movimiento =
            await _repository.ObtenerParaActualizarAsync(
                movimientoId,
                cancellationToken);

        if (movimiento is null)
        {
            throw new KeyNotFoundException(
                $"No se encontró el movimiento {movimientoId}.");
        }

        var diferencia = nuevoValor - movimiento.Valor;

        var movimientosPosteriores =
            await _repository.ObtenerPosterioresAsync(
                movimiento.CuentaId,
                movimiento.Fecha,
                movimiento.MovimientoId,
                cancellationToken);

        ValidarSaldo(movimiento.Saldo + diferencia);

        foreach (var posterior in movimientosPosteriores)
        {
            ValidarSaldo(posterior.Saldo + diferencia);
        }

        var nuevoSaldoDisponible =
            movimiento.Cuenta.SaldoDisponible + diferencia;

        ValidarSaldo(nuevoSaldoDisponible);

        movimiento.Valor = nuevoValor;
        movimiento.TipoMovimiento = ObtenerTipo(nuevoValor);
        movimiento.Saldo += diferencia;

        foreach (var posterior in movimientosPosteriores)
        {
            posterior.Saldo += diferencia;
        }

        movimiento.Cuenta.SaldoDisponible =
            nuevoSaldoDisponible;

        await _repository.GuardarCambiosAsync(cancellationToken);

        return Mapear(movimiento);
    }

    private static void ValidarValor(decimal valor)
    {
        if (valor == 0)
        {
            throw new ArgumentException(
                "El valor del movimiento no puede ser cero.");
        }
    }

    private static void ValidarSaldo(decimal saldo)
    {
        if (saldo < 0)
        {
            throw new SaldoNoDisponibleException();
        }

        if (saldo > SaldoMaximo)
        {
            throw new ArgumentException(
                "El saldo excede el valor máximo permitido.");
        }
    }

    private static string ObtenerTipo(decimal valor)
    {
        return valor > 0 ? "Depósito" : "Retiro";
    }

    private static MovimientoResponse Mapear(Movimiento movimiento)
    {
        return new MovimientoResponse
        {
            MovimientoId = movimiento.MovimientoId,
            CuentaId = movimiento.CuentaId,
            Fecha = movimiento.Fecha,
            TipoMovimiento = movimiento.TipoMovimiento,
            Valor = movimiento.Valor,
            Saldo = movimiento.Saldo
        };
    }
}