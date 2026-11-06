namespace Banking.Domain.Entidades;

/// <summary>Catálogo con datos fijos (HasData). No se realizan conversiones entre monedas.</summary>
public sealed class Moneda
{
    public static class Ids
    {
        public const int NIO = 1;
        public const int USD = 2;
    }

    private Moneda()
    {
    }

    public int Id { get; private set; }

    /// <summary>Código ISO 4217.</summary>
    public string Codigo { get; private set; } = string.Empty;

    public string Nombre { get; private set; } = string.Empty;

    public string Simbolo { get; private set; } = string.Empty;
}