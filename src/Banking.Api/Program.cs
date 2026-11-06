using System.Reflection;
using System.Text.Json.Serialization;
using Banking.Api.Manejadores;
using Banking.Application;
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

builder.Services.AgregarAplicacion();
builder.Services.AgregarInfraestructura(cadenaConexion, directorioProyecto);

// Los enums (por ejemplo, Genero) se reciben y envían como texto: "Femenino".
builder.Services
    .AddControllers()
    .AddJsonOptions(opciones => opciones.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Manejo global de excepciones con respuestas ProblemDetails.
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ManejadorExcepcionesGlobal>();

builder.Services.AddOpenApi();

var app = builder.Build();

// Entorno de demostración: aplica las migraciones incluidas en el repositorio al iniciar.
await app.Services.AplicarMigracionesAsync();

app.UseExceptionHandler();

// Documentación interactiva solo en Development o en el entorno explícito "Evaluacion".
// En cualquier otro entorno (por ejemplo, Production) Swagger no se publica.
if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Evaluacion"))
{
    app.MapOpenApi();
    app.UseSwaggerUI(opciones =>
    {
        opciones.SwaggerEndpoint("/openapi/v1.json", "Banking API v1");
        opciones.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

app.MapControllers();

await app.RunAsync();
