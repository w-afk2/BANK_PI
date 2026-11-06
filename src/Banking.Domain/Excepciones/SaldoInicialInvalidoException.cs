
namespace Banking.Domain.Excepciones;
/// <summary>Saldo inicial negativo. Se traduce a HTTP 400.</summary>

public sealed class SaldoInicialInvalidoException : ValidacionException
{
    public SaldoInicialInvalidoException() : base("El saldo inicial no puede ser negativo.")
    {
    }
}
