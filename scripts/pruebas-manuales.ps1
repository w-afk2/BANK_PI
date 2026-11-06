<#
  Pruebas manuales de la Etapa 2 contra la API en ejecucion.
  Uso (con la API corriendo, por ejemplo con F5):
    powershell -ExecutionPolicy Bypass -File scripts\pruebas-manuales.ps1
  Opcional: -Base "https://localhost:7050"
#>
param([string]$Base = "https://localhost:7050")

$ErrorActionPreference = "Stop"
$resultados = New-Object System.Collections.Generic.List[object]

function Llamar([string]$Metodo, [string]$Ruta, $Cuerpo = $null) {
    $parametros = @{
        Uri             = "$Base$Ruta"
        Method          = $Metodo
        UseBasicParsing = $true
        ContentType     = "application/json; charset=utf-8"
    }
    if ($null -ne $Cuerpo) {
        $parametros.Body = [System.Text.Encoding]::UTF8.GetBytes(($Cuerpo | ConvertTo-Json -Compress))
    }
    try {
        $respuesta = Invoke-WebRequest @parametros
        $json = $null
        if ($respuesta.Content) { $json = $respuesta.Content | ConvertFrom-Json }
        return [pscustomobject]@{ Codigo = [int]$respuesta.StatusCode; Json = $json }
    }
    catch {
        $http = $_.Exception.Response
        if ($null -eq $http) { throw }
        $json = $null
        if ($_.ErrorDetails -and $_.ErrorDetails.Message) {
            try { $json = $_.ErrorDetails.Message | ConvertFrom-Json } catch { }
        }
        return [pscustomobject]@{ Codigo = [int]$http.StatusCode; Json = $json }
    }
}

function Registrar([string]$Numero, [string]$Prueba, [int]$Esperado, $Respuesta, [bool]$CondicionExtra, [string]$Detalle) {
    $ok = ($Respuesta.Codigo -eq $Esperado) -and $CondicionExtra
    if (-not $Detalle -and $Respuesta.Json -and $Respuesta.Json.detail) { $Detalle = $Respuesta.Json.detail }
    $resultados.Add([pscustomobject]@{
        '#'       = $Numero
        Prueba    = $Prueba
        Esperado  = $Esperado
        Obtenido  = $Respuesta.Codigo
        Resultado = $(if ($ok) { "OK" } else { "FALLO" })
        Detalle   = $Detalle
    })
}

$hoy = (Get-Date).ToString("yyyyMMdd")

# 1. Registrar cliente
$r1 = Llamar POST "/api/clientes" @{
    nombres = "Ana Maria"; primerApellido = "Lopez"; segundoApellido = "Perez"
    fechaNacimiento = "1995-04-12"; genero = "Femenino"; ingresosMensuales = 25000.00
}
$clienteId = $r1.Json.id
Registrar "1" "Registrar cliente" 201 $r1 ($r1.Json.nombreCompleto -eq "Ana Maria Lopez Perez") "id=$clienteId; nombreCompleto=$($r1.Json.nombreCompleto)"

# 2. Consultar perfil
$r2 = Llamar GET "/api/clientes/$clienteId"
Registrar "2" "Consultar perfil" 200 $r2 (@($r2.Json.cuentas).Count -eq 0) "cuentas=$(@($r2.Json.cuentas).Count)"

# 3. Crear cuenta con saldo inicial 1000 (genera deposito inicial)
$r3 = Llamar POST "/api/cuentas" @{ clienteId = $clienteId; tipoCuentaId = 1; monedaId = 1; saldoInicial = 1000.00 }
$numero = $r3.Json.numeroCuenta
$formatoOk = ($numero -match '^ACC-\d{8}-\d{4}$') -and ($numero.Length -eq 17) -and ($numero.StartsWith("ACC-$hoy-")) -and ($numero -notmatch '-0000$')
Registrar "3" "Crear cuenta (saldo inicial 1000)" 201 $r3 ($formatoOk -and [decimal]$r3.Json.saldo -eq 1000) "numero=$numero; saldo=$($r3.Json.saldo); estado=$($r3.Json.estado)"

