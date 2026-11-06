namespace Banking.Application.Abstracciones;

public interface ICatalogoRepositorio
{
    Task<bool> ExisteTipoCuentaAsync(int tipoCuentaId, CancellationToken cancellationToken = default);

    Task<bool> ExisteMonedaAsync(int monedaId, CancellationToken cancellationToken = default);
}
