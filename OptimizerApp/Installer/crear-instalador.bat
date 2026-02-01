@echo off
REM Script para compilar el instalador Inno Setup
REM Descarga e instala Inno Setup si no está disponible, luego compila el instalador

echo.
echo ======================================
echo   PC Performance Optimizer
echo   Generador de Instalador EXE
echo ======================================
echo.

REM Verificar si Inno Setup está instalado
if exist "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" (
    echo [OK] Inno Setup encontrado
    goto compile
)

echo [!] Inno Setup no está instalado
echo.
echo Descargando Inno Setup...
echo.

REM Descargar Inno Setup
powershell -NoProfile -Command ^
    "$url = 'https://files.jrsoftware.org/is/6/innosetup-6.2.2.exe';" ^
    "$outFile = '$env:TEMP\InnoSetup.exe';" ^
    "Write-Host 'Descargando desde: $url';" ^
    "try {" ^
    "    Invoke-WebRequest -Uri $url -OutFile $outFile -UseBasicParsing;" ^
    "    Write-Host 'Descarga completada: $outFile';" ^
    "    exit 0" ^
    "} catch {" ^
    "    Write-Host 'Error al descargar' -ForegroundColor Red;" ^
    "    exit 1" ^
    "}"

if %errorlevel% neq 0 (
    echo.
    echo ERROR: No se pudo descargar Inno Setup
    echo.
    echo Por favor descarga e instala manualmente desde:
    echo   https://jrsoftware.org/isdl.php
    echo.
    pause
    exit /b 1
)

echo.
echo Instalando Inno Setup...
echo.

start /wait "%TEMP%\InnoSetup.exe" /VERYSILENT /NORESTART

if not exist "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" (
    echo.
    echo ERROR: Instalación de Inno Setup falló
    echo.
    pause
    exit /b 1
)

echo [OK] Inno Setup instalado correctamente
echo.

:compile
echo Compilando instalador...
echo.

powershell -NoProfile -ExecutionPolicy Bypass -Command ^
    "cd '%~dp0'; .\build-innosetup.ps1"

if %errorlevel% neq 0 (
    echo.
    echo ERROR: Compilación fallida
    echo.
    pause
    exit /b 1
)

echo.
echo ======================================
echo   Instalador compilado exitosamente
echo ======================================
echo.
echo Ubicación:
echo   %~dp0bin\Release\OptimizerApp-v1.0.0-Setup.exe
echo.
echo El archivo está listo para distribuir.
echo.
pause
