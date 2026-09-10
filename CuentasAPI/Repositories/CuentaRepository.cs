using CuentasAPI.Data;
using CuentasAPI.Models;
using CuentasAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CuentasAPI.Repositories;

public sealed class CuentaRepository : ICuentaRepository
{
    private readonly CuentasDbContext _context;

    public CuentaRepository(CuentasDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Cuenta>> ObtenerTodasAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Cuentas
            .AsNoTracking()
            .OrderBy(cuenta => cuenta.CuentaId)
            .ToListAsync(cancellationToken);
    }

    public Task<Cuenta?> ObtenerPorIdAsync(
        int cuentaId,
        CancellationToken cancellationToken = default)
    {
        return _context.Cuentas.SingleOrDefaultAsync(
            cuenta => cuenta.CuentaId == cuentaId,
            cancellationToken);
    }

    public Task<bool> ExisteNumeroCuentaAsync(
        string numeroCuenta,
        int? cuentaIdExcluir = null,
        CancellationToken cancellationToken = default)
    {
        return _context.Cuentas.AnyAsync(
            cuenta =>
                cuenta.NumeroCuenta == numeroCuenta &&
                (!cuentaIdExcluir.HasValue ||
                 cuenta.CuentaId != cuentaIdExcluir.Value),
            cancellationToken);
    }

    public async Task AgregarAsync(
        Cuenta cuenta,
        CancellationToken cancellationToken = default)
    {
        await _context.Cuentas.AddAsync(
            cuenta,
            cancellationToken);
    }

    public void Actualizar(Cuenta cuenta)
    {
        _context.Cuentas.Update(cuenta);
    }

    public Task<int> GuardarCambiosAsync(
        CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}