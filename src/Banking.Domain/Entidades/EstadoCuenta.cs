namespace Banking.Domain.Entidades;

/// <summary>Catálogo con datos fijos (HasData).</summary>
public sealed class EstadoCuenta
{
    public static class Ids
    {
        public const int Activa = 1;
        public const int Inactiva = 2;
    }

    private EstadoCuenta()
    {
    }

    public int Id { get; private set; }

    public string Nombre { get; private set; } = string.Empty;
}