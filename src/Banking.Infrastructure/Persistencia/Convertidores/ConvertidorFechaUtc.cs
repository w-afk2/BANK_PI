using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Banking.Infrastructure.Persistencia.Convertidores;

/// <summary>
/// SQLite no conserva DateTimeKind: al leer, EF Core devuelve Unspecified.
/// Todas las fechas se guardan en UTC, así que se restaura DateTimeKind.Utc al materializar.
/// </summary>
public sealed class ConvertidorFechaUtc : ValueConverter<DateTime, DateTime>
{
    public ConvertidorFechaUtc()
        : base(fecha => fecha, fecha => DateTime.SpecifyKind(fecha, DateTimeKind.Utc))
    {
    }
}