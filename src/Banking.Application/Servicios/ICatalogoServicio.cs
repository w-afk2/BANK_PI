using Banking.Application.Dtos;

namespace Banking.Application.Servicios;

/// <summary>Lectura de catálogos para la interfaz.</summary>
public interface ICatalogoServicio
{
    Task<IReadOnlyList<CatalogoRespuesta>> ObtenerTiposCuentaAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CatalogoRespuesta>> ObtenerEstadosCuentaAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<MonedaRespuesta>> ObtenerMonedasAsync(CancellationToken cancellationToken = default);
}
