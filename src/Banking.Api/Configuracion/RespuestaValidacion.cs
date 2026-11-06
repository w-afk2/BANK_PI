using Microsoft.AspNetCore.Mvc;

namespace Banking.Api.Configuracion;

/// <summary>
/// Respuesta 400 de la validación automática de [ApiController] (JSON mal formado, campos faltantes
/// o con tipo incorrecto). Los mensajes se devuelven en español y sin detalles internos, como
/// nombres de tipos .NET o posiciones dentro del JSON.
/// </summary>
public static class RespuestaValidacion
{
    public static IActionResult Crear(ActionContext contexto)
    {
        var errores = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        var hayErroresDeJson = false;

        foreach (var (clave, entrada) in contexto.ModelState)
        {
            if (entrada.Errors.Count == 0)
            {
                continue;
            }

            var esErrorDeJson = clave.StartsWith('$');
            hayErroresDeJson |= esErrorDeJson;

            var campo = NombreCampo(clave);
            errores[campo] = entrada.Errors
                .Select(error => esErrorDeJson || !EsObligatorio(error.ErrorMessage)
                    ? $"El campo {campo} no tiene un valor válido."
                    : $"El campo {campo} es obligatorio.")
                .Distinct()
                .ToArray();
        }

        // Cuando un campo del JSON es inválido, ASP.NET también informa que falta el parámetro completo
        // (por ejemplo "solicitud"). Ese error es redundante y confuso, así que se omite.
        if (hayErroresDeJson)
        {
            foreach (var parametro in contexto.ActionDescriptor.Parameters)
            {
                errores.Remove(parametro.Name);
            }
        }

        var problema = new ValidationProblemDetails(errores)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Datos inválidos",
            Detail = "Revise los datos enviados.",
            Instance = contexto.HttpContext.Request.Path
        };

        var resultado = new BadRequestObjectResult(problema);
        resultado.ContentTypes.Add("application/problem+json");
        return resultado;
    }

    /// <summary>"$.saldoInicial" → "saldoInicial"; "solicitud.Monto" → "Monto"; "$" o "" → "cuerpo".</summary>
    private static string NombreCampo(string clave)
    {
        var campo = clave.TrimStart('$').TrimStart('.');
        var ultimoPunto = campo.LastIndexOf('.');

        if (ultimoPunto >= 0)
        {
            campo = campo[(ultimoPunto + 1)..];
        }

        return string.IsNullOrWhiteSpace(campo) ? "cuerpo" : campo;
    }

    private static bool EsObligatorio(string mensaje)
    {
        return mensaje.Contains("required", StringComparison.OrdinalIgnoreCase);
    }
}