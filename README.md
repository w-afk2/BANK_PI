# Banking API — Banca digital

API REST de banca digital desarrollada como prueba técnica para la vacante de Programador Backend.
Permite registrar clientes, crear cuentas bancarias, consultar saldos, depositar, retirar y consultar el
historial de transacciones. Incluye una interfaz web para el operador, documentación interactiva con
Swagger UI y pruebas automatizadas con xUnit.

## Tecnologías

- .NET 10 · C# · ASP.NET Core Web API
- Entity Framework Core 10.0.14 · SQLite
- xUnit
- Swagger UI sobre el documento OpenAPI nativo de .NET
- Interfaz web con HTML, CSS y JavaScript (sin frameworks)

## Requisitos

- SDK de .NET 10 (`dotnet --version` debe mostrar `10.0.x`)
- Opcional: Visual Studio 2026

No se necesita instalar SQLite ni crear la base de datos manualmente.

## Estructura

```
Banking.slnx
src/
  Banking.Domain          Entidades, reglas de negocio y excepciones de dominio (sin dependencias externas)
  Banking.Application     Casos de uso, DTOs, interfaces de repositorios y excepciones de aplicación
  Banking.Infrastructure  EF Core + SQLite: DbContext, configuraciones, migraciones, repositorios
  Banking.Api             Controladores, manejo global de excepciones, Swagger e interfaz web (wwwroot)
tests/
  Banking.Tests           Pruebas unitarias y de integración (xUnit)
```

Las dependencias apuntan hacia el dominio: `Api → Application → Domain` e `Infrastructure → Application/Domain`.
Los controladores no acceden al `DbContext`: usan los servicios de aplicación y devuelven DTOs.

## Ejecución

### Desde la línea de comandos

```bash
git clone https://github.com/w-afk2/BANK_PI.git
cd BANK_PI
dotnet tool restore
dotnet build Banking.slnx
dotnet run --project src/Banking.Api --launch-profile https
```

La consola muestra la dirección en `Now listening on: ...`. Con el perfil `https`:

| Recurso | URL |
|---|---|
| Interfaz web del operador | https://localhost:7050/ |
| Swagger UI | https://localhost:7050/swagger |
| Documento OpenAPI | https://localhost:7050/openapi/v1.json |

Los puertos pueden variar; se configuran en `src/Banking.Api/Properties/launchSettings.json`.
Si el navegador advierte sobre el certificado, ejecute una vez `dotnet dev-certs https --trust`.

### Desde Visual Studio 2026

Abra `Banking.slnx`, seleccione el proyecto **Banking.Api** con el perfil **https** y presione **Ctrl+F5**
(iniciar sin depurar). Con F5 el depurador se detiene en las excepciones de negocio, que son esperadas
y las traduce el manejador global.

### Entornos

| Entorno | Swagger y auditoría | Uso |
|---|---|---|
| `Development` | Disponibles | Valor de `launchSettings.json` (F5, Ctrl+F5, `dotnet run`) |
| `Evaluacion` | Disponibles | Entorno explícito para evaluación local |
| Cualquier otro (por ejemplo `Production`) | **No disponibles** | El proyecto no tiene autenticación |

Para usar el entorno `Evaluacion` en PowerShell:

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Evaluacion"
dotnet run --project src/Banking.Api --no-launch-profile --urls "https://localhost:7050"
```

## Base de datos

- Archivo SQLite: **`src/Banking.Api/Datos/banking.db`**.
- La ruta relativa `Datos/banking.db` de `appsettings.json` se resuelve siempre contra el directorio del
  proyecto `Banking.Api`, fijado al compilar. La API y `dotnet ef` usan el mismo archivo, sin importar
  desde qué carpeta se ejecute el comando. La carpeta `Datos` se crea automáticamente.
- Al iniciar, la API aplica las migraciones pendientes con `MigrateAsync()` (entorno de demostración).
  No se usa `EnsureCreated()`.
- La base de datos no se incluye en el repositorio (`.gitignore`). Para empezar desde cero, detenga la
  API y elimine `src/Banking.Api/Datos/banking.db`.
- Los catálogos se cargan desde la migración: tipos de cuenta (1 Ahorro, 2 Corriente), estados
  (1 Activa, 2 Inactiva), monedas (1 NIO C$, 2 USD US$) y tipos de transacción (Depósito, Retiro).

### Migraciones manuales

```bash
# Aplicar migraciones sin iniciar la API
dotnet ef database update --project src/Banking.Infrastructure --startup-project src/Banking.Api

