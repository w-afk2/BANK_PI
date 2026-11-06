using Banking.Domain.Enumeraciones;

namespace Banking.Application.Dtos;

/// <summary>
/// Filtros de la consulta de auditoría. Desde es inclusivo y Hasta exclusivo (instantes con zona horaria,
/// por ejemplo 2026-11-06T06:00:00Z). Todos los filtros son opcionales.
/// </summary>
public sealed class FiltroAuditoriaSolicitud
{
    public DateTimeOffset? Desde { get; set; }

    public DateTimeOffset? Hasta { get; set; }

    public ResultadoAuditoria? Resultado { get; set; }

    public string? NumeroCuenta { get; set; }

    public int Pagina { get; set; } = 1;

    public int TamanoPagina { get; set; } = 20;
}

/// <summary>Evento de auditoría sin información sensible: solo identificadores y un mensaje controlado.</summary>
public sealed record AuditoriaRespuesta(
    int Id,
    DateTime FechaHora,
    string Accion,
    string Resultado,
    string Descripcion,
    int? ClienteId,
    string? NumeroCuenta);

public sealed record PaginaRespuesta<T>(
    IReadOnlyList<T> Elementos,
    int Pagina,
    int TamanoPagina,
    int TotalElementos,
    int TotalPaginas);
