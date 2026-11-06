using Banking.Application.Abstracciones;
using Banking.Application.Dtos;
using Banking.Domain.Excepciones;

namespace Banking.Application.Servicios;

public sealed class AuditoriaServicio(IAuditoriaRepositorio auditorias) : IAuditoriaServicio
{
    public const int TamanoPaginaMaximo = 100;

    public async Task<PaginaRespuesta<AuditoriaRespuesta>> ConsultarAsync(
        FiltroAuditoriaSolicitud filtro,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filtro);
        Validar(filtro);

        var consulta = new ConsultaAuditoria(
            filtro.Desde?.UtcDateTime,
            filtro.Hasta?.UtcDateTime,
            filtro.Resultado,
            string.IsNullOrWhiteSpace(filtro.NumeroCuenta) ? null : filtro.NumeroCuenta.Trim(),
            filtro.Pagina,
            filtro.TamanoPagina);

        var resultado = await auditorias.ConsultarAsync(consulta, cancellationToken);
        var totalPaginas = (int)Math.Ceiling(resultado.Total / (double)filtro.TamanoPagina);

        return new PaginaRespuesta<AuditoriaRespuesta>(
            resultado.Elementos.Select(auditoria => auditoria.ARespuesta()).ToList(),
            filtro.Pagina,
            filtro.TamanoPagina,
            resultado.Total,
            totalPaginas);
    }

    private static void Validar(FiltroAuditoriaSolicitud filtro)
    {
        if (filtro.Pagina < 1)
        {
            throw new ValidacionException("La página debe ser mayor o igual a 1.");
        }

        if (filtro.TamanoPagina is < 1 or > TamanoPaginaMaximo)
        {
            throw new ValidacionException($"El tamaño de página debe estar entre 1 y {TamanoPaginaMaximo}.");
        }

        if ((long)(filtro.Pagina - 1) * filtro.TamanoPagina > int.MaxValue)
        {
            throw new ValidacionException("La página solicitada está fuera de rango.");
        }

        if (filtro.Desde is not null && filtro.Hasta is not null && filtro.Desde > filtro.Hasta)
        {
            throw new ValidacionException("La fecha inicial no puede ser posterior a la fecha final.");
        }
    }
}
