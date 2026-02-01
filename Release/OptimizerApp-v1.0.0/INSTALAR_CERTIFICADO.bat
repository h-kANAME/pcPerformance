@echo off
echo ================================================
echo  Instalando Certificado de Confianza
echo ================================================
echo.
echo Este script instalara el certificado para permitir
echo la instalacion de PC Performance Optimizer
echo.
pause

PowerShell -ExecutionPolicy Bypass -Command "$msi = '%~dp0OptimizerApp-v1.0.0.msi'; $cert = (Get-AuthenticodeSignature $msi).SignerCertificate; $store = New-Object System.Security.Cryptography.X509Certificates.X509Store('Root','LocalMachine'); $store.Open('ReadWrite'); $store.Add($cert); $store.Close(); Write-Host 'Certificado instalado correctamente' -ForegroundColor Green"

echo.
echo ================================================
echo  Certificado Instalado
echo ================================================
echo.
echo Ahora puedes instalar OptimizerApp-v1.0.0.msi
echo sin advertencias.
echo.
pause
