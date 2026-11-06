namespace Banking.Domain.Excepciones;

/// <summary>
/// Base de las excepciones que representan el incumplimiento de una regla de negocio.
/// El mensaje está pensado para mostrarse al usuario: no debe contener datos sensibles.
/// </summary>

public abstract class ExcepcionDominio : Exception
{
    protected ExcepcionDominio(string mensaje) : base(mensaje)
    {
    }


}
