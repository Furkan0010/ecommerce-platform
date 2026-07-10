<#
.SYNOPSIS
    start.ps1 ile ayaga kaldirilan her seyi durdurur.

.DESCRIPTION
    Varsayilan davranis DURDURMAKTIR (docker compose stop): konteynerler diskte
    kalir, "docker compose start" ile aninda geri gelirler.
    Silmek istiyorsaniz -Down, veriyi de silmek istiyorsaniz -Purge kullanin.

.PARAMETER KeepInfra
    Konteynerlere hic dokunmaz, sadece .NET servislerini durdurur.

.PARAMETER Down
    Konteynerleri durdurur VE siler (veri hacimleri korunur).

.PARAMETER Purge
    Konteynerleri VE veri hacimlerini siler (sepet verisi ve rabbitmq kuyruklari gider).

.EXAMPLE
    .\stop.ps1              # servisler + konteynerler durur
    .\stop.ps1 -KeepInfra   # sadece servisler durur
    .\stop.ps1 -Down        # konteynerler silinir de
#>
[CmdletBinding()]
param(
    [switch]$KeepInfra,
    [switch]$Down,
    [switch]$Purge
)

$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot

$names = 'Identity.Api','Catalog.Api','Basket.Api','Ordering.Api','Payment.Api','Gateway.Api'

Write-Host "[1/2] .NET servisleri durduruluyor..." -ForegroundColor Cyan
$procs = Get-Process -Name $names -ErrorAction SilentlyContinue
if ($procs) {
    foreach ($p in $procs) {
        Write-Host "  -> $($p.ProcessName) (PID $($p.Id))"
        Stop-Process -Id $p.Id -Force
    }
} else {
    Write-Host "  (calisan servis yok)" -ForegroundColor DarkGray
}

# "dotnet run" sarmalayicisi cocuk sureci baslatir. Sarmalayici olunce cocuk
# hayatta kalip portu tutmaya devam edebilir. dotnet.exe'nin kendi yolu
# Program Files'ta oldugu icin Path'e degil, komut satirina bakmak gerekir.
$repo = $PSScriptRoot
$orphans = Get-CimInstance Win32_Process -Filter "Name = 'dotnet.exe'" -ErrorAction SilentlyContinue |
           Where-Object { $_.CommandLine -and $_.CommandLine -like "*$repo*" }
foreach ($o in $orphans) {
    Write-Host "  -> dotnet run sarmalayicisi (PID $($o.ProcessId))"
    Stop-Process -Id $o.ProcessId -Force -ErrorAction SilentlyContinue
}

Write-Host "[2/2] Docker altyapisi..." -ForegroundColor Cyan
if ($KeepInfra) {
    Write-Host "  (konteynerler calisir birakildi -KeepInfra)" -ForegroundColor DarkGray
} elseif ($Purge) {
    docker compose down -v          # konteyner + veri hacimleri silinir
} elseif ($Down) {
    docker compose down             # konteyner silinir, veri hacimleri kalir
} else {
    docker compose stop             # VARSAYILAN: sadece durdurur, konteyner diskte kalir
}

Write-Host "`nDurduruldu." -ForegroundColor Green
