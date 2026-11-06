using System.Reflection;
using Banking.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// La ruta relativa de la cadena de conexión se resuelve contra el directorio del proyecto de la API
// (fijado al compilar), tanto al ejecutar la API como al usar dotnet ef.
var directorioProyecto = typeof(Program).Assembly
    .GetCustomAttributes<AssemblyMetadataAttribute>()
    .FirstOrDefault(atributo => atributo.Key == "DirectorioProyecto")?.Value
    ?? builder.Environment.ContentRootPath;

var cadenaConexion = builder.Configuration.GetConnectionString("Banking")
    ?? throw new InvalidOperationException("No se configuró la cadena de conexión 'Banking'.");

builder.Services.AgregarInfraestructura(cadenaConexion, directorioProyecto);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// Entorno de demostración: aplica las migraciones incluidas en el repositorio al iniciar.
await app.Services.AplicarMigracionesAsync();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();

await app.RunAsync();