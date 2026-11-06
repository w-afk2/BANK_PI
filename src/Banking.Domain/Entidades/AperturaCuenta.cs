namespace Banking.Domain.Entidades;

/// <summary>
/// Resultado de abrir una cuenta. DepositoInicial existe solo si el saldo inicial es mayor que cero.
/// </summary>
public sealed record AperturaCuenta(CuentaBancaria Cuenta, Transaccion? DepositoInicial);
