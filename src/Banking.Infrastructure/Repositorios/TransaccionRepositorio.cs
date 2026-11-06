using Banking.Application.Abstracciones;
using Banking.Domain.Entidades;
using Banking.Infrastructure.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace Banking.Infrastructure.Repositorios;

internal sealed class TransaccionRepositorio(BankingDbContext contexto) : ITransaccionRepositorio
{
    public void Agregar(Transaccion transaccion) => contexto.Transacciones.Add(transaccion);

    public Task<Transaccion?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return contexto.Transacciones
            .AsNoTracking()
            .Include(transaccion => transaccion.TipoTransaccion)
            .FirstOrDefaultAsync(transaccion => transaccion.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Transaccion>> ObtenerPorCuentaAsync(
        int cuentaBancariaId,
        CancellationToken cancellationToken = default)
    {
        return await contexto.Transacciones
            .AsNoTracking()
            .Include(transaccion => transaccion.TipoTransaccion)
            .Where(transaccion => transaccion.CuentaBancariaId == cuentaBancariaId)
            .OrderBy(transaccion => transaccion.FechaHora)
            .ThenBy(transaccion => transaccion.Id)
            .ToListAsync(cancellationToken);
    }
}
