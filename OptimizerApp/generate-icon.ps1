# Script PowerShell para generar AppIcon.ico
# Ejecutar desde: d:\Desarrollo\pcPerformance\OptimizerApp

Add-Type -AssemblyName System.Drawing

Write-Host "Generando AppIcon.ico..." -ForegroundColor Cyan

$AssetDir = "OptimizerApp\Assets"

# Crear directorio si no existe
if (-not (Test-Path $AssetDir)) {
    New-Item -ItemType Directory -Path $AssetDir | Out-Null
}

# Crear bitmap 256x256
$bitmap = New-Object System.Drawing.Bitmap(256, 256)
$graphics = [System.Drawing.Graphics]::FromImage($bitmap)

# Fondo negro
$graphics.Clear([System.Drawing.Color]::FromArgb(13, 13, 13))

# Borde verde
$greenColor = [System.Drawing.Color]::FromArgb(68, 214, 44)
$pen = New-Object System.Drawing.Pen($greenColor, 3)
$graphics.DrawRectangle($pen, 8, 8, 240, 240)
$pen.Dispose()

# Texto OP
$font = New-Object System.Drawing.Font("Arial", 96, [System.Drawing.FontStyle]::Bold)
$brush = New-Object System.Drawing.SolidBrush($greenColor)
$format = New-Object System.Drawing.StringFormat
$format.Alignment = [System.Drawing.StringAlignment]::Center
$format.LineAlignment = [System.Drawing.StringAlignment]::Center
$graphics.DrawString("OP", $font, $brush, 128, 110, $format)
$font.Dispose()
$brush.Dispose()

# Texto By KYZ
$font = New-Object System.Drawing.Font("Arial", 18, [System.Drawing.FontStyle]::Regular)
$whiteColor = [System.Drawing.Color]::FromArgb(232, 232, 232)
$brush = New-Object System.Drawing.SolidBrush($whiteColor)
$graphics.DrawString("By KYZ", $font, $brush, 128, 165, $format)
$font.Dispose()
$brush.Dispose()

$graphics.Dispose()

# Guardar como PNG
$pngPath = Join-Path $AssetDir "AppIcon.png"
$bitmap.Save($pngPath, [System.Drawing.Imaging.ImageFormat]::Png)
Write-Host "Saved PNG: $pngPath" -ForegroundColor Green

# Guardar como ICO
$icoPath = Join-Path $AssetDir "AppIcon.ico"
$ico = [System.Drawing.Icon]::FromHandle($bitmap.GetHicon())
$fs = [System.IO.File]::Create($icoPath)
$ico.Save($fs)
$fs.Close()
$fs.Dispose()

$bitmap.Dispose()

Write-Host "Saved ICO: $icoPath" -ForegroundColor Green
Write-Host "Icon generation completed successfully" -ForegroundColor Green
