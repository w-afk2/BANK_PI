using Banking.Domain.Entidades;

namespace Banking.Application.Abstracciones;

public interface IAuditoriaRepositorio
{
    void Agregar(Auditoria auditoria);

    /// <summary>Eventos más recientes primero (FechaHora y luego Id, descendente).</summary>
    Task<ResultadoPaginado<Auditoria>> ConsultarAsync(
        ConsultaAuditoria consulta,
        CancellationToken cancellationToken = default);
}
