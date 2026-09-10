using CuentasAPI.Data;
using CuentasAPI.Models;
using CuentasAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CuentasAPI.Repositories;

public sealed class ReporteRepository : IReporteRepository
{
    private readonly CuentasDbContext _context;

    public ReporteRepository(CuentasDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<ReporteEstadoCuentaItem>>
        ObtenerEstadoCuentaAsync(
            int clienteId,
            DateTime fechaInicio,
            DateTime fechaFinExclusiva,
            CancellationToken cancellationToken = default)
    {
        return await (
            from cuenta in _context.Cuentas.AsNoTracking()
            join movimiento in _context.Movimientos.AsNoTracking()
                on cuenta.CuentaId equals movimiento.CuentaId
            where cuenta.ClienteId == clienteId
                  && movimiento.Fecha >= fechaInicio
                  && movimiento.Fecha < fechaFinExclusiva
            orderby movimiento.Fecha, movimiento.MovimientoId
            select new ReporteEstadoCuentaItem
            {
                Fecha = movimiento.Fecha,
                NumeroCuenta = cuenta.NumeroCuenta,
                Tipo = cuenta.TipoCuenta,
                SaldoInicial = cuenta.SaldoInicial,
                Estado = cuenta.Estado,
                Movimiento = movimiento.Valor,
                SaldoDisponible = movimiento.Saldo
            })
            .ToListAsync(cancellationToken);
    }
}