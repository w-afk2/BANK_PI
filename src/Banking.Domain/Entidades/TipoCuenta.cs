namespace Banking.Domain.Entidades;

/// <summary>Catálogo con datos fijos (HasData).</summary>
public sealed class TipoCuenta
{
    public static class Ids
    {
        public const int Ahorro = 1;
        public const int Corriente = 2;
    }

    private TipoCuenta()
    {
    }

    public int Id { get; private set; }

    public string Nombre { get; private set; } = string.Empty;
}