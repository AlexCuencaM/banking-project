using CuentasAPI.Data;
using CuentasAPI.Models;
using CuentasAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CuentasAPI.Repositories;

public sealed class ClienteProyeccionRepository
    : IClienteProyeccionRepository
{
    private readonly CuentasDbContext _context;

    public ClienteProyeccionRepository(
        CuentasDbContext context)
    {
        _context = context;
    }

    public Task<ClienteProyeccion?> ObtenerPorIdAsync(
        int clienteId,
        CancellationToken cancellationToken = default)
    {
        return _context.ClientesProyeccion
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.ClienteId == clienteId,
                cancellationToken);
    }
}