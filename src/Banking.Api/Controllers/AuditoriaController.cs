using Banking.Api.Filtros;
using Banking.Application.Dtos;
using Banking.Application.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace Banking.Api.Controllers;

/// <summary>
/// Consulta paginada de auditoría. Sin autenticación en el proyecto, solo está disponible
/// en los entornos Development o Evaluacion; en otro entorno responde 404.
/// </summary>
[ApiController]
[Route("api/auditoria")]
[Produces("application/json")]
[SoloEntornoLocal]
public sealed class AuditoriaController(IAuditoriaServicio auditoriaServicio) : ControllerBase
{
    /// <summary>
    /// Filtros opcionales: desde (inclusivo), hasta (exclusivo), resultado (Exitoso o Rechazado),
    /// numeroCuenta, pagina (desde 1) y tamanoPagina (1 a 100). Más recientes primero.
    /// </summary>
    [HttpGet]
    [ProducesResponseType<PaginaRespuesta<AuditoriaRespuesta>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaginaRespuesta<AuditoriaRespuesta>>> Consultar(
        [FromQuery] FiltroAuditoriaSolicitud filtro,
        CancellationToken cancellationToken)
    {
        return Ok(await auditoriaServicio.ConsultarAsync(filtro, cancellationToken));
    }
}
