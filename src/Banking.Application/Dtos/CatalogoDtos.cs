namespace Banking.Application.Dtos;

/// <summary>Elemento de un catálogo simple (TipoCuenta, EstadoCuenta).</summary>
public sealed record CatalogoRespuesta(int Id, string Nombre);

public sealed record MonedaRespuesta(int Id, string Codigo, string Nombre, string Simbolo);
