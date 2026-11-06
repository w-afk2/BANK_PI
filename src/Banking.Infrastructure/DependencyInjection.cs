using Banking.Application.Abstracciones;
using Banking.Infrastructure.Persistencia;
using Banking.Infrastructure.Repositorios;
using Banking.Infrastructure.Servicios;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Banking.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registra la persistencia (EF Core + SQLite), los repositorios, la unidad de trabajo y el reloj.
    /// Una ruta relativa en la cadena de conexión se resuelve contra <paramref name="directorioBase"/>.
    /// </summary>
    public static IServiceCollection AgregarInfraestructura(
        this IServiceCollection services,
        string cadenaConexion,
        string directorioBase)
    {
        var cadenaResuelta = RutaBaseDatos.ResolverCadenaConexion(cadenaConexion, directorioBase);

        services.AddDbContext<BankingDbContext>(opciones => opciones.UseSqlite(cadenaResuelta));

        services.AddScoped<IUnidadDeTrabajo, UnidadDeTrabajo>();
        services.AddScoped<IClienteRepositorio, ClienteRepositorio>();
        services.AddScoped<ICuentaBancariaRepositorio, CuentaBancariaRepositorio>();
        services.AddScoped<ITransaccionRepositorio, TransaccionRepositorio>();
        services.AddScoped<IAuditoriaRepositorio, AuditoriaRepositorio>();
        services.AddScoped<ICatalogoRepositorio, CatalogoRepositorio>();
        services.AddSingleton<IRelojSistema, RelojSistema>();

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
