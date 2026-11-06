using Banking.Infrastructure.Persistencia;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Banking.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registra la persistencia (EF Core + SQLite). Una ruta relativa en la cadena de conexión se
    /// resuelve contra <paramref name="directorioBase"/>. Los repositorios se registran en la etapa 2.
    /// </summary>
    public static IServiceCollection AgregarInfraestructura(
        this IServiceCollection services,
        string cadenaConexion,
        string directorioBase)
    {
        var cadenaResuelta = RutaBaseDatos.ResolverCadenaConexion(cadenaConexion, directorioBase);

        services.AddDbContext<BankingDbContext>(opciones => opciones.UseSqlite(cadenaResuelta));

        return services;
    }

    /// <summary>
    /// Aplica las migraciones incluidas en el repositorio (entorno de demostración).
    /// No se usa EnsureCreated().
    /// </summary>
    public static async Task AplicarMigracionesAsync(
        this IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        await using var alcance = services.CreateAsyncScope();

        var contexto = alcance.ServiceProvider.GetRequiredService<BankingDbContext>();
        var logger = alcance.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger(typeof(DependencyInjection).FullName!);

        logger.LogInformation("Base de datos SQLite: {CadenaConexion}", contexto.Database.GetConnectionString());

        await contexto.Database.MigrateAsync(cancellationToken);
    }
}