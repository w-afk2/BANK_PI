using System.Globalization;

namespace Banking.Domain.Comun;

/// <summary>
/// Formato ACC-YYYYMMDD-XXXX (17 caracteres). La fecha corresponde a America/Managua
/// y XXXX es un consecutivo diario de 0001 a 9999.
/// </summary>
public static class FormatoNumeroCuenta
{
    public const string Prefijo = "ACC-";
    public const string FormatoFecha = "yyyyMMdd";
    public const int Longitud = 17;
    public const int ConsecutivoMinimo = 1;
    public const int ConsecutivoMaximo = 9999;

    private const int InicioFecha = 4;
    private const int LongitudFecha = 8;
    private const int PosicionSeparador = 12;
    private const int InicioConsecutivo = 13;
    private const int LongitudConsecutivo = 4;

    public static string Formatear(DateOnly fecha, int consecutivo)
    {
        if (consecutivo is < ConsecutivoMinimo or > ConsecutivoMaximo)
        {
            throw new ArgumentOutOfRangeException(
                nameof(consecutivo),
                $"El consecutivo debe estar entre {ConsecutivoMinimo} y {ConsecutivoMaximo}.");
        }

        var fechaTexto = fecha.ToString(FormatoFecha, CultureInfo.InvariantCulture);
        return $"{Prefijo}{fechaTexto}-{consecutivo.ToString("D4", CultureInfo.InvariantCulture)}";
    }

    public static bool EsValido(string? numeroCuenta)
    {
        if (numeroCuenta is null || numeroCuenta.Length != Longitud)
        {
            return false;
        }

        if (!numeroCuenta.StartsWith(Prefijo, StringComparison.Ordinal)
            || numeroCuenta[PosicionSeparador] != '-')
        {
            return false;
        }

        var fechaValida = DateOnly.TryParseExact(
            numeroCuenta.Substring(InicioFecha, LongitudFecha),
            FormatoFecha,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out _);

        var consecutivo = numeroCuenta.Substring(InicioConsecutivo, LongitudConsecutivo);

        // Solo se admiten 0001 a 9999: cuatro dígitos y un valor mínimo de 1.
        return fechaValida
            && consecutivo.All(char.IsAsciiDigit)
            && int.Parse(consecutivo, CultureInfo.InvariantCulture) >= ConsecutivoMinimo;
    }
}
