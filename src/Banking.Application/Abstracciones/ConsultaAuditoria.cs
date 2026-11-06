using Banking.Domain.Enumeraciones;

namespace Banking.Application.Abstracciones;

/// <summary>Criterios ya validados para consultar la auditoría (fechas en UTC).</summary>
public sealed record ConsultaAuditoria(
    DateTime? DesdeUtc,
    DateTime? HastaUtc,
    ResultadoAuditoria? Resultado,
    string? NumeroCuenta,
    int Pagina,
    int TamanoPagina);

/// <summary>Una página de resultados y el total de elementos que cumplen los filtros.</summary>
public sealed record ResultadoPaginado<T>(IReadOnlyList<T> Elementos, int Total);
