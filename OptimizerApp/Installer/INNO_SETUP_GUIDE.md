# Crear Instalador EXE con Inno Setup

## Pasos Rápidos

### 1. Instalar Inno Setup

Descarga desde: https://jrsoftware.org/isdl.php

Elige la versión Unicode (recomendado): `innosetup-6.2.2.exe` o más reciente.

### 2. Compilar el Instalador

Ejecuta el script:

```powershell
cd d:\Desarrollo\pcPerformance\OptimizerApp\Installer
.\build-innosetup.ps1
```

O simplemente ejecuta el BAT:

```
crear-instalador.bat
```

### 3. El Instalador Estará Listo

Ubicación: `OptimizerApp-v1.0.0-Setup.exe` en la carpeta `bin\Release\`

## Qué Incluye

El instalador EXE contiene:
- ✅ Aplicación principal (OptimizerApp.exe)
- ✅ .NET 10.0 Runtime (auto-contenido)
- ✅ Dependencias y DLLs
- ✅ Paquetes de idioma (13 idiomas)
- ✅ Iconos y assets

## Distribución

Comparte el archivo `OptimizerApp-v1.0.0-Setup.exe` con los usuarios finales.
No requiere descargas adicionales ni dependencias externas.

## Desinstalación

Los usuarios pueden desinstalar desde:
- Control Panel > Programas > Desinstalar programas
- O ejecutar el desinstalador desde el menú Inicio

## Archivo de Configuración

`OptimizerApp.iss` - Script Inno Setup (no es necesario modificar para compilar)

Para cambiar versión:
```
#define MyAppVersion "1.0.1"
```

Luego recompila con `build-innosetup.ps1`
