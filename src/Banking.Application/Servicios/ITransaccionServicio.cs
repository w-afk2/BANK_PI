using Banking.Application.Dtos;

namespace Banking.Application.Servicios;

/// <summary>Operaciones 5, 6 y 7: depositar, retirar y consultar historial.</summary>
public interface ITransaccionServicio
{
    Task<TransaccionRespuesta> DepositarAsync(
        string numeroCuenta,
        OperacionSolicitud solicitud,
        CancellationToken cancellationToken = default);

    Task<TransaccionRespuesta> RetirarAsync(
        string numeroCuenta,
        OperacionSolicitud solicitud,
        CancellationToken cancellationToken = default);

    Task<HistorialRespuesta> ObtenerHistorialAsync(
        string numeroCuenta,
        CancellationToken cancellationToken = default);
}
