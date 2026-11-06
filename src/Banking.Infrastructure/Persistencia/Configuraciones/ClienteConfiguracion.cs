using Banking.Domain.Entidades;
using Banking.Domain.Enumeraciones;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Banking.Infrastructure.Persistencia.Configuraciones;

internal sealed class ClienteConfiguracion : IEntityTypeConfiguration<Cliente>
{
    private const int LongitudMaximaGenero = 20;

    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        var generosPermitidos = string.Join(", ", Enum.GetNames<Genero>().Select(nombre => $"'{nombre}'"));

        builder.ToTable("Clientes", tabla =>
        {
            tabla.HasCheckConstraint("CK_Clientes_IngresosMensuales", "\"IngresosMensuales\" >= 0");
            tabla.HasCheckConstraint("CK_Clientes_Genero", $"\"Genero\" IN ({generosPermitidos})");
        });

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Nombres)
            .HasMaxLength(Cliente.LongitudMaximaNombres)
            .IsRequired();

        builder.Property(c => c.PrimerApellido)
            .HasMaxLength(Cliente.LongitudMaximaApellido)
            .IsRequired();

        builder.Property(c => c.SegundoApellido)
            .HasMaxLength(Cliente.LongitudMaximaApellido);

        builder.Property(c => c.FechaNacimiento).IsRequired();

        builder.Property(c => c.Genero)
            .HasConversion<string>()
            .HasMaxLength(LongitudMaximaGenero)
            .IsRequired();

        builder.Property(c => c.IngresosMensuales).IsRequired();

        builder.Ignore(c => c.NombreCompleto);
    }
}