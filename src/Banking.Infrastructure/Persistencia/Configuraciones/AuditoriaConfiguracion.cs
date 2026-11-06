using Banking.Domain.Entidades;
using Banking.Domain.Enumeraciones;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Banking.Infrastructure.Persistencia.Configuraciones;

internal sealed class AuditoriaConfiguracion : IEntityTypeConfiguration<Auditoria>
{
    private const int LongitudMaximaAccion = 30;
    private const int LongitudMaximaResultado = 20;

    public void Configure(EntityTypeBuilder<Auditoria> builder)
    {
        builder.ToTable("Auditorias", tabla =>
        {
            tabla.HasCheckConstraint("CK_Auditorias_Accion", $"\"Accion\" IN ({ValoresPermitidos<AccionAuditoria>()})");
            tabla.HasCheckConstraint("CK_Auditorias_Resultado", $"\"Resultado\" IN ({ValoresPermitidos<ResultadoAuditoria>()})");
        });

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Accion)
            .HasConversion<string>()
            .HasMaxLength(LongitudMaximaAccion)
            .IsRequired();

        builder.Property(a => a.Resultado)
            .HasConversion<string>()
            .HasMaxLength(LongitudMaximaResultado)
            .IsRequired();

        builder.Property(a => a.Descripcion)
            .HasMaxLength(Auditoria.LongitudMaximaDescripcion)
            .IsRequired();

        builder.Property(a => a.FechaHora).IsRequired();

        // Índices para la consulta paginada con filtros (fecha, resultado, cuenta).
        builder.HasIndex(a => a.FechaHora);
        builder.HasIndex(a => a.Resultado);

        builder.HasOne(a => a.Cliente)
            .WithMany()
            .HasForeignKey(a => a.ClienteId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.CuentaBancaria)
            .WithMany()
            .HasForeignKey(a => a.CuentaBancariaId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static string ValoresPermitidos<TEnum>()
        where TEnum : struct, Enum
    {
        return string.Join(", ", Enum.GetNames<TEnum>().Select(nombre => $"'{nombre}'"));
    }
}