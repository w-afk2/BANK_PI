namespace Banking.Domain.Excepciones;

/// <summary>Operación financiera sobre una cuenta inactiva. Se traduce a HTTP 409.</summary>
public sealed class CuentaInactivaException : ExcepcionDominio
{
    public CuentaInactivaException()
        : base("La cuenta está inactiva y no permite depósitos ni retiros.")
    {
    }
}
