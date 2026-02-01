@echo off
echo ================================================
echo  Desbloqueando OptimizerApp MSI
echo ================================================
echo.
echo Este script desbloqueara el instalador para
echo permitir su ejecucion.
echo.
pause

PowerShell -ExecutionPolicy Bypass -Command "Unblock-File -Path '%~dp0OptimizerApp-v1.0.0.msi'; Write-Host 'Archivo desbloqueado correctamente' -ForegroundColor Green"

echo.
echo ================================================
echo  MSI Desbloqueado
echo ================================================
echo.
echo Ahora ejecuta OptimizerApp-v1.0.0.msi
echo.
pause
