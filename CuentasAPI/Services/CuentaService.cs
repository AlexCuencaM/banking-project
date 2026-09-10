using CuentasAPI.DTOs.Cuentas;
using CuentasAPI.Models;
using CuentasAPI.Repositories.Interfaces;
using CuentasAPI.Services.Interfaces;

namespace CuentasAPI.Services;

public sealed class CuentaService : ICuentaService
{
    private readonly ICuentaRepository _repository;

    public CuentaService(ICuentaRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<CuentaResponse>> ObtenerTodasAsync(
        CancellationToken cancellationToken = default)
    {
        var cuentas =
            await _repository.ObtenerTodasAsync(cancellationToken);

        return cuentas.Select(Mapear).ToList();
    }

    public async Task<CuentaResponse> ObtenerPorIdAsync(
        int cuentaId,
        CancellationToken cancellationToken = default)
    {
        var cuenta = await _repository.ObtenerPorIdAsync(
            cuentaId,
            cancellationToken);

        if (cuenta is null)
        {
            throw new KeyNotFoundException(
                $"No se encontró la cuenta {cuentaId}.");
        }

        return Mapear(cuenta);
    }

    public async Task<CuentaResponse> CrearAsync(
        CrearCuentaRequest request,
        CancellationToken cancellationToken = default)
    {
        var numeroCuenta = request.NumeroCuenta.Trim();
        if (request.SaldoInicial < 0)
        {
            throw new ArgumentException(
                "El saldo inicial no puede ser negativo.");
        }
        if (await _repository.ExisteNumeroCuentaAsync(
                numeroCuenta,
                cancellationToken: cancellationToken))
        {
            throw new InvalidOperationException(
                $"Ya existe la cuenta número {numeroCuenta}.");
        }

        var cuenta = new Cuenta
        {
            ClienteId = request.ClienteId,
            NumeroCuenta = numeroCuenta,
            TipoCuenta = request.TipoCuenta,
            SaldoInicial = request.SaldoInicial,
            SaldoDisponible = request.SaldoInicial,
            Estado = request.Estado
        };

        await _repository.AgregarAsync(
            cuenta,
            cancellationToken);

        await _repository.GuardarCambiosAsync(cancellationToken);

        return Mapear(cuenta);
    }

    public async Task ActualizarAsync(
        int cuentaId,
        ActualizarCuentaRequest request,
        CancellationToken cancellationToken = default)
    {
        var cuenta = await _repository.ObtenerPorIdAsync(
            cuentaId,
            cancellationToken);

        if (cuenta is null)
        {
            throw new KeyNotFoundException(
                $"No se encontró la cuenta {cuentaId}.");
        }

        var numeroCuenta = request.NumeroCuenta.Trim();

        if (await _repository.ExisteNumeroCuentaAsync(
                numeroCuenta,
                cuentaId,
                cancellationToken))
        {
            throw new InvalidOperationException(
                $"Ya existe otra cuenta número {numeroCuenta}.");
        }

        cuenta.NumeroCuenta = numeroCuenta;
        cuenta.TipoCuenta = request.TipoCuenta;
        cuenta.Estado = request.Estado;

        _repository.Actualizar(cuenta);

        await _repository.GuardarCambiosAsync(cancellationToken);
    }

    private static CuentaResponse Mapear(Cuenta cuenta)
    {
        return new CuentaResponse
        {
            CuentaId = cuenta.CuentaId,
            ClienteId = cuenta.ClienteId,
            NumeroCuenta = cuenta.NumeroCuenta,
            TipoCuenta = cuenta.TipoCuenta,
            SaldoInicial = cuenta.SaldoInicial,
            SaldoDisponible = cuenta.SaldoDisponible,
            Estado = cuenta.Estado
        };
    }
}