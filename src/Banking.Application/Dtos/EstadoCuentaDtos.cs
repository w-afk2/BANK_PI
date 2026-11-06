namespace Banking.Application.Dtos;

/// <summary>Funcionalidad adicional: cambiar la cuenta entre Activa (1) e Inactiva (2).</summary>
public sealed record CambiarEstadoSolicitud(int EstadoCuentaId);
