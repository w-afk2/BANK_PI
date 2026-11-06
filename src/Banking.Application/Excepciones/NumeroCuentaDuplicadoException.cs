namespace Banking.Application.Excepciones;

/// <summary>
/// El índice único de NumeroCuenta rechazó el número generado (colisión por concurrencia).
/// El servicio reintenta de forma limitada; si se agotan los intentos, se traduce a HTTP 409.
/// </summary>
public sealed class NumeroCuentaDuplicadoException : ExcepcionAplicacion
{
    public NumeroCuentaDuplicadoException(Exception? excepcionInterna = null)
        : base("No fue posible generar un número de cuenta único. Intente nuevamente.", excepcionInterna)
    {
    }
}
