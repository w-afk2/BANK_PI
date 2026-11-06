namespace Banking.Application.Excepciones;

/// <summary>Ya se usaron los consecutivos 0001-9999 del día. Se traduce a HTTP 409.</summary>
public sealed class NumerosCuentaAgotadosException : ExcepcionAplicacion
{
    public NumerosCuentaAgotadosException()
        : base("No hay números de cuenta disponibles para la fecha actual.")
    {
    }
}
