using Microsoft.Data.Sqlite;

namespace Banking.Infrastructure.Persistencia;

/// <summary>
/// Resuelve la ruta del archivo SQLite contra un directorio base fijo (el del proyecto de la API),
/// para que la ejecución normal y las herramientas de EF Core usen el mismo archivo.
/// </summary>
public static class RutaBaseDatos
{
    public static string ResolverCadenaConexion(string cadenaConexion, string directorioBase)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cadenaConexion);
        ArgumentException.ThrowIfNullOrWhiteSpace(directorioBase);

        var constructor = new SqliteConnectionStringBuilder(cadenaConexion);

        var esEnMemoria = constructor.Mode == SqliteOpenMode.Memory
            || string.Equals(constructor.DataSource, ":memory:", StringComparison.OrdinalIgnoreCase);

        if (esEnMemoria || string.IsNullOrWhiteSpace(constructor.DataSource))
        {
            return constructor.ToString();
        }

        var rutaArchivo = Path.IsPathRooted(constructor.DataSource)
            ? constructor.DataSource
            : Path.GetFullPath(Path.Combine(directorioBase, constructor.DataSource));

        var directorio = Path.GetDirectoryName(rutaArchivo);
        if (!string.IsNullOrEmpty(directorio))
        {
            Directory.CreateDirectory(directorio);
        }

        constructor.DataSource = rutaArchivo;
        return constructor.ToString();
    }
}