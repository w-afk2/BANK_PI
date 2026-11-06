using Banking.Application.Dtos;
using Banking.Application.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace Banking.Api.Controllers;

/// <summary>Catálogos de solo lectura usados por la interfaz.</summary>
[ApiController]
[Route("api/catalogos")]
[Produces("application/json")]
public sealed class CatalogosController(ICatalogoServicio catalogoServicio) : ControllerBase
{
    [HttpGet("tipos-cuenta")]
    [ProducesResponseType<IReadOnlyList<CatalogoRespuesta>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CatalogoRespuesta>>> ObtenerTiposCuenta(CancellationToken cancellationToken)
    {
        return Ok(await catalogoServicio.ObtenerTiposCuentaAsync(cancellationToken));
    }

    [HttpGet("estados-cuenta")]
    [ProducesResponseType<IReadOnlyList<CatalogoRespuesta>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CatalogoRespuesta>>> ObtenerEstadosCuenta(CancellationToken cancellationToken)
    {
        return Ok(await catalogoServicio.ObtenerEstadosCuentaAsync(cancellationToken));
    }

    [HttpGet("monedas")]
    [ProducesResponseType<IReadOnlyList<MonedaRespuesta>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<MonedaRespuesta>>> ObtenerMonedas(CancellationToken cancellationToken)
    {
        return Ok(await catalogoServicio.ObtenerMonedasAsync(cancellationToken));
    }
}
