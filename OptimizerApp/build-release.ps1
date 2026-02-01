# PC Performance Optimizer - Build Release
# Ejecutar: powershell -ExecutionPolicy Bypass -File build-release.ps1

param([string]$Version = "1.0.0")

$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "=========================================="
Write-Host "PC Performance Optimizer - Release Build"
Write-Host "=========================================="
Write-Host ""

# Step 1: Clean (opcional)
Write-Host "[1/3] Limpiando..." -ForegroundColor Cyan

# Step 2: Publish
Write-Host "[2/3] Publicando aplicacion..." -ForegroundColor Cyan
Push-Location .\OptimizerApp
dotnet publish -c Release -r win-x64 --self-contained --nologo 2>&1 | Out-Null
if ($LASTEXITCODE -ne 0) {
    Write-Host "  ERROR: Publish fallido" -ForegroundColor Red
    Pop-Location
    exit 1
}
Pop-Location

Write-Host "  OK: Publicado exitosamente" -ForegroundColor Green

# Step 3: Create installer
Write-Host "[3/3] Creando paquete instalador..." -ForegroundColor Cyan

$publishDir = ".\OptimizerApp\bin\Release\net10.0-windows\win-x64\publish"
$releaseDir = "..\Release"
$installerDir = Join-Path $releaseDir "OptimizerApp-v$Version"
$zipName = "OptimizerApp-v$Version-Installer.zip"

# Clean old
if (Test-Path $releaseDir) { 
    Remove-Item $releaseDir -Recurse -Force | Out-Null
}

New-Item -ItemType Directory -Path $installerDir -Force | Out-Null

# Copy files
Copy-Item "$publishDir\*" $installerDir -Recurse -Force | Out-Null

# Install.bat
@"
@echo off
setlocal enabledelayedexpansion
title PC Performance Optimizer - Setup
color 0A

cls
echo.
echo ====================================
echo  PC Performance Optimizer Setup
echo ====================================
echo.

net session >nul 2>&1
if %errorLevel% neq 0 (
    echo ERROR: Ejecute como administrador!
    pause
    exit /b 1
)

set "INSTALL_PATH=%ProgramFiles%\PC Performance Optimizer"

if exist "!INSTALL_PATH!" (
    rmdir /s /q "!INSTALL_PATH!"
)

mkdir "!INSTALL_PATH!"
echo Instalando archivos...
xcopy /E /I /Y "%~dp0*.*" "!INSTALL_PATH!" >nul

echo Creando accesos directos...
powershell -NoProfile -Command "`$WshShell = New-Object -ComObject WScript.Shell; `$shortcut = `$WshShell.CreateShortCut([Environment]::GetFolderPath('StartMenu') + '\PC Performance Optimizer.lnk'); `$shortcut.TargetPath = '!INSTALL_PATH!\OptimizerApp.exe'; `$shortcut.Save()" 2>nul
powershell -NoProfile -Command "`$WshShell = New-Object -ComObject WScript.Shell; `$shortcut = `$WshShell.CreateShortCut([Environment]::GetFolderPath('Desktop') + '\PC Performance Optimizer.lnk'); `$shortcut.TargetPath = '!INSTALL_PATH!\OptimizerApp.exe'; `$shortcut.Save()" 2>nul

reg add "HKLM\Software\Microsoft\Windows\CurrentVersion\Uninstall\PCPerformanceOptimizer" /v "DisplayName" /t REG_SZ /d "PC Performance Optimizer" /f >nul
reg add "HKLM\Software\Microsoft\Windows\CurrentVersion\Uninstall\PCPerformanceOptimizer" /v "DisplayVersion" /t REG_SZ /d "1.0.0" /f >nul
reg add "HKLM\Software\Microsoft\Windows\CurrentVersion\Uninstall\PCPerformanceOptimizer" /v "InstallLocation" /t REG_SZ /d "!INSTALL_PATH!" /f >nul

cls
echo.
echo ====================================
echo Instalacion Completada!
echo ====================================
echo.
echo Se ha instalado en: !INSTALL_PATH!
echo Presione una tecla para salir...
pause
"@ | Out-File (Join-Path $installerDir "Install.bat") -Encoding ASCII

# Uninstall.bat
@"
@echo off
setlocal enabledelayedexpansion
title PC Performance Optimizer - Desinstalar

net session >nul 2>&1
if %errorLevel% neq 0 (
    echo ERROR: Ejecute como administrador!
    pause
    exit /b 1
)

echo Desea desinstalar PC Performance Optimizer?
set /p choice="[S/N]: "
if /i not "%choice%"=="S" exit /b 0

set "INSTALL_PATH=%ProgramFiles%\PC Performance Optimizer"
del /f /q "%APPDATA%\Microsoft\Windows\Start Menu\Programs\PC Performance Optimizer.lnk" 2>nul
del /f /q "%USERPROFILE%\Desktop\PC Performance Optimizer.lnk" 2>nul
if exist "!INSTALL_PATH!" rmdir /s /q "!INSTALL_PATH!"
reg delete "HKLM\Software\Microsoft\Windows\CurrentVersion\Uninstall\PCPerformanceOptimizer" /f 2>nul

echo Desinstalacion completada.
pause
"@ | Out-File (Join-Path $installerDir "Uninstall.bat") -Encoding ASCII

# README
@"
PC Performance Optimizer - By KYZ
===================================

Version: $Version

INSTALACION:
1. Extraiga este ZIP en cualquier carpeta
2. Ejecute Install.bat como administrador
3. Siga las instrucciones

DESINSTALACION:
Ejecute Uninstall.bat como administrador

REQUISITOS:
- Windows 10 o superior (64 bits)
- 500 MB espacio disponible

CARACTERISTICAS:
- Diagnostico del sistema (CPU, RAM)
- Limpieza de memoria
- Limpieza de archivos temporales
- Monitoreo de procesos

Contacte al desarrollador para soporte.
"@ | Out-File (Join-Path $installerDir "README.txt") -Encoding UTF8

# Create ZIP
$zipPath = Join-Path ".." $zipName
if (Test-Path $zipPath) { Remove-Item $zipPath -Force }
Compress-Archive -Path $installerDir -DestinationPath $zipPath -CompressionLevel Optimal

$zipSize = (Get-Item $zipPath).Length / 1MB

Write-Host "  OK: Paquete creado" -ForegroundColor Green
Write-Host ""
Write-Host "=========================================="
Write-Host "LISTO!" -ForegroundColor Green
Write-Host "=========================================="
Write-Host ""
Write-Host "Archivo: $zipName"
Write-Host "Tamaño: $([math]::Round($zipSize, 2)) MB"
Write-Host "Ubicacion: $releaseDir"
Write-Host ""
Write-Host "Pasos para distribuir:"
Write-Host "1. Compartir el ZIP con usuarios"
Write-Host "2. Usuarios extraen el ZIP"
Write-Host "3. Usuarios ejecutan Install.bat como admin"
Write-Host ""
