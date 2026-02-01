# INSTRUCCIONES: Instalar con Control Inteligente de Aplicaciones Activo

## El Problema
Windows bloquea el instalador porque no tiene un certificado comercial.
Mensaje: "El administrador del sistema estableció directivas para impedir esta instalación"

---

## ✅ MÉTODO 1: Instalar el Certificado (Recomendado)

### Pasos:
1. **Haz clic derecho** en `INSTALAR_CERTIFICADO.bat`
2. Selecciona **"Ejecutar como administrador"**
3. Haz clic en **"Sí"** cuando aparezca el UAC
4. Presiona **Enter** cuando lo solicite
5. Ahora instala `OptimizerApp-v1.0.0.msi` normalmente

**NOTA:** Esto agrega el certificado de confianza solo en este PC.

---

## ✅ MÉTODO 2: Desbloquear el Archivo

### Opción A - Script Automático:
1. **Haz doble clic** en `DESBLOQUEAR_MSI.bat`
2. Presiona **Enter** cuando lo solicite
3. Instala `OptimizerApp-v1.0.0.msi`

### Opción B - Manual:
1. **Clic derecho** en `OptimizerApp-v1.0.0.msi`
2. Selecciona **Propiedades**
3. En la pestaña **General**, abajo del todo:
   - Marca ☑ **Desbloquear**
4. Clic en **Aplicar** → **Aceptar**
5. Ahora haz doble clic en el MSI para instalar

---

## ✅ MÉTODO 3: Deshabilitar Control Inteligente Temporalmente

### Windows 11:
1. Abre **Configuración** (Win + I)
2. Ve a **Privacidad y seguridad**
3. Clic en **Seguridad de Windows**
4. Clic en **Control de aplicaciones y explorador**
5. En **Comprobar aplicaciones y archivos**, selecciona **Desactivado**
6. Instala el MSI
7. **IMPORTANTE:** Vuelve a activar la protección después

### Windows 10:
1. Abre **Configuración** (Win + I)
2. Ve a **Actualización y seguridad**
3. Clic en **Seguridad de Windows**
4. Continúa igual que Windows 11 desde el paso 4

---

## ✅ MÉTODO 4: Instalar desde PowerShell (Avanzado)

Abre **PowerShell como Administrador** y ejecuta:

```powershell
Start-Process msiexec.exe -ArgumentList "/i `"$PWD\OptimizerApp-v1.0.0.msi`"" -Verb RunAs
```

---

## ❓ ¿Por Qué Aparece Este Mensaje?

El instalador está firmado con un certificado auto-firmado (solo para pruebas).
Para distribución profesional sin advertencias, se necesita un certificado comercial (~$200-300 USD/año).

---

## ✅ Cualquiera de los 4 métodos permite instalar correctamente

**El instalador es seguro, solo necesita permiso para ejecutarse.**
