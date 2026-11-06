namespace Banking.Application.Excepciones;

/// <summary>
/// Base de los errores de la capa de aplicación (recurso inexistente, conflictos).
/// El mensaje está pensado para mostrarse al usuario: no debe contener datos sensibles.
/// </summary>
public abstract class ExcepcionAplicacion : Exception
{
    protected ExcepcionAplicacion(string mensaje) : base(mensaje)
    {
    }

    protected ExcepcionAplicacion(string mensaje, Exception? excepcionInterna) : base(mensaje, excepcionInterna)
    {
    }
}
