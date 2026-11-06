using Banking.Application.Dtos;
using Banking.Application.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace Banking.Api.Controllers;

[ApiController]
[Route("api/cuentas")]
[Produces("application/json")]
public sealed class CuentasController(ICuentaBancariaServicio cuentaServicio) : ControllerBase
{
    /// <summary>Crea una cuenta con número generado automáticamente (RF-02).</summary>
    [HttpPost]
    [ProducesResponseType<CuentaRespuesta>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CuentaRespuesta>> Crear(
        CrearCuentaSolicitud solicitud,
        CancellationToken cancellationToken)
    {
        var cuenta = await cuentaServicio.CrearAsync(solicitud, cancellationToken);
        return CreatedAtAction(nameof(ConsultarSaldo), new { numeroCuenta = cuenta.NumeroCuenta }, cuenta);
    }

    /// <summary>Consulta el saldo actual por número de cuenta (RF-03).</summary>
    [HttpGet("{numeroCuenta}/saldo")]
    [ProducesResponseType<SaldoRespuesta>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SaldoRespuesta>> ConsultarSaldo(
        string numeroCuenta,
        CancellationToken cancellationToken)
    {
        return Ok(await cuentaServicio.ConsultarSaldoAsync(numeroCuenta, cancellationToken));
    }

    /// <summary>
    /// Cambia el estado de la cuenta (1 = Activa, 2 = Inactiva). Solicitar el estado actual
    /// responde 200 sin modificar la cuenta ni registrar auditoría.
    /// </summary>
    [HttpPatch("{numeroCuenta}/estado")]
    [ProducesResponseType<CuentaRespuesta>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CuentaRespuesta>> CambiarEstado(
        string numeroCuenta,
        CambiarEstadoSolicitud solicitud,
        CancellationToken cancellationToken)
    {
        return Ok(await cuentaServicio.CambiarEstadoAsync(numeroCuenta, solicitud, cancellationToken));
    }
}
