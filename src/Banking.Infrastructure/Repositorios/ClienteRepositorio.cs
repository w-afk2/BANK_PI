using Banking.Application.Abstracciones;
using Banking.Domain.Entidades;
using Banking.Infrastructure.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace Banking.Infrastructure.Repositorios;

internal sealed class ClienteRepositorio(BankingDbContext contexto) : IClienteRepositorio
{
    public void Agregar(Cliente cliente) => contexto.Clientes.Add(cliente);

    public Task<Cliente?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return contexto.Clientes
            .AsNoTracking()
            .FirstOrDefaultAsync(cliente => cliente.Id == id, cancellationToken);
    }

    public Task<bool> ExisteAsync(int id, CancellationToken cancellationToken = default)
    {
        return contexto.Clientes.AnyAsync(cliente => cliente.Id == id, cancellationToken);
    }
}
