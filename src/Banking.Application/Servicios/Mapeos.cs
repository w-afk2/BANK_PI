using Banking.Application.Dtos;
using Banking.Domain.Entidades;

namespace Banking.Application.Servicios;

/// <summary>Conversión de entidades a DTOs de respuesta. Nunca se exponen entidades completas.</summary>
internal static class Mapeos
{
    /// <summary>Los ingresos mensuales del cliente siempre se expresan en córdobas.</summary>
    public const string MonedaIngresos = "NIO";

    public static ClienteRespuesta ARespuesta(this Cliente cliente, IReadOnlyList<CuentaRespuesta> cuentas)
    {
        return new ClienteRespuesta(
            cliente.Id,
            cliente.Nombres,
            cliente.PrimerApellido,
            cliente.SegundoApellido,
            cliente.NombreCompleto,
            cliente.FechaNacimiento,
            cliente.Genero.ToString(),
            cliente.IngresosMensuales,
            MonedaIngresos,
            cuentas);
    }

    public static CuentaRespuesta ARespuesta(this CuentaBancaria cuenta)
    {
        return new CuentaRespuesta(
            cuenta.Id,
            cuenta.NumeroCuenta,
            cuenta.ClienteId,
            cuenta.TipoCuenta?.Nombre ?? string.Empty,
            cuenta.EstadoCuenta?.Nombre ?? string.Empty,
            cuenta.Moneda?.Codigo ?? string.Empty,
            cuenta.Moneda?.Simbolo ?? string.Empty,
            cuenta.Saldo,
            cuenta.FechaCreacion);
    }

    public static SaldoRespuesta ARespuestaSaldo(this CuentaBancaria cuenta)
    {
        return new SaldoRespuesta(
            cuenta.NumeroCuenta,
            cuenta.Saldo,
            cuenta.Moneda?.Codigo ?? string.Empty,
            cuenta.Moneda?.Simbolo ?? string.Empty,
            cuenta.EstadoCuenta?.Nombre ?? string.Empty);
    }

    public static TransaccionRespuesta ARespuesta(this Transaccion transaccion, CuentaBancaria cuenta)
    {
        return new TransaccionRespuesta(
            transaccion.Id,
            cuenta.NumeroCuenta,
            transaccion.TipoTransaccion?.Nombre ?? string.Empty,
            transaccion.Monto,
            transaccion.FechaHora,
            transaccion.SaldoResultante,
            cuenta.Moneda?.Codigo ?? string.Empty);
    }

    public static AuditoriaRespuesta ARespuesta(this Auditoria auditoria)
    {
        return new AuditoriaRespuesta(
            auditoria.Id,
            auditoria.FechaHora,
            auditoria.Accion.ToString(),
            auditoria.Resultado.ToString(),
            auditoria.Descripcion,
            auditoria.ClienteId,
            auditoria.CuentaBancaria?.NumeroCuenta);
    }

    public static CatalogoRespuesta ARespuesta(this TipoCuenta tipo) => new(tipo.Id, tipo.Nombre);

    public static CatalogoRespuesta ARespuesta(this EstadoCuenta estado) => new(estado.Id, estado.Nombre);

    public static MonedaRespuesta ARespuesta(this Moneda moneda) =>
        new(moneda.Id, moneda.Codigo, moneda.Nombre, moneda.Simbolo);
}
