namespace Banking.Application.Abstracciones;

/// <summary>Fuente de fecha y hora. Se abstrae para poder fijar el tiempo en las pruebas.</summary>
public interface IRelojSistema
{
    /// <summary>Fecha y hora actual en UTC (operaciones, transacciones y auditoría).</summary>
    DateTime AhoraUtc { get; }

    /// <summary>Fecha actual en America/Managua (número de cuenta y validación de fecha de nacimiento).</summary>
    DateOnly FechaActualManagua { get; }
}
