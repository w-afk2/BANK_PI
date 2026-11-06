using Banking.Application.Abstracciones;
using Banking.Application.Dtos;
using Banking.Application.Excepciones;
using Banking.Domain.Entidades;
using Banking.Domain.Enumeraciones;
using Banking.Domain.Excepciones;

namespace Banking.Application.Servicios;

public sealed class CuentaBancariaServicio(
    IClienteRepositorio clientes,
    ICuentaBancariaRepositorio cuentas,
    ITransaccionRepositorio transacciones,
    ICatalogoRepositorio catalogos,
    IAuditoriaRepositorio auditorias,
    IGeneradorNumeroCuenta generadorNumeroCuenta,
    IUnidadDeTrabajo unidadDeTrabajo,
    IRelojSistema reloj) : ICuentaBancariaServicio
{
    /// <summary>Intentos ante una colisión del número de cuenta por creación concurrente.</summary>
    public const int MaximoIntentosNumeroCuenta = 3;

    /// <summary>
    /// RF-02. Guarda en una sola transacción la cuenta, su depósito inicial (si el saldo inicial es
    /// mayor que cero) y el evento de auditoría. Si el número colisiona, vuelve a consultar el
    /// consecutivo y reintenta hasta <see cref="MaximoIntentosNumeroCuenta"/> veces.
    /// </summary>
    public async Task<CuentaRespuesta> CrearAsync(
        CrearCuentaSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(solicitud);

        if (!await clientes.ExisteAsync(solicitud.ClienteId, cancellationToken))
        {
            throw new RecursoNoEncontradoException("El cliente indicado no existe.");
        }

        if (!await catalogos.ExisteTipoCuentaAsync(solicitud.TipoCuentaId, cancellationToken))
        {
            throw new ValidacionException("El tipo de cuenta indicado no es válido.");
        }

        if (!await catalogos.ExisteMonedaAsync(solicitud.MonedaId, cancellationToken))
        {
            throw new ValidacionException("La moneda indicada no es válida.");
        }

        for (var intento = 1; ; intento++)
        {
            var numeroCuenta = await generadorNumeroCuenta.GenerarAsync(cancellationToken);
            var ahora = reloj.AhoraUtc;

            var apertura = CuentaBancaria.Abrir(
                solicitud.ClienteId,
                solicitud.TipoCuentaId,
                solicitud.MonedaId,
                numeroCuenta,
                solicitud.SaldoInicial,
                ahora);

            cuentas.Agregar(apertura.Cuenta);

            if (apertura.DepositoInicial is not null)
            {
                transacciones.Agregar(apertura.DepositoInicial);
            }

            auditorias.Agregar(Auditoria.RegistrarConEntidades(
                AccionAuditoria.CrearCuenta,
                ResultadoAuditoria.Exitoso,
                "Cuenta bancaria creada.",
                ahora,
                cuenta: apertura.Cuenta));

            try
            {
                await unidadDeTrabajo.GuardarCambiosAsync(cancellationToken);
            }
            catch (NumeroCuentaDuplicadoException) when (intento < MaximoIntentosNumeroCuenta)
            {
                unidadDeTrabajo.DescartarCambios();
                continue;
            }

            // Recarga para obtener el tipo, el estado y la moneda de la cuenta recién creada.
            var cuentaCreada = await cuentas.ObtenerPorNumeroAsync(numeroCuenta, cancellationToken)
                ?? apertura.Cuenta;

            return cuentaCreada.ARespuesta();
        }
    }

    /// <summary>RF-03. Una cuenta inactiva también puede consultarse.</summary>
    public async Task<SaldoRespuesta> ConsultarSaldoAsync(
        string numeroCuenta,
        CancellationToken cancellationToken = default)
    {
        var cuenta = await cuentas.ObtenerPorNumeroAsync(numeroCuenta, cancellationToken)
            ?? throw new RecursoNoEncontradoException("La cuenta indicada no existe.");

        return cuenta.ARespuestaSaldo();
    }
}
