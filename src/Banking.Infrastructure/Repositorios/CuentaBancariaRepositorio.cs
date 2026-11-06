using System.Globalization;
using Banking.Application.Abstracciones;
using Banking.Domain.Comun;
using Banking.Domain.Entidades;
using Banking.Infrastructure.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace Banking.Infrastructure.Repositorios;

internal sealed class CuentaBancariaRepositorio(BankingDbContext contexto) : ICuentaBancariaRepositorio
{
    private const int LongitudConsecutivo = 4;

    public void Agregar(CuentaBancaria cuenta) => contexto.CuentasBancarias.Add(cuenta);

    /// <summary>Con seguimiento de cambios: la cuenta devuelta puede modificarse y guardarse.</summary>
    public Task<CuentaBancaria?> ObtenerPorNumeroAsync(string numeroCuenta, CancellationToken cancellationToken = default)
    {
        return ConCatalogos(contexto.CuentasBancarias)
            .FirstOrDefaultAsync(cuenta => cuenta.NumeroCuenta == numeroCuenta, cancellationToken);
    }

    public async Task<IReadOnlyList<CuentaBancaria>> ObtenerPorClienteAsync(int clienteId, CancellationToken cancellationToken = default)
    {
        return await ConCatalogos(contexto.CuentasBancarias.AsNoTracking())
            .Where(cuenta => cuenta.ClienteId == clienteId)
            .OrderBy(cuenta => cuenta.Id)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Busca el mayor número del día (ACC-YYYYMMDD-XXXX). Como el consecutivo tiene ceros a la
    /// izquierda, el orden alfabético coincide con el numérico.
    /// </summary>
    public async Task<int> ObtenerUltimoConsecutivoAsync(DateOnly fecha, CancellationToken cancellationToken = default)
    {
        var prefijoDelDia = string.Concat(
            FormatoNumeroCuenta.Prefijo,
            fecha.ToString(FormatoNumeroCuenta.FormatoFecha, CultureInfo.InvariantCulture),
            "-");

        var ultimoNumero = await contexto.CuentasBancarias
            .AsNoTracking()
            .Where(cuenta => cuenta.NumeroCuenta.StartsWith(prefijoDelDia))
            .OrderByDescending(cuenta => cuenta.NumeroCuenta)
            .Select(cuenta => cuenta.NumeroCuenta)
            .FirstOrDefaultAsync(cancellationToken);

        return ultimoNumero is null
            ? 0
            : int.Parse(ultimoNumero[^LongitudConsecutivo..], CultureInfo.InvariantCulture);
    }

    private static IQueryable<CuentaBancaria> ConCatalogos(IQueryable<CuentaBancaria> consulta)
    {
        return consulta
            .Include(cuenta => cuenta.TipoCuenta)
            .Include(cuenta => cuenta.EstadoCuenta)
            .Include(cuenta => cuenta.Moneda);
    }
}
