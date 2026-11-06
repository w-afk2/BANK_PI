namespace Banking.Domain.Comun;

internal static class ReglasFecha
{
    /// <summary>Las fechas de operaciones y auditoría se manejan siempre en UTC.</summary>
    public static void AsegurarUtc(DateTime fecha, string nombreParametro)
    {
        if (fecha.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException("La fecha debe estar expresada en UTC.", nombreParametro);
        }
    }
}