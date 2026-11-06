using Banking.Application.Abstracciones;
using Banking.Application.Excepciones;
using Banking.Domain.Comun;

namespace Banking.Application.Servicios;

/// <summary>
/// Consecutivo diario de cuatro dígitos (0001-9999) según la fecha de America/Managua.
/// Consulta el último número registrado del día; el índice único de NumeroCuenta es la garantía
/// definitiva de unicidad y las colisiones se reintentan en CuentaBancariaServicio.
/// </summary>
public sealed class GeneradorNumeroCuenta(
    ICuentaBancariaRepositorio cuentas,
    IRelojSistema reloj) : IGeneradorNumeroCuenta
{
    public async Task<string> GenerarAsync(CancellationToken cancellationToken = default)
    {
        var fecha = reloj.FechaActualManagua;
        var ultimoConsecutivo = await cuentas.ObtenerUltimoConsecutivoAsync(fecha, cancellationToken);
        var siguiente = ultimoConsecutivo + 1;

        if (siguiente > FormatoNumeroCuenta.ConsecutivoMaximo)
        {
            throw new NumerosCuentaAgotadosException();
        }

        return FormatoNumeroCuenta.Formatear(fecha, siguiente);
    }
}
