namespace Banking.Domain.Excepciones;

/// <summary> Retiro mayor que el saldo disponible. Se traduce a HTTP 409.</summary>
public sealed class FondosInsuficientesException : ExcepcionDominio
{
    public FondosInsuficientesException() : base("Fondos insuficientes para completar el retiro.")
    {
    }
}