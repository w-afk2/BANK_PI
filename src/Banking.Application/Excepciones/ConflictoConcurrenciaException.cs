namespace Banking.Application.Excepciones;

/// <summary>
/// Otra operación modificó la cuenta (saldo o estado) al mismo tiempo. No se reintenta.
/// Se traduce a HTTP 409.
/// </summary>
public sealed class ConflictoConcurrenciaException : ExcepcionAplicacion
{
    public ConflictoConcurrenciaException(Exception? excepcionInterna = null)
        : base(
            "La cuenta fue modificada por otra operación al mismo tiempo. Consulte el saldo e intente nuevamente.",
            excepcionInterna)
    {
    }
}
