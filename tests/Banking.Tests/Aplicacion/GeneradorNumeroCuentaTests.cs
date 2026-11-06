using Banking.Application.Excepciones;
using Banking.Application.Servicios;
using Banking.Domain.Comun;

namespace Banking.Tests.Aplicacion;

/// <summary>Pruebas unitarias del generador: formato, fecha, unicidad básica y límite diario.</summary>
public sealed class GeneradorNumeroCuentaTests
{
    private static readonly DateOnly FechaManagua = new(2026, 11, 6);

    [Fact]
    public async Task Generar_SinCuentasDelDia_DevuelvePrimerConsecutivoConFormatoYFecha()
    {
        var generador = CrearGenerador(new RepositorioCuentasFalso());

        var numero = await generador.GenerarAsync();

        Assert.Equal("ACC-20261106-0001", numero);
        Assert.Equal(FormatoNumeroCuenta.Longitud, numero.Length);
        Assert.StartsWith(FormatoNumeroCuenta.Prefijo, numero);
        Assert.Equal("20261106", numero.Substring(4, 8));
        Assert.True(FormatoNumeroCuenta.EsValido(numero));
    }

    [Fact]
    public async Task Generar_VariasCuentasDelMismoDia_ProduceNumerosUnicosYConsecutivos()
    {
        var repositorio = new RepositorioCuentasFalso();
        var generador = CrearGenerador(repositorio);
        var numeros = new List<string>();

        for (var i = 0; i < 50; i++)
        {
            var numero = await generador.GenerarAsync();
            repositorio.Registrar(numero);
            numeros.Add(numero);
        }

        Assert.Equal(50, numeros.Distinct().Count());
        Assert.Equal("ACC-20261106-0001", numeros[0]);
        Assert.Equal("ACC-20261106-0050", numeros[^1]);
        Assert.All(numeros, numero => Assert.True(FormatoNumeroCuenta.EsValido(numero)));
    }

    [Fact]
    public async Task Generar_ContinuaDespuesDelUltimoNumeroDelDiaEIgnoraOtrasFechas()
    {
        var repositorio = new RepositorioCuentasFalso();
        repositorio.Registrar("ACC-20261105-0099");   // otro día: no cuenta
        repositorio.Registrar("ACC-20261106-0041");

        var numero = await CrearGenerador(repositorio).GenerarAsync();

        Assert.Equal("ACC-20261106-0042", numero);
    }

    [Fact]
    public async Task Generar_ConsecutivosDelDiaAgotados_LanzaExcepcionControlada()
    {
        var repositorio = new RepositorioCuentasFalso();
        repositorio.Registrar("ACC-20261106-9999");

        await Assert.ThrowsAsync<NumerosCuentaAgotadosException>(
            () => CrearGenerador(repositorio).GenerarAsync());
    }

    private static GeneradorNumeroCuenta CrearGenerador(RepositorioCuentasFalso repositorio)
    {
        return new GeneradorNumeroCuenta(repositorio, new RelojFijo(FechaManagua));
    }
}