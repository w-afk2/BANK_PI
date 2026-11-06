namespace Banking.Application.Servicios;

/// <summary>Genera números con formato ACC-YYYYMMDD-XXXX.</summary>
public interface IGeneradorNumeroCuenta
{
    Task<string> GenerarAsync(CancellationToken cancellationToken = default);
}
