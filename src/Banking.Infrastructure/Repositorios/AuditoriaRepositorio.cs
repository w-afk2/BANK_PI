using Banking.Application.Abstracciones;
using Banking.Domain.Entidades;
using Banking.Infrastructure.Persistencia;

namespace Banking.Infrastructure.Repositorios;

internal sealed class AuditoriaRepositorio(BankingDbContext contexto) : IAuditoriaRepositorio
{
    public void Agregar(Auditoria auditoria) => contexto.Auditorias.Add(auditoria);
}
