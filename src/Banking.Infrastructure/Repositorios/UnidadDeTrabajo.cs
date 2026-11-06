using Banking.Application.Abstracciones;
using Banking.Application.Excepciones;
using Banking.Infrastructure.Persistencia;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Banking.Infrastructure.Repositorios;

/// <summary>
/// SaveChangesAsync guarda todos los cambios pendientes en una sola transacción de base de datos:
/// el saldo, la transacción y la auditoría se confirman juntos o no se confirma ninguno.
/// </summary>
internal sealed class UnidadDeTrabajo(
    BankingDbContext contexto,
    ILogger<UnidadDeTrabajo> logger) : IUnidadDeTrabajo
{
    private const int CodigoErrorRestriccionSqlite = 19;
    private const string IndiceNumeroCuenta = "CuentasBancarias.NumeroCuenta";

    public async Task GuardarCambiosAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await contexto.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException excepcion)
        {
            // Saldo o EstadoCuentaId cambiaron desde que se leyó la cuenta. No se guardó ningún cambio.
            logger.LogWarning(
                excepcion,
                "Conflicto de concurrencia: la cuenta fue modificada por otra operación. Se revirtieron todos los cambios.");

            throw new ConflictoConcurrenciaException(excepcion);
        }
        catch (DbUpdateException excepcion) when (EsNumeroCuentaDuplicado(excepcion))
        {
            logger.LogWarning("Colisión en el índice único de NumeroCuenta al crear una cuenta.");

            throw new NumeroCuentaDuplicadoException(excepcion);
        }
    }

    public void DescartarCambios() => contexto.ChangeTracker.Clear();

    /// <summary>
    /// Solo la violación del índice único de NumeroCuenta se considera colisión reintentable.
    /// Cualquier otro error de base de datos se propaga sin reintentos.
    /// </summary>
    private static bool EsNumeroCuentaDuplicado(DbUpdateException excepcion)
    {
        return excepcion.InnerException is SqliteException { SqliteErrorCode: CodigoErrorRestriccionSqlite } sqlite
            && sqlite.Message.Contains(IndiceNumeroCuenta, StringComparison.Ordinal);
    }
}