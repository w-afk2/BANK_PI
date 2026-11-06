using Banking.Domain.Comun;
using Banking.Domain.Enumeraciones;

namespace Banking.Domain.Entidades;

/// <summary>
/// Evento del sistema (exitoso o rechazado). Es independiente de Transaccion,
/// que representa los movimientos financieros.
/// </summary>
public sealed class Auditoria
{
    public const int LongitudMaximaDescripcion = 500;

    /// <summary>Se agrega al final si una descripción interna excede el máximo, para que el recorte sea visible.</summary>
    public const string MarcaRecorte = " [descripción recortada]";

    private Auditoria()
    {
    }

    public int Id { get; private set; }

    public int? ClienteId { get; private set; }

    public int? CuentaBancariaId { get; private set; }

    public AccionAuditoria Accion { get; private set; }

    public ResultadoAuditoria Resultado { get; private set; }

    /// <summary>Mensaje breve y controlado; incluye el motivo en los rechazos. Sin información sensible.</summary>
    public string Descripcion { get; private set; } = string.Empty;

    /// <summary>UTC.</summary>
    public DateTime FechaHora { get; private set; }

    public Cliente? Cliente { get; private set; }

    public CuentaBancaria? CuentaBancaria { get; private set; }

    /// <summary>
    /// Para eventos exitosos, guardados en la misma transacción de base de datos que la operación.
    /// Usa las navegaciones para que EF Core asigne las FK de entidades recién creadas.
    /// </summary>
    public static Auditoria RegistrarConEntidades(
        AccionAuditoria accion,
        ResultadoAuditoria resultado,
        string descripcion,
        DateTime fechaHoraUtc,
        Cliente? cliente = null,
        CuentaBancaria? cuenta = null)
    {
        var auditoria = Crear(accion, resultado, descripcion, fechaHoraUtc);
        auditoria.Cliente = cliente;
        auditoria.CuentaBancaria = cuenta;
        auditoria.ClienteId = cliente?.Id ?? cuenta?.ClienteId;
        auditoria.CuentaBancariaId = cuenta?.Id;
        return auditoria;
    }

    /// <summary>
    /// Para eventos rechazados, escritos de forma independiente después de descartar los cambios.
    /// Las FK quedan en null cuando el cliente o la cuenta no existen.
    /// </summary>
    public static Auditoria RegistrarConIdentificadores(
        AccionAuditoria accion,
        ResultadoAuditoria resultado,
        string descripcion,
        DateTime fechaHoraUtc,
        int? clienteId = null,
        int? cuentaBancariaId = null)
    {
        var auditoria = Crear(accion, resultado, descripcion, fechaHoraUtc);
        auditoria.ClienteId = clienteId;
        auditoria.CuentaBancariaId = cuentaBancariaId;
        return auditoria;
    }

    private static Auditoria Crear(
        AccionAuditoria accion,
        ResultadoAuditoria resultado,
        string descripcion,
        DateTime fechaHoraUtc)
    {
        if (string.IsNullOrWhiteSpace(descripcion))
        {
            throw new ArgumentException("La descripción de auditoría es obligatoria.", nameof(descripcion));
        }

        ReglasFecha.AsegurarUtc(fechaHoraUtc, nameof(fechaHoraUtc));

        return new Auditoria
        {
            Accion = accion,
            Resultado = resultado,
            Descripcion = AjustarDescripcion(descripcion.Trim()),
            FechaHora = fechaHoraUtc
        };
    }

    /// <summary>
    /// Las descripciones son internas y breves. Si excepcionalmente superan el límite, se conserva
    /// el inicio (donde va el motivo) y se marca explícitamente que fue recortada.
    /// </summary>
    private static string AjustarDescripcion(string descripcion)
    {
        if (descripcion.Length <= LongitudMaximaDescripcion)
        {
            return descripcion;
        }

        var longitudConservada = LongitudMaximaDescripcion - MarcaRecorte.Length;
        return descripcion[..longitudConservada].TrimEnd() + MarcaRecorte;
    }
}