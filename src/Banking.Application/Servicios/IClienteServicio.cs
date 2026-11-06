using Banking.Application.Dtos;

namespace Banking.Application.Servicios;

/// <summary>Operaciones 1 y 2: registrar cliente y consultar su perfil.</summary>
public interface IClienteServicio
{
    Task<ClienteRespuesta> RegistrarAsync(RegistrarClienteSolicitud solicitud, CancellationToken cancellationToken = default);

    Task<ClienteRespuesta> ObtenerPerfilAsync(int id, CancellationToken cancellationToken = default);
}
