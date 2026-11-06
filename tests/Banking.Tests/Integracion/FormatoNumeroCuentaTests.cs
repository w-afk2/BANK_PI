using Banking.Domain.Comun;

namespace Banking.Tests.Dominio;

public sealed class FormatoNumeroCuentaTests
{
    [Theory]
    [InlineData("ACC-20261106-0001", true)]
    [InlineData("ACC-20261106-9999", true)]
    [InlineData("ACC-20261106-0000", false)]   // consecutivo fuera de rango
    [InlineData("ACC-20261331-0001", false)]   // fecha inexistente
    [InlineData("ACC-20261106-001", false)]    // 16 caracteres
    [InlineData("XYZ-20261106-0001", false)]   // prefijo incorrecto
    [InlineData("ACC-20261106-00A1", false)]   // consecutivo no numérico
    public void EsValido_AplicaFormatoYRango(string numeroCuenta, bool esperado)
    {
        Assert.Equal(esperado, FormatoNumeroCuenta.EsValido(numeroCuenta));
    }
}