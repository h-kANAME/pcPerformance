# Script para compilar el instalador Inno Setup
# Este script compila el archivo OptimizerApp.iss para generar un EXE instalador

param(
    [string]$Version = "1.0.0"
)

$ErrorActionPreference = "Stop"

Write-Host "======================================" -ForegroundColor Cyan
Write-Host "  Compilando Instalador Inno Setup" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""

# Paso 1: Verificar que Inno Setup está instalado
Write-Host "[1/3] Verificando Inno Setup..." -ForegroundColor Yellow
$isccPath = "C:\Program Files (x86)\Inno Setup 6\ISCC.exe"

if (-not (Test-Path $isccPath)) {
    Write-Host "ERROR: Inno Setup no encontrado" -ForegroundColor Red
    Write-Host ""
    Write-Host "Descarga e instala Inno Setup desde:" -ForegroundColor Yellow
    Write-Host "  https://jrsoftware.org/isdl.php" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Luego ejecuta de nuevo este script." -ForegroundColor Yellow
    exit 1
}
Write-Host "  OK - Inno Setup encontrado" -ForegroundColor Green

# Paso 2: Verificar que existe el script ISS
Write-Host "[2/3] Verificando archivo ISS..." -ForegroundColor Yellow
$issFile = "OptimizerApp.iss"

if (-not (Test-Path $issFile)) {
    Write-Host "ERROR: Archivo no encontrado: $issFile" -ForegroundColor Red
    exit 1
}
Write-Host "  OK - Archivo ISS encontrado" -ForegroundColor Green

# Paso 3: Compilar el instalador
Write-Host "[3/3] Compilando instalador..." -ForegroundColor Yellow
try {
    & $isccPath $issFile
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "======================================" -ForegroundColor Green
        Write-Host "  COMPILACION EXITOSA" -ForegroundColor Green
        Write-Host "======================================" -ForegroundColor Green
        Write-Host ""
        
        $exePath = ".\bin\Release\OptimizerApp-v$Version-Setup.exe"
        if (Test-Path $exePath) {
            $size = [math]::Round((Get-Item $exePath).Length / 1MB, 2)
            Write-Host "Instalador generado:" -ForegroundColor Cyan
            Write-Host "  $exePath" -ForegroundColor White
            Write-Host "  Tamaño: $size MB" -ForegroundColor White
            Write-Host ""
            Write-Host "El instalador está listo para distribuir." -ForegroundColor Green
        }
    } else {
        throw "Inno Setup reportó un error"
    }
} catch {
    Write-Host ""
    Write-Host "ERROR: Fallo compilando el instalador" -ForegroundColor Red
    Write-Host "  $_" -ForegroundColor Red
    exit 1
}
