using Banking.Application.Dtos;

namespace Banking.Application.Servicios;

/// <summary>Operaciones 3 y 4 (crear cuenta y consultar saldo) y cambio de estado.</summary>
public interface ICuentaBancariaServicio
{
    Task<CuentaRespuesta> CrearAsync(CrearCuentaSolicitud solicitud, CancellationToken cancellationToken = default);

    Task<SaldoRespuesta> ConsultarSaldoAsync(string numeroCuenta, CancellationToken cancellationToken = default);

    Task<CuentaRespuesta> CambiarEstadoAsync(
        string numeroCuenta,
        CambiarEstadoSolicitud solicitud,
        CancellationToken cancellationToken = default);
}
