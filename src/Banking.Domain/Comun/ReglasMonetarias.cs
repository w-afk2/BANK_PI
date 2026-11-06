using Banking.Domain.Excepciones;

namespace Banking.Domain.Comun
{
    /// <summary>
    /// Reglas comunes para importes monetarios (Saldo, Monto, SaldoResultante, IngresosMensuales).
    /// Los importes se persisten como enteros de 64 bits en centavos, por lo que el valor
    /// absoluto máximo representable es long.MaxValue / 100.
    /// </summary>

    public class ReglasMonetarias
    {
        public const int DecimalesPermitidos = 2;

        /// <summary>long.MaxValue (9 223 372 036 854 775 807) expresado en unidades con dos decimales.</summary>
        public const decimal ImporteMaximo = 92_233_720_368_547_758.07m;

        /// <summary>Monto de depósito o retiro: mayor que cero, máximo dos decimales y dentro de rango.</summary>
        public static void ValidarMontoOperacion(decimal monto)
        {
            if (monto <= 0)
            {
                throw new MontoInvalidoException("El monto debe ser mayor que cero.");
            }

            ValidarFormato(monto, "Monto");
        }

        /// <summary>Importe que admite cero pero no valores negativos (por ejemplo, IngresosMensuales).</summary>
        public static void ValidarImporteNoNegativo(decimal importe, string campo)
        {
            if (importe < 0)
            {
                throw new MontoInvalidoException($"El valor de {campo} no puede ser negativo.");
            }

            ValidarFormato(importe, campo);
        }

        /// <summary>
        /// Rechaza importes con más de dos decimales (no se redondean) o fuera del rango almacenable.
        /// </summary>
        public static void ValidarFormato(decimal importe, string campo)
        {
            if (decimal.Round(importe, DecimalesPermitidos) != importe)
            {
                throw new MontoInvalidoException(
                    $"El valor de {campo} admite como máximo {DecimalesPermitidos} decimales.");
            }

            if (Math.Abs(importe) > ImporteMaximo)
            {
                throw new MontoInvalidoException($"El valor de {campo} excede el máximo permitido.");
            }
        }
    }
}
