using ClientesAPI.Data;
using ClientesAPI.Models;
using ClientesAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClientesAPI.Repositories;

public sealed class ClienteRepository : IClienteRepository
{
    private readonly ClientesDbContext _context;

    public ClienteRepository(ClientesDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Cliente>> ObtenerTodosAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Clientes
            .AsNoTracking()
            .OrderBy(cliente => cliente.Id)
            .ToListAsync(cancellationToken);
    }

    public Task<Cliente?> ObtenerPorIdAsync(
        int clienteId,
        CancellationToken cancellationToken = default)
    {
        return _context.Clientes
            .SingleOrDefaultAsync(
                cliente => cliente.Id == clienteId,
                cancellationToken);
    }

    public Task<bool> ExisteIdentificacionAsync(
        string identificacion,
        int? clienteIdExcluir = null,
        CancellationToken cancellationToken = default)
    {
        return _context.Clientes.AnyAsync(
            cliente =>
                cliente.Identificacion == identificacion &&
                (!clienteIdExcluir.HasValue ||
                 cliente.Id != clienteIdExcluir.Value),
            cancellationToken);
    }

    public async Task AgregarAsync(
        Cliente cliente,
        CancellationToken cancellationToken = default)
    {
        await _context.Clientes.AddAsync(
            cliente,
            cancellationToken);
    }

    public void Actualizar(Cliente cliente)
    {
        _context.Clientes.Update(cliente);
    }

    public void Eliminar(Cliente cliente)
    {
        _context.Clientes.Remove(cliente);
    }

    public Task<int> GuardarCambiosAsync(
        CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}