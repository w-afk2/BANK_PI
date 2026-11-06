using Banking.Application.Dtos;

namespace Banking.Application.Servicios;

/// <summary>Funcionalidad adicional: consulta paginada de auditoría con filtros básicos.</summary>
public interface IAuditoriaServicio
{
    Task<PaginaRespuesta<AuditoriaRespuesta>> ConsultarAsync(
        FiltroAuditoriaSolicitud filtro,
        CancellationToken cancellationToken = default);
}
