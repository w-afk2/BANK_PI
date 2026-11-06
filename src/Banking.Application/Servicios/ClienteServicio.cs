using Banking.Application.Abstracciones;
using Banking.Application.Dtos;
using Banking.Application.Excepciones;
using Banking.Domain.Entidades;
using Banking.Domain.Enumeraciones;

namespace Banking.Application.Servicios;

public sealed class ClienteServicio(
    IClienteRepositorio clientes,
    ICuentaBancariaRepositorio cuentas,
    IAuditoriaRepositorio auditorias,
    IUnidadDeTrabajo unidadDeTrabajo,
    IRelojSistema reloj) : IClienteServicio
{
    /// <summary>RF-01. El cliente y su evento de auditoría se guardan en la misma transacción.</summary>
    public async Task<ClienteRespuesta> RegistrarAsync(
        RegistrarClienteSolicitud solicitud,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(solicitud);

        var cliente = Cliente.Registrar(
            solicitud.Nombres,
            solicitud.PrimerApellido,
            solicitud.SegundoApellido,
            solicitud.FechaNacimiento,
            solicitud.Genero,
            solicitud.IngresosMensuales,
            reloj.FechaActualManagua);

        clientes.Agregar(cliente);

        auditorias.Agregar(Auditoria.RegistrarConEntidades(
            AccionAuditoria.RegistrarCliente,
            ResultadoAuditoria.Exitoso,
            "Cliente registrado.",
            reloj.AhoraUtc,
            cliente: cliente));

        await unidadDeTrabajo.GuardarCambiosAsync(cancellationToken);

        return cliente.ARespuesta([]);
    }

    /// <summary>Perfil con nombre completo y las cuentas del cliente.</summary>
    public async Task<ClienteRespuesta> ObtenerPerfilAsync(int id, CancellationToken cancellationToken = default)
    {
        var cliente = await clientes.ObtenerPorIdAsync(id, cancellationToken)
            ?? throw new RecursoNoEncontradoException("El cliente indicado no existe.");

        var cuentasCliente = await cuentas.ObtenerPorClienteAsync(id, cancellationToken);

        return cliente.ARespuesta(cuentasCliente.Select(cuenta => cuenta.ARespuesta()).ToList());
    }
}
