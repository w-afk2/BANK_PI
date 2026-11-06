using Banking.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Banking.Infrastructure.Persistencia.Configuraciones;

internal sealed class TransaccionConfiguracion : IEntityTypeConfiguration<Transaccion>
{
    public void Configure(EntityTypeBuilder<Transaccion> builder)
    {
        builder.ToTable("Transacciones", tabla =>
        {
            tabla.HasCheckConstraint("CK_Transacciones_Monto", "\"Monto\" > 0");
            tabla.HasCheckConstraint("CK_Transacciones_SaldoResultante", "\"SaldoResultante\" >= 0");
        });

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Monto).IsRequired();
        builder.Property(t => t.SaldoResultante).IsRequired();
        builder.Property(t => t.FechaHora).IsRequired();

        // Historial: filtro por cuenta y orden ascendente por FechaHora (desempate por Id).
        builder.HasIndex(t => new { t.CuentaBancariaId, t.FechaHora });

        builder.HasOne(t => t.CuentaBancaria)
            .WithMany()
            .HasForeignKey(t => t.CuentaBancariaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.TipoTransaccion)
            .WithMany()
            .HasForeignKey(t => t.TipoTransaccionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}