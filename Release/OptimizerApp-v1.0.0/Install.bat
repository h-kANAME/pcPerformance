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
powershell -NoProfile -Command "$WshShell = New-Object -ComObject WScript.Shell; $shortcut = $WshShell.CreateShortCut([Environment]::GetFolderPath('StartMenu') + '\PC Performance Optimizer.lnk'); $shortcut.TargetPath = '!INSTALL_PATH!\OptimizerApp.exe'; $shortcut.Save()" 2>nul
powershell -NoProfile -Command "$WshShell = New-Object -ComObject WScript.Shell; $shortcut = $WshShell.CreateShortCut([Environment]::GetFolderPath('Desktop') + '\PC Performance Optimizer.lnk'); $shortcut.TargetPath = '!INSTALL_PATH!\OptimizerApp.exe'; $shortcut.Save()" 2>nul

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
