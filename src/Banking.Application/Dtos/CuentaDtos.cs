namespace Banking.Application.Dtos;

/// <summary>RF-02. El número de cuenta se genera automáticamente.</summary>
public sealed record CrearCuentaSolicitud(
    int ClienteId,
    int TipoCuentaId,
    int MonedaId,
    decimal SaldoInicial);

public sealed record CuentaRespuesta(
    int Id,
    string NumeroCuenta,
    int ClienteId,
    string TipoCuenta,
    string Estado,
    string Moneda,
    string SimboloMoneda,
    decimal Saldo,
    DateTime FechaCreacion);

/// <summary>RF-03.</summary>
public sealed record SaldoRespuesta(
    string NumeroCuenta,
    decimal Saldo,
    string Moneda,
    string SimboloMoneda,
    string Estado);
