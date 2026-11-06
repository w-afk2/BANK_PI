using Banking.Application.Abstracciones;
using Banking.Application.Excepciones;
using Banking.Domain.Entidades;
using Banking.Domain.Enumeraciones;
using Banking.Infrastructure.Persistencia;
using Banking.Infrastructure.Repositorios;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Banking.Tests.Integracion;

/// <summary>
/// Concurrencia optimista con SQLite en memoria y el esquema real (migraciones).
/// Dos DbContext independientes simulan dos solicitudes simultáneas sobre la misma cuenta.
/// </summary>
public sealed class ConcurrenciaCuentaTests : IDisposable
{
    private const string NumeroCuenta = "ACC-20261106-0001";
    private const decimal SaldoInicial = 1000m;

    private readonly SqliteConnection _conexion;

    public ConcurrenciaCuentaTests()
    {
        _conexion = new SqliteConnection("DataSource=:memory:");
        _conexion.Open();

        using var contexto = CrearContexto();
        contexto.Database.Migrate();

        var cliente = Cliente.Registrar(
            "Ana", "López", null, new DateOnly(1995, 4, 12), Genero.Femenino, 25000m, new DateOnly(2026, 11, 6));
        contexto.Clientes.Add(cliente);
        contexto.SaveChanges();

        var apertura = CuentaBancaria.Abrir(
            cliente.Id, TipoCuenta.Ids.Ahorro, Moneda.Ids.NIO, NumeroCuenta, SaldoInicial, DateTime.UtcNow);
        contexto.CuentasBancarias.Add(apertura.Cuenta);
        contexto.Transacciones.Add(apertura.DepositoInicial!);
        contexto.SaveChanges();
    }

    [Fact]
    public async Task RetirosSimultaneos_ElSegundoRecibeConflictoYNoDejaDatosInconsistentes()
    {
        await using var contextoA = CrearContexto();
        await using var contextoB = CrearContexto();

        // Ambas solicitudes leen la cuenta con saldo 1000.
        var cuentaA = await ObtenerCuentaAsync(contextoA);
        var cuentaB = await ObtenerCuentaAsync(contextoB);

        contextoA.Transacciones.Add(cuentaA.Retirar(700m, DateTime.UtcNow));
        await CrearUnidad(contextoA).GuardarCambiosAsync();

        // B aún ve 1000: el dominio permite el retiro, pero el token de concurrencia (Saldo) lo impide.
        contextoB.Transacciones.Add(cuentaB.Retirar(700m, DateTime.UtcNow));
        contextoB.Auditorias.Add(Auditoria.RegistrarConEntidades(
            AccionAuditoria.Retirar, ResultadoAuditoria.Exitoso, "Retiro registrado.", DateTime.UtcNow, cuenta: cuentaB));

        await Assert.ThrowsAsync<ConflictoConcurrenciaException>(
            () => CrearUnidad(contextoB).GuardarCambiosAsync());

        await using var verificacion = CrearContexto();
        var cuenta = await ObtenerCuentaAsync(verificacion);
        var movimientos = await ObtenerMovimientosAsync(verificacion, cuenta.Id);

        Assert.Equal(300m, cuenta.Saldo);                   // solo se aplicó el retiro de A
        Assert.Equal(2, movimientos.Count);                 // depósito inicial + retiro de A
        Assert.Equal(300m, movimientos[^1].SaldoResultante);
        Assert.Equal(0, await verificacion.Auditorias.CountAsync()); // la auditoría de B se revirtió con todo lo demás
    }

    [Fact]
    public async Task DepositoConEstadoDesactualizado_SeRechazaSiOtraOperacionInactivoLaCuenta()
    {
        await using var contextoA = CrearContexto();
        await using var contextoB = CrearContexto();

        var cuentaA = await ObtenerCuentaAsync(contextoA);
        var cuentaB = await ObtenerCuentaAsync(contextoB);

        cuentaA.CambiarEstado(EstadoCuenta.Ids.Inactiva);
        await CrearUnidad(contextoA).GuardarCambiosAsync();

        // B todavía ve la cuenta Activa: el token EstadoCuentaId impide guardar el depósito.
        contextoB.Transacciones.Add(cuentaB.Depositar(500m, DateTime.UtcNow));

        await Assert.ThrowsAsync<ConflictoConcurrenciaException>(
            () => CrearUnidad(contextoB).GuardarCambiosAsync());

        await using var verificacion = CrearContexto();
        var cuenta = await ObtenerCuentaAsync(verificacion);
        var movimientos = await ObtenerMovimientosAsync(verificacion, cuenta.Id);

        Assert.Equal(SaldoInicial, cuenta.Saldo);
        Assert.Equal(EstadoCuenta.Ids.Inactiva, cuenta.EstadoCuentaId);
        Assert.Single(movimientos);                          // solo el depósito inicial
    }

    [Fact]
    public async Task OperacionesSecuenciales_NoGeneranConflicto()
    {
        await using (var contextoA = CrearContexto())
        {
            var cuenta = await ObtenerCuentaAsync(contextoA);
            contextoA.Transacciones.Add(cuenta.Retirar(200m, DateTime.UtcNow));
            await CrearUnidad(contextoA).GuardarCambiosAsync();
        }

        await using (var contextoB = CrearContexto())
        {
            // B lee después de que A guardó, con el saldo actualizado.
            var cuenta = await ObtenerCuentaAsync(contextoB);
            contextoB.Transacciones.Add(cuenta.Retirar(300m, DateTime.UtcNow));
            await CrearUnidad(contextoB).GuardarCambiosAsync();
        }

        await using var verificacion = CrearContexto();
        var final = await ObtenerCuentaAsync(verificacion);

        Assert.Equal(500m, final.Saldo);
        Assert.Equal(3, (await ObtenerMovimientosAsync(verificacion, final.Id)).Count);
    }

    public void Dispose() => _conexion.Dispose();

    private BankingDbContext CrearContexto()
    {
        return new BankingDbContext(
            new DbContextOptionsBuilder<BankingDbContext>().UseSqlite(_conexion).Options);
    }

    private static UnidadDeTrabajo CrearUnidad(BankingDbContext contexto)
    {
        return new UnidadDeTrabajo(contexto, NullLogger<UnidadDeTrabajo>.Instance);
    }

    private static Task<CuentaBancaria> ObtenerCuentaAsync(BankingDbContext contexto)
    {
        return contexto.CuentasBancarias.SingleAsync(cuenta => cuenta.NumeroCuenta == NumeroCuenta);
    }

    private static Task<List<Transaccion>> ObtenerMovimientosAsync(BankingDbContext contexto, int cuentaId)
    {
        return contexto.Transacciones
            .Where(transaccion => transaccion.CuentaBancariaId == cuentaId)
            .OrderBy(transaccion => transaccion.Id)
            .ToListAsync();
    }
}