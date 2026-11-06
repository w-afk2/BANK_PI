using Banking.Application.Excepciones;
using Banking.Domain.Excepciones;
using Microsoft.AspNetCore.Diagnostics;

namespace Banking.Api.Manejadores;

/// <summary>
/// Traduce las excepciones a respuestas ProblemDetails (JSON) con el código HTTP aprobado.
/// Los controladores no usan try-catch. Nunca se exponen trazas ni detalles internos.
/// </summary>
public sealed class ManejadorExcepcionesGlobal(
    IProblemDetailsService problemDetailsService,
    ILogger<ManejadorExcepcionesGlobal> logger) : IExceptionHandler
{
    private const string MensajeErrorInterno = "Ocurrió un error inesperado. Intente nuevamente más tarde.";

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (estado, titulo) = Clasificar(exception);
        var esErrorInterno = estado == StatusCodes.Status500InternalServerError;

        if (esErrorInterno)
        {
            logger.LogError(
                exception,
                "Error no controlado en {Metodo} {Ruta}.",
                httpContext.Request.Method,
                httpContext.Request.Path);
        }
        else
        {
            logger.LogInformation("Solicitud rechazada con {Estado}: {Mensaje}", estado, exception.Message);
        }

        httpContext.Response.StatusCode = estado;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails =
            {
                Status = estado,
                Title = titulo,
                Detail = esErrorInterno ? MensajeErrorInterno : exception.Message
            }
        });
    }

    /// <summary>
    /// 400: montos inválidos, datos incorrectos y saldo inicial negativo.
    /// 404: cliente o cuenta inexistente.
    /// 409: fondos insuficientes, cuenta inactiva, concurrencia y números de cuenta.
    /// 500: cualquier otro error.
    /// </summary>
    private static (int Estado, string Titulo) Clasificar(Exception exception) => exception switch
    {
        ValidacionException => (StatusCodes.Status400BadRequest, "Datos inválidos"),
        RecursoNoEncontradoException => (StatusCodes.Status404NotFound, "Recurso no encontrado"),
        FondosInsuficientesException
            or CuentaInactivaException
            or ConflictoConcurrenciaException
            or NumerosCuentaAgotadosException
            or NumeroCuentaDuplicadoException => (StatusCodes.Status409Conflict, "Operación rechazada"),
        _ => (StatusCodes.Status500InternalServerError, "Error interno")
    };
}
