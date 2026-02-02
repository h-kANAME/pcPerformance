# ⚡ PC PERFORMANCE OPTIMIZER v1.2

> 🎮

Una herramienta diseñada para gamers y usuarios profesionales que exigen el máximo rendimiento de su máquina. Optimiza memoria, unidades de almacenamiento y recursos del sistema en **Windows 11** con una interfaz con modo oscuro que no te deja ciego/a.

---

## 🎯 ¿Qué hace?

**PC Performance Optimizer** te da control total sobre los recursos de tu sistema:

### 💾 **MEMORIA**

- Monitoreo en tiempo real del uso RAM
- Limpieza agresiva de memoria caché y buffers
- Detección de procesos que consumen recursos innecesarios
- Análisis de uso por aplicación

### 💿 **DISCO**

- **Limpiar Temporal**: Borra archivos innecesarios (TEMP, Prefetch, etc.)
- **TRIM SSD**: Optimiza el rendimiento de unidades SSD
- **Desfragmentación**: Organiza datos en HDDs tradicionales
- Detección automática SSD/HDD
- Reportes detallados de operaciones

### ⚙️ **SISTEMA**

- Información de hardware
- Monitoreo de procesos y CPU
- Gestión de aplicaciones de inicio

---

## 🚀 Inicio Rápido

### Instalación

1. Descarga **OptimizerApp-v1.2-Setup.exe** (43 MB)
2. Ejecuta el instalador
3. ✅ **Listo** - No requiere .NET instalado, todo está incluido

### Desde el Código Fuente

```bash
git clone <repository-url>
cd OptimizerApp
dotnet build -c Release
dotnet run
```

---

## 🔧 Requisitos de Desarrollo

Para contribuir o compilar desde código:

| Requisito     | Versión |
| ------------- | -------- |
| Windows       | 11       |
| .NET SDK      | 10.0+    |
| Visual Studio | 2022     |
| C# Language   | 12.0+    |

### Instalación de Dependencias

```bash
# Restaurar NuGet packages
dotnet restore

# Compilar
dotnet build -c Release

# Publicar
dotnet publish -c Release
```

---

## 📊 Características Clave

✨ **Interfaz fachera**

- Tema oscuro (negro + verde cibernético)
- UI moderna y responsiva
- Paneles en tiempo real

⚡ **Rendimiento**

- Operaciones asincrónicas no bloqueantes
- Caché inteligente para queries costosas
- Ejecución paralela de tareas

🛡️ **Seguro**

- Validaciones antes de cada operación
- Confirmaciones de acciones críticas
- Sin modificación de archivos del sistema operativo

📈 **Informativo**

- Reportes detallados por operación
- Gráficos de uso de recursos
- Historial de optimizaciones

---

## 💻 Arquitectura

Construido con **MVVM** en **.NET 10 WPF**:

```
OptimizerApp/
├── Models/           # Entidades y estructuras de datos
├── Services/         # Lógica de negocio (Memoria, Disco, Diagnósticos)
├── ViewModels/       # MVVM ViewModels
├── Views/            # Interfaces de usuario XAML
├── Converters/       # Conversores de datos para bindings
└── Assets/           # Iconos y recursos
```

---

## 🎮 Cómo Usar

### 1. **MEMORIA**

- Abre la pestaña "Memoria"
- Haz clic en "Analizar" para escanear
- Presiona "Limpiar" para liberar RAM

### 2. **DISCO**

- Ve a "Disco"
- Selecciona la unidad a optimizar
- Elige la operación:
  - **Limpiar Temp**: Elimina archivos temporales
  - **TRIM** (SSD): Optimiza rendimiento
  - **Desfragmentar** (HDD): Reorganiza datos

### 3. **SISTEMA**

- Visualiza diagnósticos en tiempo real
- Monitorea procesos activos
- Gestiona aplicaciones de inicio

---

## 📋 Cambios en v1.2

✨ **Nueva Pestaña DISCO**

- Optimización SSD con TRIM
- Desfragmentación de HDD
- Limpieza de archivos temporales
- Detección automática de tipo de unidad

🎨 **UI Mejorada**

- Tema completamente oscuro
- Layout reorganizado para mejor usabilidad
- Alineación visual perfecta

⚡ **Rendimiento**

- Runtime de .NET 10 incluido (self-contained)
- Caché de tipos de disco
- Timeouts inteligentes en operaciones

---

## 🐛 Reportar Problemas

¿Encontraste un bug? ¿Tienes sugerencias?

1. Abre un **Issue** en GitHub
2. Describe el problema con detalles
3. Incluye versión de Windows y especificaciones

Me podes dejar un mensaje desde mi sitio: [https://kyz.com.ar/]()

---

## 📝 Licencia

Este proyecto está bajo licencia **MIT**. Eres libre de usarlo, modificarlo y distribuirlo.

---

## 👨‍💻 Desarrolladores

- **Versión**: 1.2
- **Framework**: .NET 10 WPF
- **Plataforma**: Windows 11 x64
- **Lenguaje**: C# 12.0

---
