using Banking.Api.Configuracion;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Banking.Api.Filtros;

/// <summary>
/// Restringe un endpoint a los entornos Development o Evaluacion. En cualquier otro entorno
/// responde 404, como si el endpoint no existiera.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class SoloEntornoLocalAttribute : Attribute, IResourceFilter
{
    public void OnResourceExecuting(ResourceExecutingContext context)
    {
        var entorno = context.HttpContext.RequestServices.GetRequiredService<IHostEnvironment>();

        if (!entorno.EsEntornoLocal())
        {
            context.Result = new NotFoundResult();
        }
    }

    public void OnResourceExecuted(ResourceExecutedContext context)
    {
    }
}
