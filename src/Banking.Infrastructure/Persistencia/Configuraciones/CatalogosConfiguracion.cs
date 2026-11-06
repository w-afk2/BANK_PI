using Banking.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Banking.Infrastructure.Persistencia.Configuraciones;

// Catálogos con identificadores fijos: el dominio los referencia mediante las constantes Ids.

internal sealed class TipoCuentaConfiguracion : IEntityTypeConfiguration<TipoCuenta>
{
    public void Configure(EntityTypeBuilder<TipoCuenta> builder)
    {
        builder.ToTable("TiposCuenta");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedNever();
        builder.Property(t => t.Nombre).HasMaxLength(30).IsRequired();
        builder.HasIndex(t => t.Nombre).IsUnique();

        builder.HasData(
            new { Id = TipoCuenta.Ids.Ahorro, Nombre = "Ahorro" },
            new { Id = TipoCuenta.Ids.Corriente, Nombre = "Corriente" });
    }
}

internal sealed class EstadoCuentaConfiguracion : IEntityTypeConfiguration<EstadoCuenta>
{
    public void Configure(EntityTypeBuilder<EstadoCuenta> builder)
    {
        builder.ToTable("EstadosCuenta");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.Nombre).HasMaxLength(30).IsRequired();
        builder.HasIndex(e => e.Nombre).IsUnique();

        builder.HasData(
            new { Id = EstadoCuenta.Ids.Activa, Nombre = "Activa" },
            new { Id = EstadoCuenta.Ids.Inactiva, Nombre = "Inactiva" });
    }
}

internal sealed class MonedaConfiguracion : IEntityTypeConfiguration<Moneda>
{
    public void Configure(EntityTypeBuilder<Moneda> builder)
    {
        builder.ToTable("Monedas");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).ValueGeneratedNever();
        builder.Property(m => m.Codigo).HasMaxLength(3).IsRequired();
        builder.Property(m => m.Nombre).HasMaxLength(50).IsRequired();
        builder.Property(m => m.Simbolo).HasMaxLength(5).IsRequired();
        builder.HasIndex(m => m.Codigo).IsUnique();

        builder.HasData(
            new { Id = Moneda.Ids.NIO, Codigo = "NIO", Nombre = "Córdoba nicaragüense", Simbolo = "C$" },
            new { Id = Moneda.Ids.USD, Codigo = "USD", Nombre = "Dólar estadounidense", Simbolo = "US$" });
    }
}

internal sealed class TipoTransaccionConfiguracion : IEntityTypeConfiguration<TipoTransaccion>
{
    public void Configure(EntityTypeBuilder<TipoTransaccion> builder)
    {
        builder.ToTable("TiposTransaccion");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedNever();
        builder.Property(t => t.Nombre).HasMaxLength(30).IsRequired();
        builder.HasIndex(t => t.Nombre).IsUnique();

        builder.HasData(
            new { Id = TipoTransaccion.Ids.Deposito, Nombre = "Depósito" },
            new { Id = TipoTransaccion.Ids.Retiro, Nombre = "Retiro" });
    }
}