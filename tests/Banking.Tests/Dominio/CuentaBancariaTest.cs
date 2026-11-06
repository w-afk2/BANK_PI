using Banking.Domain.Entidades;
using Banking.Domain.Excepciones;

namespace Banking.Tests.Dominio;

/// <summary>Pruebas unitarias de las reglas de depósito y retiro (sin base de datos).</summary>
public sealed class CuentaBancariaTests
{
    private static readonly DateTime Ahora = new(2026, 11, 6, 15, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Depositar_MontoValido_IncrementaSaldoYRegistraTransaccion()
    {
        var cuenta = CrearCuenta(saldoInicial: 1000m);

        var transaccion = cuenta.Depositar(500.25m, Ahora);

        Assert.Equal(1500.25m, cuenta.Saldo);
        Assert.Equal(TipoTransaccion.Ids.Deposito, transaccion.TipoTransaccionId);
        Assert.Equal(500.25m, transaccion.Monto);
        Assert.Equal(1500.25m, transaccion.SaldoResultante);
        Assert.Equal(Ahora, transaccion.FechaHora);
    }

    [Fact]
    public void Retirar_ConFondosSuficientes_DisminuyeSaldoYRegistraTransaccion()
    {
        var cuenta = CrearCuenta(saldoInicial: 1000m);

        var transaccion = cuenta.Retirar(200m, Ahora);

        Assert.Equal(800m, cuenta.Saldo);
        Assert.Equal(TipoTransaccion.Ids.Retiro, transaccion.TipoTransaccionId);
        Assert.Equal(200m, transaccion.Monto);
        Assert.Equal(800m, transaccion.SaldoResultante);
    }

    [Fact]
    public void Retirar_FondosInsuficientes_LanzaExcepcionYNoModificaSaldo()
    {
        var cuenta = CrearCuenta(saldoInicial: 100m);

        Assert.Throws<FondosInsuficientesException>(() => cuenta.Retirar(100.01m, Ahora));

        Assert.Equal(100m, cuenta.Saldo);
    }

    [Fact]
    public void SaldoResultante_ReflejaElSaldoDespuesDeCadaOperacion()
    {
        var apertura = CuentaBancaria.Abrir(
            1, TipoCuenta.Ids.Ahorro, Moneda.Ids.NIO, "ACC-20261106-0001", 1000m, Ahora);
        var cuenta = apertura.Cuenta;

        var deposito = cuenta.Depositar(500.25m, Ahora);
        var retiro = cuenta.Retirar(200m, Ahora);

        Assert.Equal(1000m, apertura.DepositoInicial!.SaldoResultante);
        Assert.Equal(1500.25m, deposito.SaldoResultante);
        Assert.Equal(1300.25m, retiro.SaldoResultante);
        Assert.Equal(retiro.SaldoResultante, cuenta.Saldo);
    }

    private static CuentaBancaria CrearCuenta(decimal saldoInicial)
    {
        return CuentaBancaria.Abrir(
            1, TipoCuenta.Ids.Ahorro, Moneda.Ids.NIO, "ACC-20261106-0001", saldoInicial, Ahora).Cuenta;
    }
}