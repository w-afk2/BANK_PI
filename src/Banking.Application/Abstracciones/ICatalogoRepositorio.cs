using Banking.Domain.Entidades;

namespace Banking.Application.Abstracciones;

public interface ICatalogoRepositorio
{
    Task<bool> ExisteTipoCuentaAsync(int tipoCuentaId, CancellationToken cancellationToken = default);

    Task<bool> ExisteMonedaAsync(int monedaId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TipoCuenta>> ListarTiposCuentaAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EstadoCuenta>> ListarEstadosCuentaAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Moneda>> ListarMonedasAsync(CancellationToken cancellationToken = default);
}
