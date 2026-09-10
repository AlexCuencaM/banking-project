using CuentasAPI.Data;
using CuentasAPI.Models;
using CuentasAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CuentasAPI.Repositories;

public sealed class MovimientoRepository : IMovimientoRepository
{
    private readonly CuentasDbContext _context;

    public MovimientoRepository(CuentasDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Movimiento>> ObtenerTodosAsync(
        int? cuentaId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Movimientos
            .AsNoTracking()
            .AsQueryable();

        if (cuentaId.HasValue)
        {
            query = query.Where(x => x.CuentaId == cuentaId.Value);
        }

        return await query
            .OrderByDescending(x => x.Fecha)
            .ThenByDescending(x => x.MovimientoId)
            .ToListAsync(cancellationToken);
    }

    public Task<Movimiento?> ObtenerPorIdAsync(
        int movimientoId,
        CancellationToken cancellationToken = default)
    {
        return _context.Movimientos
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.MovimientoId == movimientoId,
                cancellationToken);
    }

    public Task<Cuenta?> ObtenerCuentaAsync(
        int cuentaId,
        CancellationToken cancellationToken = default)
    {
        return _context.Cuentas.SingleOrDefaultAsync(
            x => x.CuentaId == cuentaId,
            cancellationToken);
    }

    public Task<Movimiento?> ObtenerParaActualizarAsync(
        int movimientoId,
        CancellationToken cancellationToken = default)
    {
        return _context.Movimientos
            .Include(x => x.Cuenta)
            .SingleOrDefaultAsync(
                x => x.MovimientoId == movimientoId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Movimiento>> ObtenerPosterioresAsync(
        int cuentaId,
        DateTime fecha,
        int movimientoId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Movimientos
            .Where(x =>
                x.CuentaId == cuentaId &&
                (x.Fecha > fecha ||
                 (x.Fecha == fecha &&
                  x.MovimientoId > movimientoId)))
            .OrderBy(x => x.Fecha)
            .ThenBy(x => x.MovimientoId)
            .ToListAsync(cancellationToken);
    }

    public async Task AgregarAsync(
        Movimiento movimiento,
        CancellationToken cancellationToken = default)
    {
        await _context.Movimientos.AddAsync(
            movimiento,
            cancellationToken);
    }

    public Task<int> GuardarCambiosAsync(
        CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}