using System.Globalization;
using Banking.Application.Abstracciones;
using Banking.Domain.Comun;
using Banking.Domain.Entidades;

namespace Banking.Tests.Aplicacion;

/// <summary>Reloj con una fecha fija de America/Managua.</summary>
internal sealed class RelojFijo(DateOnly fechaManagua) : IRelojSistema
{
    public DateTime AhoraUtc { get; } = fechaManagua.ToDateTime(new TimeOnly(18, 0), DateTimeKind.Utc);

    public DateOnly FechaActualManagua { get; } = fechaManagua;
}

/// <summary>Repositorio en memoria: solo guarda números de cuenta para el generador.</summary>
internal sealed class RepositorioCuentasFalso : ICuentaBancariaRepositorio
{
    private readonly List<string> _numeros = [];

    public void Registrar(string numeroCuenta) => _numeros.Add(numeroCuenta);

    public Task<int> ObtenerUltimoConsecutivoAsync(DateOnly fecha, CancellationToken cancellationToken = default)
    {
        var prefijoDelDia = string.Concat(
            FormatoNumeroCuenta.Prefijo,
            fecha.ToString(FormatoNumeroCuenta.FormatoFecha, CultureInfo.InvariantCulture),
            "-");

        var ultimo = _numeros
            .Where(numero => numero.StartsWith(prefijoDelDia, StringComparison.Ordinal))
            .Select(numero => int.Parse(numero[^4..], CultureInfo.InvariantCulture))
            .DefaultIfEmpty(0)
            .Max();

        return Task.FromResult(ultimo);
    }

    public void Agregar(CuentaBancaria cuenta) => throw new NotSupportedException();

    public Task<CuentaBancaria?> ObtenerPorNumeroAsync(string numeroCuenta, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();

    public Task<IReadOnlyList<CuentaBancaria>> ObtenerPorClienteAsync(int clienteId, CancellationToken cancellationToken = default)
        => throw new NotSupportedException();
}