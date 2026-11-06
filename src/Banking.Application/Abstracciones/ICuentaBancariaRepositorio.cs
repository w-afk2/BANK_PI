using Banking.Domain.Entidades;

namespace Banking.Application.Abstracciones;

public interface ICuentaBancariaRepositorio
{
    void Agregar(CuentaBancaria cuenta);

    /// <summary>Devuelve la cuenta con su tipo, estado y moneda, lista para modificarse.</summary>
    Task<CuentaBancaria?> ObtenerPorNumeroAsync(string numeroCuenta, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CuentaBancaria>> ObtenerPorClienteAsync(int clienteId, CancellationToken cancellationToken = default);

    /// <summary>Último consecutivo usado en la fecha indicada, o 0 si no hay cuentas ese día.</summary>
    Task<int> ObtenerUltimoConsecutivoAsync(DateOnly fecha, CancellationToken cancellationToken = default);
}
