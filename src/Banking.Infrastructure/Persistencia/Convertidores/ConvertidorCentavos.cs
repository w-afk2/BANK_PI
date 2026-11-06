using Banking.Domain.Comun;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Banking.Infrastructure.Persistencia.Convertidores;

/// <summary>
/// Almacena importes decimal como INTEGER (centavos, 64 bits) en SQLite.
/// Evita que SQLite compare u ordene importes como texto y elimina errores de representación.
/// </summary>
public sealed class ConvertidorCentavos : ValueConverter<decimal, long>
{
    private const decimal FactorCentavos = 100m;

    public ConvertidorCentavos()
        : base(importe => ACentavos(importe), centavos => centavos / FactorCentavos)
    {
    }

    public static long ACentavos(decimal importe)
    {
        // Defensa adicional: el dominio ya rechaza importes con más de dos decimales.
        if (decimal.Round(importe, ReglasMonetarias.DecimalesPermitidos) != importe)
        {
            throw new InvalidOperationException("El importe tiene más decimales de los permitidos para su almacenamiento.");
        }

        // decimal.ToInt64 lanza OverflowException si el valor no cabe en 64 bits.
        return decimal.ToInt64(importe * FactorCentavos);
    }
}