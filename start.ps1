<#
.SYNOPSIS
    Tum platformu tek komutla ayaga kaldirir.

.DESCRIPTION
    1) Docker altyapisini baslatir (basket-redis, ordering-rabbitmq) ve saglikli
       olmalarini bekler. Imaj derlemez, sadece mevcut imajlari kullanir.
    2) Alti .NET servisini baslatir. Veritabanlari yerel SQL Server'da kalir.

.PARAMETER NoInfra
    Docker adimini atlar. Redis/RabbitMQ zaten calisiyorsa kullanin.

.PARAMETER Background
    Servisleri ayri pencerelerde degil, arka planda calistirir; loglar logs/ altina yazilir.

.EXAMPLE
    .\start.ps1
    .\start.ps1 -Background
    .\start.ps1 -NoInfra
#>
[CmdletBinding()]
param(
    [switch]$NoInfra,
    [switch]$Background
)

$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot

$services = [ordered]@{
    'Identity.Api' = @{ Path = 'ecommerce-identity/src/Identity.Api'; Port = 5001 }
    'Catalog.Api'  = @{ Path = 'ecommerce-catalog/src/Catalog.Api';   Port = 5002 }
    'Basket.Api'   = @{ Path = 'ecommerce-basket/src/Basket.Api';     Port = 5003 }
    'Ordering.Api' = @{ Path = 'ecommerce-ordering/src/Ordering.Api'; Port = 5004 }
    'Payment.Api'  = @{ Path = 'ecommerce-payment/src/Payment.Api';   Port = 5005 }
    'Gateway.Api'  = @{ Path = 'ecommerce-gateway/src/Gateway.Api';   Port = 8080 }
}

function Test-PortListening([int]$Port) {
    $null -ne (Get-NetTCPConnection -LocalPort $Port -State Listen -ErrorAction SilentlyContinue)
}

function Wait-ForPort([int]$Port, [int]$TimeoutSec = 90) {
    $sw = [Diagnostics.Stopwatch]::StartNew()
    while ($sw.Elapsed.TotalSeconds -lt $TimeoutSec) {
        if (Test-PortListening $Port) { return $true }
        Start-Sleep -Milliseconds 500
    }
    return $false
}

# --- Onceden calisan surecleri yakala -------------------------------------
# Eski bir surec 127.0.0.1'e bagli kalirsa, Docker 0.0.0.0'a baglansa bile
# "localhost" cagrilari o surece gider ve saatlerce yanlis seyi test edersiniz.
$stale = Get-Process -Name $services.Keys -ErrorAction SilentlyContinue
if ($stale) {
    Write-Host "Zaten calisan servisler bulundu:" -ForegroundColor Yellow
    $stale | ForEach-Object { Write-Host "  - $($_.ProcessName) (PID $($_.Id))" -ForegroundColor Yellow }
    Write-Host "Once .\stop.ps1 calistirin." -ForegroundColor Yellow
    exit 1
}

# --- 1) Altyapi ------------------------------------------------------------
if (-not $NoInfra) {
    Write-Host "[1/2] Docker altyapisi (redis + rabbitmq)..." -ForegroundColor Cyan
    docker compose up -d
    if ($LASTEXITCODE -ne 0) { throw "docker compose up basarisiz oldu." }

    Write-Host "      saglik kontrolu bekleniyor..." -NoNewline
    $sw = [Diagnostics.Stopwatch]::StartNew()
    while ($sw.Elapsed.TotalSeconds -lt 90) {
        $states = docker inspect --format '{{.State.Health.Status}}' basket-redis ordering-rabbitmq 2>$null
        if ($states -and ($states | Where-Object { $_ -ne 'healthy' }).Count -eq 0) { break }
        Start-Sleep -Seconds 2
        Write-Host "." -NoNewline
    }
    Write-Host " hazir." -ForegroundColor Green
} else {
    Write-Host "[1/2] Docker adimi atlandi (-NoInfra)." -ForegroundColor DarkGray
}

# --- 2) Servisler ----------------------------------------------------------
Write-Host "[2/2] Servisler baslatiliyor..." -ForegroundColor Cyan

if ($Background) {
    $logDir = Join-Path $PSScriptRoot 'logs'
    if (-not (Test-Path $logDir)) { New-Item -ItemType Directory -Path $logDir | Out-Null }
}

foreach ($name in $services.Keys) {
    $svc = $services[$name]
    # NOT: degisken adi $args OLAMAZ; PowerShell'in otomatik degiskeni.
    $dotnetArgs = "run --project `"$($svc.Path)`" --launch-profile http"

    if ($Background) {
        $log = Join-Path $logDir "$name.log"
        Start-Process -FilePath 'dotnet' -ArgumentList $dotnetArgs `
            -RedirectStandardOutput $log -RedirectStandardError "$log.err" `
            -WindowStyle Hidden | Out-Null
    } else {
        Start-Process -FilePath 'powershell' -ArgumentList @(
            '-NoExit', '-Command',
            "`$host.UI.RawUI.WindowTitle = '$name (:$($svc.Port))'; dotnet $dotnetArgs"
        ) | Out-Null
    }
    Write-Host "  -> $name (:$($svc.Port))"
}

# --- Hazir olmalarini bekle -------------------------------------------------
Write-Host "`nPortlar dinlemeye baslasin diye bekleniyor..." -ForegroundColor Cyan
$failed = @()
foreach ($name in $services.Keys) {
    $port = $services[$name].Port
    if (Wait-ForPort $port 120) {
        Write-Host ("  {0,-14} :{1}  hazir" -f $name, $port) -ForegroundColor Green
    } else {
        Write-Host ("  {0,-14} :{1}  ZAMAN ASIMI" -f $name, $port) -ForegroundColor Red
        $failed += $name
    }
}

Write-Host ""
if ($failed.Count -gt 0) {
    Write-Host "Baslamayan servis(ler): $($failed -join ', ')" -ForegroundColor Red
    if ($Background) { Write-Host "Loglara bakin: logs\<servis>.log" -ForegroundColor Red }
    exit 1
}

Write-Host "Platform hazir." -ForegroundColor Green
Write-Host "  Gateway     : http://localhost:8080"
Write-Host "  RabbitMQ UI : http://localhost:15672  (guest / guest)"
Write-Host "  Durdurmak   : .\stop.ps1"
