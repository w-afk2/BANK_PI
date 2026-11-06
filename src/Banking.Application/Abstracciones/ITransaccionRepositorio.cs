using Banking.Domain.Entidades;

namespace Banking.Application.Abstracciones;

public interface ITransaccionRepositorio
{
    void Agregar(Transaccion transaccion);

    Task<Transaccion?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Movimientos de la cuenta en orden cronológico ascendente (FechaHora y luego Id).</summary>
    Task<IReadOnlyList<Transaccion>> ObtenerPorCuentaAsync(int cuentaBancariaId, CancellationToken cancellationToken = default);
}
