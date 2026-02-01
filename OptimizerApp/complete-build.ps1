# Script para compilar y crear el instalador completo de PC Performance Optimizer
# Uso: powershell -ExecutionPolicy Bypass -File complete-build.ps1

param(
    [string]$Configuration = "Release",
    [string]$Version = "1.0.0"
)

$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "=========================================="
Write-Host "PC Performance Optimizer - Complete Build"
Write-Host "=========================================="
Write-Host ""

# Step 1: Clean previous builds
Write-Host "[1/4] Cleaning previous builds..." -ForegroundColor Cyan
try {
    dotnet clean --configuration $Configuration --nologo -v q *> $null
    Write-Host "  ✓ Clean completed" -ForegroundColor Green
}
catch {
    Write-Host "  ⚠ Clean skipped" -ForegroundColor Yellow
}
finally {}

# Step 2: Publish Release build
Write-Host "[2/4] Publishing application..." -ForegroundColor Cyan
try {
    $publishOutput = dotnet publish -c $Configuration -r win-x64 --self-contained --nologo 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  ✓ Publish completed successfully" -ForegroundColor Green
        $publishDir = ".\bin\$Configuration\net10.0-windows\win-x64\publish"
    }
    else {
        throw "Publish failed"
    }
}
catch {
    Write-Host "  ✗ Publish failed" -ForegroundColor Red
    Write-Host $publishOutput
    exit 1
}

# Step 3: Verify publish output
Write-Host "[3/4] Verifying build artifacts..." -ForegroundColor Cyan
if (-not (Test-Path $publishDir)) {
    Write-Host "  ✗ Publish directory not found at $publishDir" -ForegroundColor Red
    exit 1
}

$exePath = Join-Path $publishDir "OptimizerApp.exe"
if (-not (Test-Path $exePath)) {
    Write-Host "  ✗ Executable not found" -ForegroundColor Red
    exit 1
}

$exeSize = (Get-Item $exePath).Length / 1MB
Write-Host "  ✓ Build artifacts verified" -ForegroundColor Green
Write-Host "    - Executable size: $([math]::Round($exeSize, 2)) MB"

# Step 4: Create installer package
Write-Host "[4/4] Creating installer package..." -ForegroundColor Cyan

$sourceDir = Resolve-Path $publishDir
$outputDir = ".\Release"
$installerDir = Join-Path $outputDir "OptimizerApp-v$Version"
$zipName = "OptimizerApp-v$Version-Installer.zip"

# Clean previous installer
if (Test-Path $outputDir) {
    Write-Host "  Cleaning previous package..."
    Remove-Item $outputDir -Recurse -Force
}

# Create directories
New-Item -ItemType Directory -Path $installerDir | Out-Null

# Copy application files
Write-Host "  Copying application files..."
Copy-Item "$sourceDir\*" $installerDir -Recurse -Force

# Get total size
$totalSize = (Get-ChildItem $installerDir -Recurse | Measure-Object -Property Length -Sum).Sum / 1MB

# Create Install.bat
$installScript = @'
@echo off
REM PC Performance Optimizer - Setup Script
REM Ejecutar como administrador

setlocal enabledelayedexpansion
title PC Performance Optimizer - Setup
color 0A

cls
echo.
echo ====================================
echo  PC Performance Optimizer Setup
echo ====================================
echo.

REM Check for admin rights
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo ERROR: Este script requiere derechos de administrador!
    echo Por favor, ejecute como administrador.
    pause
    exit /b 1
)

set "INSTALL_PATH=%ProgramFiles%\PC Performance Optimizer"

echo Instalando en: !INSTALL_PATH!
echo.

if exist "!INSTALL_PATH!" (
    echo Removiendo version anterior...
    rmdir /s /q "!INSTALL_PATH!"
)

echo Creando directorio...
mkdir "!INSTALL_PATH!"

echo Copiando archivos... (esto puede tomar unos momentos)
xcopy /E /I /Y "%~dp0*.*" "!INSTALL_PATH!" >nul

echo Creando accesos directos...

