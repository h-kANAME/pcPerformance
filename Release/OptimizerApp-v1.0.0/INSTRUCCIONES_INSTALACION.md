# PC Performance Optimizer - Guía de Instalación

## ¿Qué encontrarás en esta carpeta?

### 📦 OptimizerApp-v1.0.0.msi
**Este es el instalador principal** que debes usar para instalar PC Performance Optimizer en tu computadora.

### 🗑️ Uninstall.bat
Script de desinstalación alternativo si deseas desinstalar la aplicación desde esta carpeta.

---

## 📥 Instalación (Recomendado)

### Opción 1: Instalador MSI (Recomendado)

1. **Haz doble clic** en `OptimizerApp-v1.0.0.msi`
2. **Acepta** los permisos de administrador cuando Windows lo solicite
3. **Sigue** el asistente de instalación:
   - Acepta el acuerdo de licencia
   - Elige la ubicación de instalación (predeterminado: `C:\Program Files\PC Performance Optimizer`)
   - Haz clic en "Instalar"
4. **Espera** a que finalice la instalación
5. **Encuentra** el acceso directo en:
   - 🖥️ Escritorio: "PC Performance Optimizer"
   - 📋 Menú Inicio: "PC Performance Optimizer"

### Opción 2: Instalación desde archivos (Solo para usuarios avanzados)

Si prefieres una instalación manual sin usar el MSI:

1. **Ejecuta** `Install.bat` como administrador
2. Esto copiará los archivos y creará accesos directos manualmente

---

## 🗑️ Desinstalación

### Método 1: Panel de Control (Recomendado)

1. Abre **Panel de Control** → **Programas** → **Desinstalar un programa**
2. Busca **"PC Performance Optimizer"**
3. Haz clic derecho → **Desinstalar**
4. Sigue el asistente de desinstalación

### Método 2: Script de Desinstalación

1. **Haz doble clic** en `Uninstall.bat`
2. **Acepta** los permisos de administrador
3. Confirma la desinstalación
4. La aplicación se eliminará completamente

---

## ✅ Requisitos del Sistema

- **Sistema Operativo:** Windows 10 o Windows 11 (64 bits)
- **Espacio en Disco:** ~200 MB
- **RAM:** Mínimo 4 GB
- **Privilegios:** Permisos de administrador para instalar/desinstalar

---

## ❓ Preguntas Frecuentes

### ¿Por qué aparece "Windows protegió tu PC"?

Esta advertencia es normal para aplicaciones no firmadas digitalmente. Para continuar:
1. Haz clic en **"Más información"**
2. Haz clic en **"Ejecutar de todas formas"**

### ¿Dónde se instala la aplicación?

Por defecto en: `C:\Program Files\PC Performance Optimizer\`

### ¿Cómo actualizo a una versión nueva?

1. Desinstala la versión anterior usando el Panel de Control
2. Instala la nueva versión con el nuevo archivo MSI

### ¿Qué archivos NO debo distribuir?

Los archivos `.dll`, `.exe`, `.deps.json` y carpetas de idiomas son **parte de la aplicación**.
Solo comparte el archivo **OptimizerApp-v1.0.0.msi** con tus usuarios finales.

---

## 🛡️ Seguridad

- ✅ Sin malware, sin spyware
- ✅ No recopila datos personales
- ✅ No requiere conexión a Internet
- ✅ Código verificable en el repositorio fuente

---

## 📞 Soporte

Si tienes problemas con la instalación:
- Verifica que tengas permisos de administrador
- Cierra cualquier instancia de OptimizerApp.exe antes de instalar/desinstalar
- Desactiva temporalmente el antivirus si bloquea la instalación

---

**Desarrollado por:** KYZ  
**Versión:** 1.0.0  
**Fecha:** Enero 2026
