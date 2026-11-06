using Banking.Application.Abstracciones;
using Banking.Domain.Entidades;
using Banking.Infrastructure.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace Banking.Infrastructure.Repositorios;

internal sealed class CatalogoRepositorio(BankingDbContext contexto) : ICatalogoRepositorio
{
    public Task<bool> ExisteTipoCuentaAsync(int tipoCuentaId, CancellationToken cancellationToken = default)
    {
        return contexto.TiposCuenta.AnyAsync(tipo => tipo.Id == tipoCuentaId, cancellationToken);
    }

    public Task<bool> ExisteMonedaAsync(int monedaId, CancellationToken cancellationToken = default)
    {
        return contexto.Monedas.AnyAsync(moneda => moneda.Id == monedaId, cancellationToken);
    }

    public async Task<IReadOnlyList<TipoCuenta>> ListarTiposCuentaAsync(CancellationToken cancellationToken = default)
    {
        return await contexto.TiposCuenta.AsNoTracking().OrderBy(tipo => tipo.Id).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<EstadoCuenta>> ListarEstadosCuentaAsync(CancellationToken cancellationToken = default)
    {
        return await contexto.EstadosCuenta.AsNoTracking().OrderBy(estado => estado.Id).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Moneda>> ListarMonedasAsync(CancellationToken cancellationToken = default)
    {
        return await contexto.Monedas.AsNoTracking().OrderBy(moneda => moneda.Id).ToListAsync(cancellationToken);
    }
}