REM Start Menu shortcut
powershell -NoProfile -Command ^
    "$WshShell = New-Object -ComObject WScript.Shell;" ^
    "$MenuPath = [Environment]::GetFolderPath('StartMenu');" ^
    "$shortcut = $WshShell.CreateShortCut((Join-Path $MenuPath 'PC Performance Optimizer.lnk'));" ^
    "$shortcut.TargetPath = '!INSTALL_PATH!\OptimizerApp.exe';" ^
    "$shortcut.WorkingDirectory = '!INSTALL_PATH!';" ^
    "$shortcut.Save()" 2>nul

REM Desktop shortcut  
powershell -NoProfile -Command ^
    "$WshShell = New-Object -ComObject WScript.Shell;" ^
    "$DesktopPath = [Environment]::GetFolderPath('Desktop');" ^
    "$shortcut = $WshShell.CreateShortCut((Join-Path $DesktopPath 'PC Performance Optimizer.lnk'));" ^
    "$shortcut.TargetPath = '!INSTALL_PATH!\OptimizerApp.exe';" ^
    "$shortcut.WorkingDirectory = '!INSTALL_PATH!';" ^
    "$shortcut.Save()" 2>nul

REM Add to Add/Remove Programs
reg add "HKLM\Software\Microsoft\Windows\CurrentVersion\Uninstall\PCPerformanceOptimizer" ^
    /v "DisplayName" /t REG_SZ /d "PC Performance Optimizer" /f >nul
reg add "HKLM\Software\Microsoft\Windows\CurrentVersion\Uninstall\PCPerformanceOptimizer" ^
    /v "DisplayVersion" /t REG_SZ /d "1.0.0" /f >nul
reg add "HKLM\Software\Microsoft\Windows\CurrentVersion\Uninstall\PCPerformanceOptimizer" ^
    /v "InstallLocation" /t REG_SZ /d "!INSTALL_PATH!" /f >nul
reg add "HKLM\Software\Microsoft\Windows\CurrentVersion\Uninstall\PCPerformanceOptimizer" ^
    /v "UninstallString" /t REG_SZ /d "!INSTALL_PATH!\Uninstall.bat" /f >nul

cls
echo.
echo ====================================
echo  Instalacion Completada!
echo ====================================
echo.
echo Ubicacion: !INSTALL_PATH!
echo.
echo Accesos directos creados en:
echo - Menu Inicio
echo - Escritorio
echo.
echo Ahora puede ejecutar PC Performance Optimizer.
echo.
echo Presione una tecla para salir...
echo pause
'@

$installScript | Out-File (Join-Path $installerDir "Install.bat") -Encoding ASCII -Force

# Create Uninstall.bat
$uninstallScript = @'
@echo off
REM PC Performance Optimizer - Uninstall Script

setlocal enabledelayedexpansion
title PC Performance Optimizer - Uninstall
color 0A

cls
echo.
echo ====================================
echo  PC Performance Optimizer Uninstall
echo ====================================
echo.

net session >nul 2>&1
if %errorLevel% neq 0 (
    echo ERROR: Requiere derechos de administrador!
    pause
    exit /b 1
)

set "INSTALL_PATH=%ProgramFiles%\PC Performance Optimizer"

echo Desea desinstalar PC Performance Optimizer?
echo.
echo [S] Si, desinstalar
echo [N] No, cancelar
echo.
set /p choice="Opcion: "
if /i not "%choice%"=="S" (
    echo Desinstalacion cancelada.
    pause
    exit /b 0
)

echo.
echo Desinstalando...

REM Remove shortcuts
del /f /q "%APPDATA%\Microsoft\Windows\Start Menu\Programs\PC Performance Optimizer.lnk" 2>nul
del /f /q "%USERPROFILE%\Desktop\PC Performance Optimizer.lnk" 2>nul

REM Remove installation directory
if exist "!INSTALL_PATH!" (
    rmdir /s /q "!INSTALL_PATH!"
)

