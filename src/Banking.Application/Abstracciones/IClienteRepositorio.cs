using Banking.Domain.Entidades;

namespace Banking.Application.Abstracciones;

public interface IClienteRepositorio
{
    void Agregar(Cliente cliente);

    Task<Cliente?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);

    Task<bool> ExisteAsync(int id, CancellationToken cancellationToken = default);
}
