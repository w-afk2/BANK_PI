using Banking.Domain.Entidades;
using Banking.Infrastructure.Persistencia.Convertidores;
using Microsoft.EntityFrameworkCore;

namespace Banking.Infrastructure.Persistencia;

public class BankingDbContext(DbContextOptions<BankingDbContext> options) : DbContext(options)
{
    public DbSet<Cliente> Clientes => Set<Cliente>();

    public DbSet<CuentaBancaria> CuentasBancarias => Set<CuentaBancaria>();

    public DbSet<TipoCuenta> TiposCuenta => Set<TipoCuenta>();

    public DbSet<EstadoCuenta> EstadosCuenta => Set<EstadoCuenta>();

    public DbSet<Moneda> Monedas => Set<Moneda>();

    public DbSet<Transaccion> Transacciones => Set<Transaccion>();

    public DbSet<TipoTransaccion> TiposTransaccion => Set<TipoTransaccion>();

    public DbSet<Auditoria> Auditorias => Set<Auditoria>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BankingDbContext).Assembly);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // Saldo, Monto, SaldoResultante e IngresosMensuales: enteros en centavos.
        configurationBuilder.Properties<decimal>().HaveConversion<ConvertidorCentavos>();

        // FechaCreacion y FechaHora: UTC al leer.
        configurationBuilder.Properties<DateTime>().HaveConversion<ConvertidorFechaUtc>();
    }
}