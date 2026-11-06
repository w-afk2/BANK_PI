using Banking.Application.Abstracciones;
using Banking.Application.Dtos;

namespace Banking.Application.Servicios;

public sealed class CatalogoServicio(ICatalogoRepositorio catalogos) : ICatalogoServicio
{
    public async Task<IReadOnlyList<CatalogoRespuesta>> ObtenerTiposCuentaAsync(CancellationToken cancellationToken = default)
    {
        var tipos = await catalogos.ListarTiposCuentaAsync(cancellationToken);
        return tipos.Select(tipo => tipo.ARespuesta()).ToList();
    }

    public async Task<IReadOnlyList<CatalogoRespuesta>> ObtenerEstadosCuentaAsync(CancellationToken cancellationToken = default)
    {
        var estados = await catalogos.ListarEstadosCuentaAsync(cancellationToken);
        return estados.Select(estado => estado.ARespuesta()).ToList();
    }

    public async Task<IReadOnlyList<MonedaRespuesta>> ObtenerMonedasAsync(CancellationToken cancellationToken = default)
    {
        var monedas = await catalogos.ListarMonedasAsync(cancellationToken);
        return monedas.Select(moneda => moneda.ARespuesta()).ToList();
    }
}
