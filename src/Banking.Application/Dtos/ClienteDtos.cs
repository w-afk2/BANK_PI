using Banking.Domain.Enumeraciones;

namespace Banking.Application.Dtos;

/// <summary>RF-01. IngresosMensuales se expresa en córdobas (NIO).</summary>
public sealed record RegistrarClienteSolicitud(
    string Nombres,
    string PrimerApellido,
    string? SegundoApellido,
    DateOnly FechaNacimiento,
    Genero Genero,
    decimal IngresosMensuales);

/// <summary>Perfil del cliente con su nombre completo y sus cuentas.</summary>
public sealed record ClienteRespuesta(
    int Id,
    string Nombres,
    string PrimerApellido,
    string? SegundoApellido,
    string NombreCompleto,
    DateOnly FechaNacimiento,
    string Genero,
    decimal IngresosMensuales,
    string MonedaIngresos,
    IReadOnlyList<CuentaRespuesta> Cuentas);