# 3b. Segunda cuenta con saldo 0: consecutivo siguiente y sin transaccion
$r3b = Llamar POST "/api/cuentas" @{ clienteId = $clienteId; tipoCuentaId = 2; monedaId = 2; saldoInicial = 0 }
$numero2 = $r3b.Json.numeroCuenta
$consecutivoOk = $false
if ($numero -and $numero2) { $consecutivoOk = ([int]$numero2.Substring(13) -eq [int]$numero.Substring(13) + 1) }
$h3b = Llamar GET "/api/cuentas/$numero2/transacciones"
Registrar "3b" "Cuenta saldo 0: consecutivo +1, sin transaccion" 201 $r3b ($consecutivoOk -and @($h3b.Json.transacciones).Count -eq 0) "numero=$numero2; movimientos=$(@($h3b.Json.transacciones).Count)"

# 4. Deposito
$r4 = Llamar POST "/api/cuentas/$numero/depositos" @{ monto = 500.25 }
Registrar "4" "Deposito 500.25" 201 $r4 ([decimal]$r4.Json.saldoResultante -eq 1500.25) "saldoResultante=$($r4.Json.saldoResultante)"

# 5. Retiro con fondos
$r5 = Llamar POST "/api/cuentas/$numero/retiros" @{ monto = 200 }
Registrar "5" "Retiro 200" 201 $r5 ([decimal]$r5.Json.saldoResultante -eq 1300.25) "saldoResultante=$($r5.Json.saldoResultante)"

# 6. Retiro sin fondos
$r6 = Llamar POST "/api/cuentas/$numero/retiros" @{ monto = 999999 }
Registrar "6" "Retiro sin fondos (999999)" 409 $r6 $true ""

# 7. Monto con 3 decimales
$r7 = Llamar POST "/api/cuentas/$numero/depositos" @{ monto = 10.555 }
Registrar "7" "Deposito 10.555 (3 decimales)" 400 $r7 $true ""

# 8. Monto cero
$r8 = Llamar POST "/api/cuentas/$numero/depositos" @{ monto = 0 }
Registrar "8" "Deposito 0" 400 $r8 $true ""

# 9. Saldo sin cambios tras los rechazos 6, 7 y 8
$r9 = Llamar GET "/api/cuentas/$numero/saldo"
Registrar "9" "Saldo tras rechazos" 200 $r9 ([decimal]$r9.Json.saldo -eq 1300.25) "saldo=$($r9.Json.saldo)"

# 10. Historial cronologico: solo 3 movimientos exitosos
$r10 = Llamar GET "/api/cuentas/$numero/transacciones"
$movs = @($r10.Json.transacciones)
$fechasOrdenadas = $true
for ($i = 1; $i -lt $movs.Count; $i++) {
    if ([datetime]$movs[$i].fechaHora -lt [datetime]$movs[$i - 1].fechaHora) { $fechasOrdenadas = $false }
}
$historialOk = ($movs.Count -eq 3) -and $fechasOrdenadas `
    -and ($movs[0].tipo -like 'Dep*') -and ([decimal]$movs[0].saldoResultante -eq 1000) `
    -and ($movs[1].tipo -like 'Dep*') -and ([decimal]$movs[1].saldoResultante -eq 1500.25) `
    -and ($movs[2].tipo -eq 'Retiro') -and ([decimal]$movs[2].saldoResultante -eq 1300.25)
$resumen = ($movs | ForEach-Object { "$($_.tipo) $($_.monto) -> $($_.saldoResultante)" }) -join " | "
Registrar "10" "Historial cronologico" 200 $r10 $historialOk "$($movs.Count) movimientos: $resumen"

# 11. Cuenta inexistente
$r11 = Llamar GET "/api/cuentas/ACC-20990101-0001/saldo"
Registrar "11" "Saldo de cuenta inexistente" 404 $r11 $true ""

# 12. Cliente inexistente
$r12 = Llamar POST "/api/cuentas" @{ clienteId = 999999; tipoCuentaId = 1; monedaId = 1; saldoInicial = 100 }
Registrar "12" "Cuenta para cliente inexistente" 404 $r12 $true ""

# 13. Saldo inicial negativo
$r13 = Llamar POST "/api/cuentas" @{ clienteId = $clienteId; tipoCuentaId = 1; monedaId = 1; saldoInicial = -5 }
Registrar "13" "Saldo inicial negativo" 400 $r13 $true ""

$resultados | Format-Table -AutoSize -Wrap
$fallos = @($resultados | Where-Object { $_.Resultado -eq "FALLO" }).Count
Write-Host ""
Write-Host "Total: $($resultados.Count)  Correctas: $($resultados.Count - $fallos)  Fallidas: $fallos"
