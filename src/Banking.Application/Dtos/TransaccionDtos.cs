namespace Banking.Application.Dtos;

/// <summary>RF-04 y RF-05. El monto se expresa en la moneda de la cuenta.</summary>
public sealed record OperacionSolicitud(decimal Monto);

/// <summary>RF-06: identificador, tipo, monto, fecha y hora (UTC) y saldo resultante.</summary>
public sealed record TransaccionRespuesta(
    int Id,
    string NumeroCuenta,
    string Tipo,
    decimal Monto,
    DateTime FechaHora,
    decimal SaldoResultante,
    string Moneda);

public sealed record HistorialRespuesta(
    string NumeroCuenta,
    string Moneda,
    string SimboloMoneda,
    decimal SaldoActual,
    IReadOnlyList<TransaccionRespuesta> Transacciones);
