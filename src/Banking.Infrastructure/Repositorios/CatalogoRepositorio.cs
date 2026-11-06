using Banking.Application.Abstracciones;
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
}
