namespace Banking.Domain.Excepciones;

/// <summary>Importe no positivo, con más de dos decimales o fuera de rango. Se traduce a HTTP 400.</summary>
public sealed class MontoInvalidoException : ValidacionException
{
    public MontoInvalidoException(string mensaje) : base(mensaje)
    {



    }
}