REM Remove registry entry
reg delete "HKLM\Software\Microsoft\Windows\CurrentVersion\Uninstall\PCPerformanceOptimizer" /f 2>nul

echo.
echo Desinstalacion completada.
echo Presione una tecla para salir...
echo pause
'@

$uninstallScript | Out-File (Join-Path $installerDir "Uninstall.bat") -Encoding ASCII -Force

# Create README.txt
$readme = @"
PC Performance Optimizer - By KYZ
===================================

Versi├│n: $Version

INSTALACION (Windows 10/11):
============================

1. Extraiga el archivo OptimizerApp-v$Version-Installer.zip
2. Abra la carpeta extraida
3. Ejecute "Install.bat" como ADMINISTRADOR
4. Siga las instrucciones

La aplicaci├│n se instalar├í en:
  C:\Program Files\PC Performance Optimizer

Se crear├ín accesos directos en:
  - Men├║ Inicio
  - Escritorio


DESINSTALACION:
================

Opci├│n 1 - Directa:
  1. Abra la carpeta de instalaci├│n
  2. Ejecute "Uninstall.bat" como administrador

Opci├│n 2 - A trav├⌐s de Windows:
  1. Panel de Control > Programas > Programas y caracter├┐sticas
  2. Busque "PC Performance Optimizer"
  3. Haga clic en "Desinstalar"


REQUISITOS:
===========
- Windows 10 o superior (64 bits)
- 500 MB de espacio disponible
- Conexi├│n a Internet (opcional)
- Derechos de administrador para instalar


CARACTERISTICAS:
=================
✓ Diagn├│stico de PC
  - Uso de CPU, RAM, Discos
  - Temperatura (si est├í disponible)
  - Procesos en tiempo real

✓ Monitoreo de Procesos
  - Lista completa de procesos activos
  - Uso de CPU y memoria por proceso
  - Opci├│n para cerrar procesos seleccionados

✓ Limpieza de Memoria
  - Monitoreo en tiempo real
  - Limpiar procesos con mayor consumo de RAM
  - Limpiar archivos temporales
  - Estadisticas de espacio liberado

✓ Optimizaciones de Chrome
  - Detecta procesos de Chrome
  - Opci├│n de limpiar procesos de Chrome


NOTAS IMPORTANTES:
===================
- Ejecute la aplicaci├│n como administrador para acceso completo
- Hacer clic en "Cerrar proceso" puede afectar aplicaciones activas
- Se recomienda guardar el trabajo antes de ejecutar limpiezas
- Los cambios de memoria se aplican inmediatamente

SOPORTE Y CONTACTO:
====================
Para reportar problemas o sugerencias, contacte al desarrollador.

By KYZ - 2024
Licencia: Libre para uso personal

"@

$readme | Out-File (Join-Path $installerDir "README.txt") -Encoding UTF8 -Force

# Create the ZIP file
Write-Host "  Creating ZIP package..."
if (Test-Path $zipName) {
    Remove-Item $zipName -Force
}

Compress-Archive -Path $installerDir -DestinationPath $zipName -CompressionLevel Optimal

$zipSize = (Get-Item $zipName).Length / 1MB

Write-Host "  ✓ Installer package created" -ForegroundColor Green
Write-Host "    - Package size: $([math]::Round($zipSize, 2)) MB"

# Summary
Write-Host ""
Write-Host "=========================================="
Write-Host "✓ Build Complete!" -ForegroundColor Green
Write-Host "=========================================="
Write-Host ""
Write-Host "Generated files:" -ForegroundColor Yellow
Write-Host "  📦 $zipName"
Write-Host "     Size: $([math]::Round($zipSize, 2)) MB"
Write-Host ""
Write-Host "Distribution:" -ForegroundColor Yellow
Write-Host "  1. Share the ZIP file with users"
Write-Host "  2. Users extract it to any folder"
Write-Host "  3. Users run Install.bat as administrator"
Write-Host "  4. Application will be installed in Program Files"
Write-Host ""
Write-Host "Ready for distribution!" -ForegroundColor Green
Write-Host ""
