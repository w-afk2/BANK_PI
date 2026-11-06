using Banking.Application.Dtos;
using Banking.Application.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace Banking.Api.Controllers;

[ApiController]
[Route("api/clientes")]
[Produces("application/json")]
public sealed class ClientesController(IClienteServicio clienteServicio) : ControllerBase
{
    /// <summary>Registra un cliente (RF-01). IngresosMensuales en córdobas (NIO).</summary>
    [HttpPost]
    [ProducesResponseType<ClienteRespuesta>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ClienteRespuesta>> Registrar(
        RegistrarClienteSolicitud solicitud,
        CancellationToken cancellationToken)
    {
        var cliente = await clienteServicio.RegistrarAsync(solicitud, cancellationToken);
        return CreatedAtAction(nameof(ObtenerPerfil), new { id = cliente.Id }, cliente);
    }

    /// <summary>Consulta el perfil del cliente con su nombre completo y sus cuentas.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType<ClienteRespuesta>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClienteRespuesta>> ObtenerPerfil(int id, CancellationToken cancellationToken)
    {
        return Ok(await clienteServicio.ObtenerPerfilAsync(id, cancellationToken));
    }
}
