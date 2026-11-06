using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Banking.Infrastructure.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombres = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    PrimerApellido = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    SegundoApellido = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    FechaNacimiento = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Genero = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    IngresosMensuales = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.Id);
                    table.CheckConstraint("CK_Clientes_Genero", "\"Genero\" IN ('Masculino', 'Femenino', 'Otro', 'PrefieroNoDecirlo')");
                    table.CheckConstraint("CK_Clientes_IngresosMensuales", "\"IngresosMensuales\" >= 0");
                });

            migrationBuilder.CreateTable(
                name: "EstadosCuenta",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstadosCuenta", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Monedas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Codigo = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Simbolo = table.Column<string>(type: "TEXT", maxLength: 5, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Monedas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TiposCuenta",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposCuenta", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TiposTransaccion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposTransaccion", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CuentasBancarias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ClienteId = table.Column<int>(type: "INTEGER", nullable: false),
                    TipoCuentaId = table.Column<int>(type: "INTEGER", nullable: false),
                    EstadoCuentaId = table.Column<int>(type: "INTEGER", nullable: false),
                    MonedaId = table.Column<int>(type: "INTEGER", nullable: false),
                    NumeroCuenta = table.Column<string>(type: "TEXT", maxLength: 17, nullable: false),
                    Saldo = table.Column<long>(type: "INTEGER", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CuentasBancarias", x => x.Id);
                    table.CheckConstraint("CK_CuentasBancarias_NumeroCuenta", "length(\"NumeroCuenta\") = 17");
                    table.CheckConstraint("CK_CuentasBancarias_Saldo", "\"Saldo\" >= 0");
                    table.ForeignKey(
                        name: "FK_CuentasBancarias_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CuentasBancarias_EstadosCuenta_EstadoCuentaId",
                        column: x => x.EstadoCuentaId,
                        principalTable: "EstadosCuenta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CuentasBancarias_Monedas_MonedaId",
                        column: x => x.MonedaId,
                        principalTable: "Monedas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CuentasBancarias_TiposCuenta_TipoCuentaId",
                        column: x => x.TipoCuentaId,
                        principalTable: "TiposCuenta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Auditorias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ClienteId = table.Column<int>(type: "INTEGER", nullable: true),
                    CuentaBancariaId = table.Column<int>(type: "INTEGER", nullable: true),
                    Accion = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    Resultado = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    FechaHora = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Auditorias", x => x.Id);
                    table.CheckConstraint("CK_Auditorias_Accion", "\"Accion\" IN ('RegistrarCliente', 'CrearCuenta', 'Depositar', 'Retirar', 'CambiarEstadoCuenta')");
                    table.CheckConstraint("CK_Auditorias_Resultado", "\"Resultado\" IN ('Exitoso', 'Rechazado')");
                    table.ForeignKey(
                        name: "FK_Auditorias_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Auditorias_CuentasBancarias_CuentaBancariaId",
                        column: x => x.CuentaBancariaId,
                        principalTable: "CuentasBancarias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Transacciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CuentaBancariaId = table.Column<int>(type: "INTEGER", nullable: false),
                    TipoTransaccionId = table.Column<int>(type: "INTEGER", nullable: false),
                    Monto = table.Column<long>(type: "INTEGER", nullable: false),
                    FechaHora = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SaldoResultante = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transacciones", x => x.Id);
                    table.CheckConstraint("CK_Transacciones_Monto", "\"Monto\" > 0");
                    table.CheckConstraint("CK_Transacciones_SaldoResultante", "\"SaldoResultante\" >= 0");
                    table.ForeignKey(
                        name: "FK_Transacciones_CuentasBancarias_CuentaBancariaId",
                        column: x => x.CuentaBancariaId,
                        principalTable: "CuentasBancarias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Transacciones_TiposTransaccion_TipoTransaccionId",
                        column: x => x.TipoTransaccionId,
                        principalTable: "TiposTransaccion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "EstadosCuenta",
                columns: new[] { "Id", "Nombre" },
                values: new object[,]
                {
                    { 1, "Activa" },
                    { 2, "Inactiva" }
                });

            migrationBuilder.InsertData(
                table: "Monedas",
                columns: new[] { "Id", "Codigo", "Nombre", "Simbolo" },
                values: new object[,]
                {
                    { 1, "NIO", "Córdoba nicaragüense", "C$" },
                    { 2, "USD", "Dólar estadounidense", "US$" }
                });

            migrationBuilder.InsertData(
                table: "TiposCuenta",
                columns: new[] { "Id", "Nombre" },
                values: new object[,]
                {
                    { 1, "Ahorro" },
                    { 2, "Corriente" }
                });

            migrationBuilder.InsertData(
                table: "TiposTransaccion",
                columns: new[] { "Id", "Nombre" },
                values: new object[,]
                {
                    { 1, "Depósito" },
                    { 2, "Retiro" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Auditorias_ClienteId",
                table: "Auditorias",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Auditorias_CuentaBancariaId",
                table: "Auditorias",
                column: "CuentaBancariaId");

            migrationBuilder.CreateIndex(
                name: "IX_Auditorias_FechaHora",
                table: "Auditorias",
                column: "FechaHora");

            migrationBuilder.CreateIndex(
                name: "IX_Auditorias_Resultado",
                table: "Auditorias",
                column: "Resultado");

            migrationBuilder.CreateIndex(
                name: "IX_CuentasBancarias_ClienteId",
                table: "CuentasBancarias",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_CuentasBancarias_EstadoCuentaId",
                table: "CuentasBancarias",
                column: "EstadoCuentaId");

            migrationBuilder.CreateIndex(
                name: "IX_CuentasBancarias_MonedaId",
                table: "CuentasBancarias",
                column: "MonedaId");

            migrationBuilder.CreateIndex(
                name: "IX_CuentasBancarias_NumeroCuenta",
                table: "CuentasBancarias",
                column: "NumeroCuenta",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CuentasBancarias_TipoCuentaId",
                table: "CuentasBancarias",
                column: "TipoCuentaId");

            migrationBuilder.CreateIndex(
                name: "IX_EstadosCuenta_Nombre",
                table: "EstadosCuenta",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Monedas_Codigo",
                table: "Monedas",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TiposCuenta_Nombre",
                table: "TiposCuenta",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TiposTransaccion_Nombre",
                table: "TiposTransaccion",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transacciones_CuentaBancariaId_FechaHora",
                table: "Transacciones",
                columns: new[] { "CuentaBancariaId", "FechaHora" });

            migrationBuilder.CreateIndex(
                name: "IX_Transacciones_TipoTransaccionId",
                table: "Transacciones",
                column: "TipoTransaccionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Auditorias");

            migrationBuilder.DropTable(
                name: "Transacciones");

            migrationBuilder.DropTable(
                name: "CuentasBancarias");

            migrationBuilder.DropTable(
                name: "TiposTransaccion");

            migrationBuilder.DropTable(
                name: "Clientes");

            migrationBuilder.DropTable(
                name: "EstadosCuenta");

            migrationBuilder.DropTable(
                name: "Monedas");

            migrationBuilder.DropTable(
                name: "TiposCuenta");
        }
    }
}
