namespace Banking.Application.Excepciones;

/// <summary>Cliente o cuenta inexistente. Se traduce a HTTP 404.</summary>
public sealed class RecursoNoEncontradoException : ExcepcionAplicacion
{
    public RecursoNoEncontradoException(string mensaje) : base(mensaje)
    {
    }
}
