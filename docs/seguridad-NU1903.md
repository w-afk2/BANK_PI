# Advertencia NU1903: SQLite nativo vulnerable (CVE-2025-6965)

## Diagnóstico

| Campo | Valor |
|---|---|
| Advertencia | NU1903: paquete con vulnerabilidad de gravedad alta |
| Paquete afectado | `SQLitePCLRaw.lib.e_sqlite3` **2.1.11** (dependencia transitiva) |
| Origen | `Microsoft.EntityFrameworkCore.Sqlite` 10.0.10 → `SQLitePCLRaw.bundle_e_sqlite3` 2.1.11 |
| Aviso | GHSA-2m69-gcr7-jv3q / CVE-2025-6965 |
| Severidad | Alta (CVSS 4.0: 7.2; CVSS 3.1: 9.8) |
| Versiones afectadas | `SQLitePCLRaw.lib.e_sqlite3` 2.0.0 a 2.1.11 |
| Descripción | En SQLite anterior a 3.50.2, el número de términos de agregación puede exceder el de columnas disponibles y provocar corrupción de memoria. |

## Corrección aplicada

Se actualizó EF Core de **10.0.10 a 10.0.14** (versión de parche estable de la misma línea 10.0):

- `Microsoft.EntityFrameworkCore.Sqlite` 10.0.14 depende de `SQLitePCLRaw.bundle_e_sqlite3` **2.1.13**.
- La línea 2.1.12+ de SQLitePCLRaw referencia la biblioteca nativa SQLite 3.53.x, posterior a 3.50.2.
- `Microsoft.EntityFrameworkCore.Design` y la herramienta `dotnet-ef` se alinearon a 10.0.14.

Se descartó forzar `SQLitePCLRaw` 3.x: es un cambio de versión mayor no requerido por EF Core 10.

No se desactivó ni se ocultó ninguna advertencia de seguridad.

## Compatibilidad

- Es una actualización de parche: no cambia el modelo ni las APIs usadas.
- No se requiere una nueva migración (se comprueba con `dotnet ef migrations has-pending-model-changes`).

## Verificación

```powershell
dotnet build
dotnet list package --vulnerable --include-transitive
dotnet ef migrations has-pending-model-changes --project src/Banking.Infrastructure --startup-project src/Banking.Api
```

Resultado esperado: compilación sin NU1903, ningún paquete vulnerable y ningún cambio de modelo pendiente.

## Referencias

- https://github.com/advisories/GHSA-2m69-gcr7-jv3q
- https://osv.dev/vulnerability/GHSA-2m69-gcr7-jv3q
- https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.Sqlite
- https://github.com/ericsink/SQLitePCL.raw/releases/tag/v2.1.12
