using Banking.Domain.Comun;
using Banking.Domain.Excepciones;

namespace Banking.Domain.Entidades;

/// <summary>
/// RF-02 a RF-05. Contiene las reglas de saldo y estado; no depende de la persistencia.
/// Saldo y EstadoCuentaId se configuran como tokens de concurrencia optimista.
/// </summary>
public sealed class CuentaBancaria
{
    private CuentaBancaria()
    {
    }

    public int Id { get; private set; }

    public int ClienteId { get; private set; }

    public int TipoCuentaId { get; private set; }

    public int EstadoCuentaId { get; private set; }

    public int MonedaId { get; private set; }

    public string NumeroCuenta { get; private set; } = string.Empty;

    /// <summary>Expresado en la moneda de la cuenta.</summary>
    public decimal Saldo { get; private set; }

    /// <summary>UTC.</summary>
    public DateTime FechaCreacion { get; private set; }

    public Cliente? Cliente { get; private set; }

    public TipoCuenta? TipoCuenta { get; private set; }

    public EstadoCuenta? EstadoCuenta { get; private set; }

    public Moneda? Moneda { get; private set; }

    public bool EstaActiva => EstadoCuentaId == EstadoCuenta.Ids.Activa;

    /// <summary>
    /// Abre una cuenta activa. Si el saldo inicial es mayor que cero, se registra un depósito
    /// por ese importe; si es cero, no se genera transacción.
    /// </summary>
    public static AperturaCuenta Abrir(
        int clienteId,
        int tipoCuentaId,
        int monedaId,
        string numeroCuenta,
        decimal saldoInicial,
        DateTime fechaCreacionUtc)
    {
        if (saldoInicial < 0)
        {
            throw new SaldoInicialInvalidoException();
        }

        ReglasMonetarias.ValidarFormato(saldoInicial, "SaldoInicial");
        ReglasFecha.AsegurarUtc(fechaCreacionUtc, nameof(fechaCreacionUtc));

        if (!FormatoNumeroCuenta.EsValido(numeroCuenta))
        {
            throw new ArgumentException("El número de cuenta no tiene el formato ACC-YYYYMMDD-XXXX.", nameof(numeroCuenta));
        }

        var cuenta = new CuentaBancaria
        {
            ClienteId = clienteId,
            TipoCuentaId = tipoCuentaId,
            MonedaId = monedaId,
            EstadoCuentaId = EstadoCuenta.Ids.Activa,
            NumeroCuenta = numeroCuenta,
            Saldo = 0m,
            FechaCreacion = fechaCreacionUtc
        };

        var depositoInicial = saldoInicial > 0
            ? cuenta.Depositar(saldoInicial, fechaCreacionUtc)
            : null;

        return new AperturaCuenta(cuenta, depositoInicial);
    }

    /// <summary>RF-04. Incrementa el saldo y devuelve la transacción con el saldo resultante.</summary>
    public Transaccion Depositar(decimal monto, DateTime fechaHoraUtc)
    {
        ReglasMonetarias.ValidarMontoOperacion(monto);
        ReglasFecha.AsegurarUtc(fechaHoraUtc, nameof(fechaHoraUtc));
        AsegurarActiva();

        var nuevoSaldo = Saldo + monto;
        if (nuevoSaldo > ReglasMonetarias.ImporteMaximo)
        {
            throw new MontoInvalidoException("El depósito excede el saldo máximo permitido.");
        }

        Saldo = nuevoSaldo;
        return Transaccion.Registrar(this, TipoTransaccion.Ids.Deposito, monto, fechaHoraUtc);
    }

    /// <summary>
    /// RF-05. Disminuye el saldo. Si no hay fondos suficientes, se rechaza sin modificar el saldo.
    /// </summary>
    public Transaccion Retirar(decimal monto, DateTime fechaHoraUtc)
    {
        ReglasMonetarias.ValidarMontoOperacion(monto);
        ReglasFecha.AsegurarUtc(fechaHoraUtc, nameof(fechaHoraUtc));
        AsegurarActiva();

        if (monto > Saldo)
        {
            throw new FondosInsuficientesException();
        }

        Saldo -= monto;
        return Transaccion.Registrar(this, TipoTransaccion.Ids.Retiro, monto, fechaHoraUtc);
    }

    /// <summary>
    /// Funcionalidad adicional: cambiar entre Activa e Inactiva.
    /// Devuelve false si la cuenta ya estaba en ese estado: no hay cambio real ni se debe auditar.
    /// </summary>
    public bool CambiarEstado(int estadoCuentaId)
    {
        if (estadoCuentaId is not (EstadoCuenta.Ids.Activa or EstadoCuenta.Ids.Inactiva))
        {
            throw new ValidacionException("El estado de cuenta indicado no es válido.");
        }

        if (EstadoCuentaId == estadoCuentaId)
        {
            return false;
        }

        EstadoCuentaId = estadoCuentaId;
        return true;
    }

    private void AsegurarActiva()
    {
        if (!EstaActiva)
        {
            throw new CuentaInactivaException();
        }
    }
}