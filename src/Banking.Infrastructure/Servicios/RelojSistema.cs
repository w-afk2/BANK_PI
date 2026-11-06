using Banking.Application.Abstracciones;

namespace Banking.Infrastructure.Servicios;

internal sealed class RelojSistema : IRelojSistema
{
    private static readonly TimeZoneInfo ZonaManagua = ObtenerZonaManagua();

    public DateTime AhoraUtc => DateTime.UtcNow;

    public DateOnly FechaActualManagua =>
        DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, ZonaManagua));

    /// <summary>
    /// "America/Managua" es el identificador IANA (Linux, macOS y Windows con ICU).
    /// "Central America Standard Time" es el equivalente de Windows, usado como respaldo.
    /// </summary>
    private static TimeZoneInfo ObtenerZonaManagua()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("America/Managua");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Central America Standard Time");
        }
    }
}
