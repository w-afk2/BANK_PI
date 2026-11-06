using Banking.Application.Servicios;
using Microsoft.Extensions.DependencyInjection;

namespace Banking.Application;

public static class DependencyInjection
{
    /// <summary>Registra los servicios de aplicación (casos de uso).</summary>
    public static IServiceCollection AgregarAplicacion(this IServiceCollection services)
    {
        services.AddScoped<IGeneradorNumeroCuenta, GeneradorNumeroCuenta>();
        services.AddScoped<RegistradorAuditoria>();
        services.AddScoped<IClienteServicio, ClienteServicio>();
        services.AddScoped<ICuentaBancariaServicio, CuentaBancariaServicio>();
        services.AddScoped<ITransaccionServicio, TransaccionServicio>();

        return services;
    }
}
