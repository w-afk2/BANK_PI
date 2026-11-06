using Banking.Domain.Entidades;

namespace Banking.Application.Abstracciones;

public interface IAuditoriaRepositorio
{
    void Agregar(Auditoria auditoria);
}
