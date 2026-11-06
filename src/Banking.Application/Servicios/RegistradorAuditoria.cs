using Banking.Application.Abstracciones;
using Banking.Domain.Entidades;
using Banking.Domain.Enumeraciones;
using Microsoft.Extensions.Logging;

namespace Banking.Application.Servicios;

/// <summary>
/// Registra los rechazos en una escritura independiente: primero descarta los cambios de la
/// operación fallida y luego guarda solo el evento. Si la auditoría falla, se registra en los logs
/// y no se reemplaza la excepción de negocio original.
/// </summary>
public sealed class RegistradorAuditoria(
    IAuditoriaRepositorio auditorias,
    IUnidadDeTrabajo unidadDeTrabajo,
    IRelojSistema reloj,
    ILogger<RegistradorAuditoria> logger)
{
    public async Task RegistrarRechazoAsync(
        AccionAuditoria accion,
        string motivo,
        int? clienteId,
        int? cuentaBancariaId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            unidadDeTrabajo.DescartarCambios();

            auditorias.Agregar(Auditoria.RegistrarConIdentificadores(
                accion,
                ResultadoAuditoria.Rechazado,
                motivo,
                reloj.AhoraUtc,
                clienteId,
                cuentaBancariaId));

            await unidadDeTrabajo.GuardarCambiosAsync(cancellationToken);
        }
        catch (Exception excepcion)
        {
            logger.LogError(excepcion, "No se pudo registrar en auditoría el rechazo de la acción {Accion}.", accion);
        }
    }
}
