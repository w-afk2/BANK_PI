namespace Banking.Api.Configuracion;

public static class EntornoExtensions
{
    /// <summary>Entorno explícito para la evaluación local de la prueba técnica.</summary>
    public const string EntornoEvaluacion = "Evaluacion";

    /// <summary>
    /// Swagger y la consulta de auditoría solo se exponen en Development o Evaluacion,
    /// porque el proyecto no tiene autenticación.
    /// </summary>
    public static bool EsEntornoLocal(this IHostEnvironment entorno)
    {
        return entorno.IsDevelopment() || entorno.IsEnvironment(EntornoEvaluacion);
    }
}
