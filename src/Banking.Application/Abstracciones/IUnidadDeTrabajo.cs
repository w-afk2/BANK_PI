namespace Banking.Application.Abstracciones;

/// <summary>
/// Confirma de forma atómica todos los cambios pendientes (saldo, transacción y auditoría)
/// en una sola transacción de base de datos.
/// </summary>
public interface IUnidadDeTrabajo
{
    /// <summary>
    /// Guarda los cambios pendientes. Si falla, no se guarda ninguno.
    /// Lanza ConflictoConcurrenciaException o NumeroCuentaDuplicadoException según el caso.
    /// </summary>
    Task GuardarCambiosAsync(CancellationToken cancellationToken = default);

    /// <summary>Descarta los cambios pendientes que aún no se han guardado.</summary>
    void DescartarCambios();
}