# Verificar el archivo que usan las herramientas de EF Core
dotnet ef dbcontext info --project src/Banking.Infrastructure --startup-project src/Banking.Api

# Comprobar que el modelo no tiene cambios sin migración
dotnet ef migrations has-pending-model-changes --project src/Banking.Infrastructure --startup-project src/Banking.Api

# Crear una nueva migración (solo si cambia el modelo)
dotnet ef migrations add <Nombre> --project src/Banking.Infrastructure --startup-project src/Banking.Api --output-dir Persistencia/Migraciones
```

`dotnet tool restore` instala la herramienta local `dotnet-ef` declarada en `.config/dotnet-tools.json`.

## Funcionalidades y endpoints

| # | Operación | Método y ruta | Respuesta exitosa |
|---|---|---|---|
| 1 | Registrar cliente | `POST /api/clientes` | 201 |
| 2 | Consultar perfil (incluye sus cuentas) | `GET /api/clientes/{id}` | 200 |
| 3 | Crear cuenta | `POST /api/cuentas` | 201 |
| 4 | Consultar saldo | `GET /api/cuentas/{numeroCuenta}/saldo` | 200 |
| 5 | Depositar | `POST /api/cuentas/{numeroCuenta}/depositos` | 201 |
| 6 | Retirar | `POST /api/cuentas/{numeroCuenta}/retiros` | 201 |
| 7 | Historial de transacciones | `GET /api/cuentas/{numeroCuenta}/transacciones` | 200 |
| A1 | Cambiar estado de una cuenta | `PATCH /api/cuentas/{numeroCuenta}/estado` | 200 |
| A2 | Consultar auditoría (paginada) | `GET /api/auditoria` | 200 |
| — | Catálogos de apoyo | `GET /api/catalogos/tipos-cuenta`, `/estados-cuenta`, `/monedas` | 200 |

**Auditoría:** filtros opcionales `desde` (inclusivo) y `hasta` (exclusivo) como fecha y hora ISO 8601,
`resultado` (`Exitoso` o `Rechazado`), `numeroCuenta`, `pagina` (desde 1) y `tamanoPagina` (1 a 100,
por defecto 20). Los eventos más recientes aparecen primero. Solo disponible en `Development` o `Evaluacion`.

### Reglas principales

- Número de cuenta generado automáticamente: `ACC-YYYYMMDD-XXXX` (17 caracteres), con la fecha de
  America/Managua y un consecutivo diario de 0001 a 9999. Índice único en la base de datos.
- Saldo inicial mayor o igual a 0. Si es mayor que 0, se registra como un depósito en el historial.
- Depósitos y retiros mayores que 0, con un máximo de 2 decimales (se rechazan, no se redondean).
- Un retiro sin fondos suficientes se rechaza sin modificar el saldo ni registrar movimiento.
- Una cuenta inactiva puede consultarse, pero no admite depósitos ni retiros.
- Solicitar el estado actual de una cuenta responde 200 sin modificarla ni registrar auditoría.
- Cada operación guarda su saldo resultante. El historial se ordena de forma ascendente por fecha y hora.
- Montos y saldos en la moneda de la cuenta (NIO o USD), sin conversiones. Ingresos mensuales del cliente en NIO.

### Respuestas de error

Todos los errores se devuelven como `ProblemDetails` (JSON), sin trazas ni detalles internos.

| Código | Casos |
|---|---|
| 400 | Datos de entrada incorrectos, montos inválidos, saldo inicial negativo |
| 404 | Cliente o cuenta inexistente |
| 409 | Fondos insuficientes, cuenta inactiva, conflicto de concurrencia, números de cuenta agotados |
| 500 | Error inesperado (mensaje genérico) |

## Interfaz web

La interfaz (`src/Banking.Api/wwwroot`) consume exclusivamente la API: no accede a SQLite ni repite
reglas bancarias. Envía los datos tal como los escribe el operador (los montos se transmiten sin pasar
por coma flotante) y muestra las respuestas o los errores del backend.

| Pestaña | Operaciones |
|---|---|
| Clientes | Registrar cliente · Consultar perfil con sus cuentas |
| Cuentas | Crear cuenta · Consultar saldo · Cambiar estado |
| Movimientos | Depositar · Retirar · Historial de transacciones |
| Auditoría | Consulta paginada con filtros de fecha, resultado y número de cuenta |

Al registrar un cliente o crear una cuenta, su id o número se copia automáticamente en los demás
formularios. En las tablas, **Seleccionar** hace lo mismo con una cuenta existente.

## Pruebas

### Pruebas automatizadas

```bash
dotnet test
```

Resultado verificado: **total 18 · correctas 18 · fallidas 0 · omitidas 0**.

Los casos se derivaron de los requisitos funcionales y de las reglas de negocio aprobadas:

| Requisito | Prueba | Tipo |
|---|---|---|
| RF-04 Depósito exitoso y saldo actualizado | `CuentaBancariaTests.Depositar_MontoValido_IncrementaSaldoYRegistraTransaccion` | Unitaria |
| RF-05 Retiro exitoso y saldo actualizado | `CuentaBancariaTests.Retirar_ConFondosSuficientes_DisminuyeSaldoYRegistraTransaccion` | Unitaria |
| RF-05 Fondos insuficientes sin modificar el saldo | `CuentaBancariaTests.Retirar_FondosInsuficientes_LanzaExcepcionYNoModificaSaldo` | Unitaria |
| RF-06 Saldo resultante de cada operación | `CuentaBancariaTests.SaldoResultante_ReflejaElSaldoDespuesDeCadaOperacion` | Unitaria |
| Número de cuenta: formato, fecha y rango 0001-9999 | `FormatoNumeroCuentaTests.EsValido_AplicaFormatoYRango` (7 casos) | Unitaria |
| Número de cuenta: primer número del día | `GeneradorNumeroCuentaTests.Generar_SinCuentasDelDia_...` | Unitaria |
| Número de cuenta: unicidad básica (50 números) | `GeneradorNumeroCuentaTests.Generar_VariasCuentasDelMismoDia_...` | Unitaria |
| Número de cuenta: continuidad por fecha | `GeneradorNumeroCuentaTests.Generar_ContinuaDespuesDelUltimoNumeroDelDia_...` | Unitaria |
| Número de cuenta: agotamiento en 9999 | `GeneradorNumeroCuentaTests.Generar_ConsecutivosDelDiaAgotados_...` | Unitaria |
| Concurrencia: retiros simultáneos | `ConcurrenciaCuentaTests.RetirosSimultaneos_...` | Integración |
| Concurrencia: depósito con estado desactualizado | `ConcurrenciaCuentaTests.DepositoConEstadoDesactualizado_...` | Integración |
| Sin falsos conflictos en operaciones secuenciales | `ConcurrenciaCuentaTests.OperacionesSecuenciales_NoGeneranConflicto` | Integración |

- **Unitarias** (`tests/Banking.Tests/Dominio`, `Aplicacion`): validan las reglas sin base de datos.
  El reloj y el repositorio se reemplazan por dobles en memoria (`RelojFijo`, `RepositorioCuentasFalso`).
- **Integración** (`tests/Banking.Tests/Integracion`): SQLite en memoria con el esquema real de las
  migraciones. Dos `DbContext` independientes simulan solicitudes simultáneas sobre la misma cuenta y
  comprueban que la concurrencia optimista (tokens `Saldo` y `EstadoCuentaId`) impide actualizaciones
  perdidas y que una operación rechazada no deja saldos, movimientos ni auditorías parciales.

### Verificación manual de la API (14 casos)

Se verificó contra la API en ejecución, con una base de datos nueva. Resultado: **14 de 14 correctos**.
Para reproducirla, use Swagger UI o la interfaz web en este orden. `{id}` es el id devuelto en el
caso 1 y `{numero}` el número devuelto en el caso 3.

| # | Operación | Datos | Esperado |
|---|---|---|---|
| 1 | `POST /api/clientes` | Cuerpo A | 201; `nombreCompleto` = "Ana María López Pérez" |
| 2 | `GET /api/clientes/{id}` | — | 200; `cuentas` vacío |
| 3 | `POST /api/cuentas` | Cuerpo B | 201; número `ACC-<fecha de hoy>-XXXX`, 17 caracteres; saldo 1000 |
| 3b | `POST /api/cuentas` | Cuerpo B con `"tipoCuentaId": 2, "monedaId": 2, "saldoInicial": 0` | 201; consecutivo siguiente; su historial no tiene movimientos |
| 4 | `POST /api/cuentas/{numero}/depositos` | `{ "monto": 500.25 }` | 201; `saldoResultante` 1500.25 |
| 5 | `POST /api/cuentas/{numero}/retiros` | `{ "monto": 200 }` | 201; `saldoResultante` 1300.25 |
| 6 | `POST /api/cuentas/{numero}/retiros` | `{ "monto": 999999 }` | 409; fondos insuficientes |
| 7 | `POST /api/cuentas/{numero}/depositos` | `{ "monto": 10.555 }` | 400; máximo 2 decimales |
| 8 | `POST /api/cuentas/{numero}/depositos` | `{ "monto": 0 }` | 400; debe ser mayor que cero |
| 9 | `GET /api/cuentas/{numero}/saldo` | — | 200; saldo 1300.25 (sin cambios por 6, 7 y 8) |
| 10 | `GET /api/cuentas/{numero}/transacciones` | — | 200; 3 movimientos en orden: Depósito 1000 → 1000, Depósito 500.25 → 1500.25, Retiro 200 → 1300.25 |
| 11 | `GET /api/cuentas/ACC-20990101-0001/saldo` | — | 404 |
| 12 | `POST /api/cuentas` | Cuerpo B con `"clienteId": 999999` | 404 |
| 13 | `POST /api/cuentas` | Cuerpo B con `"saldoInicial": -5` | 400 |

**Cuerpo A**

```json
{ "nombres": "Ana María", "primerApellido": "López", "segundoApellido": "Pérez", "fechaNacimiento": "1995-04-12", "genero": "Femenino", "ingresosMensuales": 25000.00 }
```

**Cuerpo B** (reemplace `clienteId` por el id del caso 1)

```json
{ "clienteId": 1, "tipoCuentaId": 1, "monedaId": 1, "saldoInicial": 1000.00 }
```

### Verificación de las funcionalidades adicionales

| # | Operación | Esperado |
|---|---|---|
| 15 | `PATCH /api/cuentas/{numero}/estado` con `{ "estadoCuentaId": 2 }` | 200; estado Inactiva; evento "Cuenta inactivada." en auditoría |
| 16 | Repetir el caso 15 | 200; sin cambios y **sin** nuevo evento de auditoría |
| 17 | `POST /api/cuentas/{numero}/depositos` con `{ "monto": 10 }` sobre la cuenta inactiva | 409; el saldo no cambia |
| 18 | `PATCH /api/cuentas/{numero}/estado` con `{ "estadoCuentaId": 1 }` | 200; estado Activa |
| 19 | `GET /api/auditoria?resultado=Rechazado` | 200; incluye los rechazos de los casos 6 y 17, sin montos ni números escritos por el usuario |
| 20 | `GET /api/catalogos/monedas` | 200; NIO (C$) y USD (US$) |

## Seguridad

- **NU1903 / CVE-2025-6965:** la versión 10.0.10 de EF Core incluía de forma transitiva
  `SQLitePCLRaw.lib.e_sqlite3` 2.1.11, con una vulnerabilidad de SQLite anterior a 3.50.2 (corrupción
  de memoria). Se resolvió actualizando EF Core a **10.0.14**, que usa SQLitePCLRaw 2.1.13 con una
  versión corregida de SQLite. No se desactivó ninguna advertencia.
  Verificación: `dotnet list package --vulnerable --include-transitive`.
- Las respuestas de error no exponen trazas ni detalles internos.
- Los registros de auditoría no almacenan montos ni datos escritos por el usuario.
- Swagger y la consulta de auditoría solo se exponen en `Development` o `Evaluacion`.

## Notas técnicas

- Los importes se manejan como `decimal` y se almacenan en SQLite como enteros de 64 bits (centavos),
  para evitar errores de representación y comparaciones como texto.
- Las fechas de operaciones y auditoría se guardan en UTC; la interfaz las muestra en hora de Managua.
- El saldo, la transacción y el evento de auditoría de una operación exitosa se guardan en una sola
  transacción de base de datos. Los rechazos se auditan en una escritura independiente.
- Concurrencia optimista con `Saldo` y `EstadoCuentaId` como tokens: un conflicto se revierte, se
  registra en el log y se responde 409, sin reintentos.
