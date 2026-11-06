using Banking.Domain.Comun;
using Banking.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Banking.Infrastructure.Persistencia.Configuraciones;

internal sealed class CuentaBancariaConfiguracion : IEntityTypeConfiguration<CuentaBancaria>
{
    public void Configure(EntityTypeBuilder<CuentaBancaria> builder)
    {
        builder.ToTable("CuentasBancarias", tabla =>
        {
            tabla.HasCheckConstraint("CK_CuentasBancarias_Saldo", "\"Saldo\" >= 0");
            tabla.HasCheckConstraint(
                "CK_CuentasBancarias_NumeroCuenta",
                $"length(\"NumeroCuenta\") = {FormatoNumeroCuenta.Longitud}");
        });

        builder.HasKey(c => c.Id);

        builder.Property(c => c.NumeroCuenta)
            .HasMaxLength(FormatoNumeroCuenta.Longitud)
            .IsRequired();

        builder.HasIndex(c => c.NumeroCuenta).IsUnique();

        // Concurrencia optimista sin columna de versión: el UPDATE incluye en su WHERE
        // los valores originales de Saldo y EstadoCuentaId. Si otro proceso los cambió,
        // EF Core lanza DbUpdateConcurrencyException (se traducirá a HTTP 409).
        builder.Property(c => c.Saldo)
            .IsRequired()
            .IsConcurrencyToken();

        builder.Property(c => c.EstadoCuentaId).IsConcurrencyToken();

        builder.Property(c => c.FechaCreacion).IsRequired();

        builder.HasOne(c => c.Cliente)
            .WithMany()
            .HasForeignKey(c => c.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.TipoCuenta)
            .WithMany()
            .HasForeignKey(c => c.TipoCuentaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.EstadoCuenta)
            .WithMany()
            .HasForeignKey(c => c.EstadoCuentaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Moneda)
            .WithMany()
            .HasForeignKey(c => c.MonedaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(c => c.EstaActiva);
    }
}