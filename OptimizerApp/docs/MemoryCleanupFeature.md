# 📊 Solapa de Monitoreo de Memoria

## Características Implementadas

### 1. **Monitoreo en Tiempo Real**
- **CPU**: Porcentaje de uso del procesador
- **RAM**: Utilización en GB y porcentaje
- **Chrome**: Cantidad de procesos y consumo total

### 2. **Vista de Procesos con Tres Pestañas**

#### Top Procesos por RAM
- Muestra los 10 procesos que más memoria consumen
- Columnas: Nombre, PID, RAM (MB), CPU (%)

#### Top Procesos por CPU
- Muestra los 10 procesos con más tiempo de CPU acumulado
- Columnas: Nombre, PID, CPU (s), RAM (MB)

#### Procesos Chrome
- Solo visible cuando Chrome está en ejecución
- Detalle de cada proceso Chrome
- Columnas: Nombre, PID, RAM (MB), CPU (s)

### 3. **Acciones de Limpieza**

**Botón "Limpiar Chrome"**
- Reduce el working set de todos los procesos Chrome
- Libera memoria sin cerrar los procesos
- Solo habilitado si Chrome está ejecutándose

**Botón "Top RAM"**
- Limpia los 3 procesos que más memoria usan
- Permite liberar memoria rápidamente
- Visible mientras haya procesos activos

**Botón "Limpiar Todo"**
- Reduce working set de todos los procesos
- Operación más agresiva
- Incluye información de todo el sistema

**Botón "Refrescar"**
- Actualiza estadísticas inmediatamente
- Se ejecuta automáticamente después de limpiar

### 4. **Integración sin Duplicación**

✅ **Reutiliza `SystemDiagnosticsService`**
- Nuevos métodos: `GetMemoryStats()` y `CleanMemory()`
- Usa las mismas funciones de lectura de procesos
- Comparte estructuras de datos

✅ **Patrón MVVM consistente**
- `MemoryCleanupViewModel` sigue el patrón de otros ViewModels
- Usa `RelayCommand` para acciones
- Notificación de cambios con `ObservableObject`

### 5. **Experiencia de Usuario**

- **Estado actualizado**: Mensaje en tiempo real del estado de operaciones
- **Deshabilitación inteligente**: Botones se inhabilitan durante operaciones
- **Actualización automática**: Se refresca automáticamente después de limpiar
- **Feedback visual**: Indicadores de color (verde para Chrome, amarillo para estado)

## Detalles Técnicos

### Métodos Nuevos en SystemDiagnosticsService

```csharp
// Obtiene estadísticas completas de memoria
MemoryStats GetMemoryStats(int topProcessCount = 10)

// Limpia memoria de procesos específicos
bool CleanMemory(IReadOnlyList<int> processIds)
```

### Clase MemoryStats
```csharp
- CpuUsagePercent: double?
- RamUsedGb: double?
- RamTotalGb: double?
- RamUsagePercent: double?
- ChromeProcessCount: int
- ChromeRamGb: double?
- TopRamProcesses: IReadOnlyList<ProcessUsageInfo>
- TopCpuProcesses: IReadOnlyList<ProcessUsageInfo>
- ChromeProcesses: IReadOnlyList<ProcessUsageInfo>
- Timestamp: DateTimeOffset
```

## Uso de Windows API

- **SetProcessWorkingSetSize**: Reduce el working set de un proceso
- **GlobalMemoryStatusEx**: Obtiene información de memoria del sistema
- **Process API**: Gestiona procesos del sistema

## ⚠️ Notas de Permisos

- La limpieza de memoria funciona mejor con permisos de administrador
- La aplicación pedirá elevación de permisos si es necesario para ciertos procesos
- Algunos procesos del sistema pueden no ser accesibles
