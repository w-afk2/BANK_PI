using Banking.Application.Dtos;
using Banking.Application.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace Banking.Api.Controllers;

[ApiController]
[Route("api/cuentas/{numeroCuenta}")]
[Produces("application/json")]
public sealed class MovimientosController(ITransaccionServicio transaccionServicio) : ControllerBase
{
    /// <summary>Registra un depósito (RF-04).</summary>
    [HttpPost("depositos")]
    [ProducesResponseType<TransaccionRespuesta>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TransaccionRespuesta>> Depositar(
        string numeroCuenta,
        OperacionSolicitud solicitud,
        CancellationToken cancellationToken)
    {
        var transaccion = await transaccionServicio.DepositarAsync(numeroCuenta, solicitud, cancellationToken);
        return Created(RutaHistorial(numeroCuenta), transaccion);
    }

    /// <summary>Registra un retiro; se rechaza si no hay fondos suficientes (RF-05).</summary>
    [HttpPost("retiros")]
    [ProducesResponseType<TransaccionRespuesta>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TransaccionRespuesta>> Retirar(
        string numeroCuenta,
        OperacionSolicitud solicitud,
        CancellationToken cancellationToken)
    {
        var transaccion = await transaccionServicio.RetirarAsync(numeroCuenta, solicitud, cancellationToken);
        return Created(RutaHistorial(numeroCuenta), transaccion);
    }

    /// <summary>Historial de movimientos en orden cronológico (RF-06).</summary>
    [HttpGet("transacciones")]
    [ProducesResponseType<HistorialRespuesta>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<HistorialRespuesta>> ObtenerHistorial(
        string numeroCuenta,
        CancellationToken cancellationToken)
    {
        return Ok(await transaccionServicio.ObtenerHistorialAsync(numeroCuenta, cancellationToken));
    }

    private static string RutaHistorial(string numeroCuenta) => $"/api/cuentas/{numeroCuenta}/transacciones";
}
