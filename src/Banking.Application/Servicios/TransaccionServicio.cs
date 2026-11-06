using Banking.Application.Abstracciones;
using Banking.Application.Dtos;
using Banking.Application.Excepciones;
using Banking.Domain.Entidades;
using Banking.Domain.Enumeraciones;
using Banking.Domain.Excepciones;

namespace Banking.Application.Servicios;

public sealed class TransaccionServicio(
    ICuentaBancariaRepositorio cuentas,
    ITransaccionRepositorio transacciones,
    IAuditoriaRepositorio auditorias,
    RegistradorAuditoria registradorAuditoria,
    IUnidadDeTrabajo unidadDeTrabajo,
    IRelojSistema reloj) : ITransaccionServicio
{
    /// <summary>RF-04.</summary>
    public Task<TransaccionRespuesta> DepositarAsync(
        string numeroCuenta,
        OperacionSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(solicitud);

        return EjecutarOperacionAsync(
            numeroCuenta,
            AccionAuditoria.Depositar,
            "Depósito registrado.",
            (cuenta, ahora) => cuenta.Depositar(solicitud.Monto, ahora),
            cancellationToken);
    }

    /// <summary>RF-05.</summary>
    public Task<TransaccionRespuesta> RetirarAsync(
        string numeroCuenta,
        OperacionSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(solicitud);

        return EjecutarOperacionAsync(
            numeroCuenta,
            AccionAuditoria.Retirar,
            "Retiro registrado.",
            (cuenta, ahora) => cuenta.Retirar(solicitud.Monto, ahora),
            cancellationToken);
    }

    /// <summary>RF-06. Orden cronológico ascendente.</summary>
    public async Task<HistorialRespuesta> ObtenerHistorialAsync(
        string numeroCuenta,
        CancellationToken cancellationToken = default)
    {
        var cuenta = await cuentas.ObtenerPorNumeroAsync(numeroCuenta, cancellationToken)
            ?? throw new RecursoNoEncontradoException("La cuenta indicada no existe.");

        var movimientos = await transacciones.ObtenerPorCuentaAsync(cuenta.Id, cancellationToken);

        return new HistorialRespuesta(
            cuenta.NumeroCuenta,
            cuenta.Moneda?.Codigo ?? string.Empty,
            cuenta.Moneda?.Simbolo ?? string.Empty,
            cuenta.Saldo,
            movimientos.Select(movimiento => movimiento.ARespuesta(cuenta)).ToList());
    }

    /// <summary>
    /// Flujo común de depósito y retiro:
    /// 1. Cuenta inexistente: se audita el rechazo y se responde 404.
    /// 2. Fondos insuficientes o cuenta inactiva: se descartan los cambios, se audita el rechazo
    ///    en una escritura independiente y se relanza la excepción original.
    /// 3. Éxito: saldo, transacción y auditoría se guardan en una sola transacción. Un conflicto de
    ///    concurrencia revierte todo y se traduce a 409, sin reintentos.
    /// </summary>
    private async Task<TransaccionRespuesta> EjecutarOperacionAsync(
        string numeroCuenta,
        AccionAuditoria accion,
        string descripcionExito,
        Func<CuentaBancaria, DateTime, Transaccion> operacion,
        CancellationToken cancellationToken)
    {
        var cuenta = await cuentas.ObtenerPorNumeroAsync(numeroCuenta, cancellationToken);
        if (cuenta is null)
        {
            await registradorAuditoria.RegistrarRechazoAsync(
                accion,
                "Operación rechazada: la cuenta indicada no existe.",
                clienteId: null,
                cuentaBancariaId: null,
                cancellationToken);

            throw new RecursoNoEncontradoException("La cuenta indicada no existe.");
        }

        var ahora = reloj.AhoraUtc;
        Transaccion transaccion;

        try
        {
            transaccion = operacion(cuenta, ahora);
        }
        catch (ExcepcionDominio excepcion) when (excepcion is FondosInsuficientesException or CuentaInactivaException)
        {
            await registradorAuditoria.RegistrarRechazoAsync(
                accion,
                $"Operación rechazada: {excepcion.Message}",
                cuenta.ClienteId,
                cuenta.Id,
                cancellationToken);

            throw;
        }

        transacciones.Agregar(transaccion);

        auditorias.Agregar(Auditoria.RegistrarConEntidades(
            accion,
            ResultadoAuditoria.Exitoso,
            descripcionExito,
            ahora,
            cuenta: cuenta));

        await unidadDeTrabajo.GuardarCambiosAsync(cancellationToken);

        // Recarga para incluir el nombre del tipo de transacción.
        var transaccionGuardada = await transacciones.ObtenerPorIdAsync(transaccion.Id, cancellationToken)
            ?? transaccion;

        return transaccionGuardada.ARespuesta(cuenta);
    }
}
