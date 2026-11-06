namespace Banking.Domain.Entidades;

/// <summary>
/// RF-06: movimiento financiero. Solo se crea desde CuentaBancaria, para que el saldo resultante
/// coincida siempre con el saldo de la cuenta tras la operación.
/// </summary>
public sealed class Transaccion
{
    private Transaccion()
    {
    }

    public int Id { get; private set; }

    public int CuentaBancariaId { get; private set; }

    public int TipoTransaccionId { get; private set; }

    public decimal Monto { get; private set; }

    /// <summary>UTC.</summary>
    public DateTime FechaHora { get; private set; }

    public decimal SaldoResultante { get; private set; }

    public CuentaBancaria? CuentaBancaria { get; private set; }

    public TipoTransaccion? TipoTransaccion { get; private set; }

    internal static Transaccion Registrar(
        CuentaBancaria cuenta,
        int tipoTransaccionId,
        decimal monto,
        DateTime fechaHoraUtc)
    {
        return new Transaccion
        {
            // La navegación permite que EF Core asigne la FK cuando la cuenta es nueva (Id aún en 0).
            CuentaBancaria = cuenta,
            CuentaBancariaId = cuenta.Id,
            TipoTransaccionId = tipoTransaccionId,
            Monto = monto,
            FechaHora = fechaHoraUtc,
            SaldoResultante = cuenta.Saldo
        };
    }
}