namespace Banking.Domain.Entidades;

/// <summary>Catálogo con datos fijos (HasData).</summary>
public sealed class TipoTransaccion
{
    public static class Ids
    {
        public const int Deposito = 1;
        public const int Retiro = 2;
    }

    private TipoTransaccion()
    {
    }

    public int Id { get; private set; }

    public string Nombre { get; private set; } = string.Empty;
}