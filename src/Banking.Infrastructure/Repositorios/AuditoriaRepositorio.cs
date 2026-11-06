using Banking.Application.Abstracciones;
using Banking.Domain.Entidades;
using Banking.Infrastructure.Persistencia;
using Microsoft.EntityFrameworkCore;

namespace Banking.Infrastructure.Repositorios;

internal sealed class AuditoriaRepositorio(BankingDbContext contexto) : IAuditoriaRepositorio
{
    public void Agregar(Auditoria auditoria) => contexto.Auditorias.Add(auditoria);

    public async Task<ResultadoPaginado<Auditoria>> ConsultarAsync(
        ConsultaAuditoria consulta,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Auditoria> eventos = contexto.Auditorias
            .AsNoTracking()
            .Include(auditoria => auditoria.CuentaBancaria);

        if (consulta.DesdeUtc is { } desde)
        {
            eventos = eventos.Where(auditoria => auditoria.FechaHora >= desde);
        }

        if (consulta.HastaUtc is { } hasta)
        {
            eventos = eventos.Where(auditoria => auditoria.FechaHora < hasta);
        }

        if (consulta.Resultado is { } resultado)
        {
            eventos = eventos.Where(auditoria => auditoria.Resultado == resultado);
        }

        if (consulta.NumeroCuenta is { } numeroCuenta)
        {
            eventos = eventos.Where(auditoria =>
                auditoria.CuentaBancaria != null && auditoria.CuentaBancaria.NumeroCuenta == numeroCuenta);
        }

        var total = await eventos.CountAsync(cancellationToken);

        var elementos = await eventos
            .OrderByDescending(auditoria => auditoria.FechaHora)
            .ThenByDescending(auditoria => auditoria.Id)
            .Skip((consulta.Pagina - 1) * consulta.TamanoPagina)
            .Take(consulta.TamanoPagina)
            .ToListAsync(cancellationToken);

        return new ResultadoPaginado<Auditoria>(elementos, total);
    }
}
