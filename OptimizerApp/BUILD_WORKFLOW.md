# Flujo de Trabajo: Build Local vs Distribución

## 🔨 Para Desarrollo Local (Testing)

Después de hacer cambios en el código, compila localmente:

### Opción 1: Build Rápido (solo actualizar binarios)
```powershell
cd d:\Desarrollo\pcPerformance\OptimizerApp
.\build-release.ps1
```
**Resultado:** Binarios compilados en `OptimizerApp\bin\Release\net10.0-windows\win-x64\publish\`

### Opción 2: Build Completo (limpia + compila + genera instalador)
```powershell
cd d:\Desarrollo\pcPerformance\OptimizerApp
.\complete-build.ps1
```
**Resultado:** 
- Binarios compilados
- Instalador EXE generado en `Installer\bin\Release\OptimizerApp-v1.0.0-Setup.exe`

---

## 📦 Para Distribución a Usuarios Finales

Una vez que validaste localmente, genera el instalador final:

### Opción 1: Script PowerShell
```powershell
cd d:\Desarrollo\pcPerformance\OptimizerApp\Installer
.\build-innosetup.ps1
```

### Opción 2: Script BAT (más fácil)
```
d:\Desarrollo\pcPerformance\OptimizerApp\Installer\crear-instalador.bat
```

**Resultado:** 
- EXE instalador en `Installer\bin\Release\OptimizerApp-v1.0.0-Setup.exe`
- Listo para distribuir a usuarios

---

## 📋 Flujo Recomendado

1. **Hacer cambios en el código**
2. **Compilar localmente:** `build-release.ps1`
3. **Probar la aplicación**
4. **Si todo está OK:**
   - Actualizar versión en `OptimizerApp.iss` (opcional)
   - Ejecutar `crear-instalador.bat`
   - Distribuir el EXE generado

---

## Actualizar Versión

Para cambiar la versión del instalador:

Edita `OptimizerApp\Installer\OptimizerApp.iss`:
```ini
#define MyAppVersion "1.0.1"  ← Cambiar aquí
```

Luego recompila con `build-innosetup.ps1`
