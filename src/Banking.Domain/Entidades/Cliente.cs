using Banking.Domain.Comun;
using Banking.Domain.Enumeraciones;
using Banking.Domain.Excepciones;

namespace Banking.Domain.Entidades;

/// <summary>RF-01: registro de clientes.</summary>
public sealed class Cliente
{
    public const int LongitudMaximaNombres = 100;
    public const int LongitudMaximaApellido = 50;

    private Cliente()
    {
    }

    public int Id { get; private set; }

    public string Nombres { get; private set; } = string.Empty;

    public string PrimerApellido { get; private set; } = string.Empty;

    public string? SegundoApellido { get; private set; }

    public DateOnly FechaNacimiento { get; private set; }

    public Genero Genero { get; private set; }

    /// <summary>Expresado siempre en córdobas (NIO), independientemente de la moneda de sus cuentas.</summary>
    public decimal IngresosMensuales { get; private set; }

    /// <summary>Nombres + PrimerApellido + SegundoApellido (si existe). No se persiste.</summary>
    public string NombreCompleto => SegundoApellido is null
        ? $"{Nombres} {PrimerApellido}"
        : $"{Nombres} {PrimerApellido} {SegundoApellido}";

    /// <param name="fechaActual">Fecha actual de referencia (America/Managua), inyectada para poder probar.</param>
    public static Cliente Registrar(
        string nombres,
        string primerApellido,
        string? segundoApellido,
        DateOnly fechaNacimiento,
        Genero genero,
        decimal ingresosMensuales,
        DateOnly fechaActual)
    {
        var nombresValidados = NormalizarObligatorio(nombres, nameof(Nombres), LongitudMaximaNombres);
        var primerApellidoValidado = NormalizarObligatorio(primerApellido, nameof(PrimerApellido), LongitudMaximaApellido);
        var segundoApellidoValidado = NormalizarOpcional(segundoApellido, nameof(SegundoApellido), LongitudMaximaApellido);

        if (fechaNacimiento > fechaActual)
        {
            throw new ValidacionException("La fecha de nacimiento no puede ser posterior a la fecha actual.");
        }

        if (!Enum.IsDefined(genero))
        {
            throw new ValidacionException("El género indicado no es válido.");
        }

        ReglasMonetarias.ValidarImporteNoNegativo(ingresosMensuales, nameof(IngresosMensuales));

        return new Cliente
        {
            Nombres = nombresValidados,
            PrimerApellido = primerApellidoValidado,
            SegundoApellido = segundoApellidoValidado,
            FechaNacimiento = fechaNacimiento,
            Genero = genero,
            IngresosMensuales = ingresosMensuales
        };
    }

    private static string NormalizarObligatorio(string? valor, string campo, int longitudMaxima)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            throw new ValidacionException($"El campo {campo} es obligatorio.");
        }

        return ValidarLongitud(valor.Trim(), campo, longitudMaxima);
    }

    private static string? NormalizarOpcional(string? valor, string campo, int longitudMaxima)
    {
        return string.IsNullOrWhiteSpace(valor)
            ? null
            : ValidarLongitud(valor.Trim(), campo, longitudMaxima);
    }

    private static string ValidarLongitud(string valor, string campo, int longitudMaxima)
    {
        if (valor.Length > longitudMaxima)
        {
            throw new ValidacionException($"El campo {campo} no puede superar {longitudMaxima} caracteres.");
        }

        return valor;
    }
}