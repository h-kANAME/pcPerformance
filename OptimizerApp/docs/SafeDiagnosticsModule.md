# Módulo de Diagnóstico Seguro

## Objetivo
Ofrecer una alternativa **no destructiva** para usuarios con PCs lentos, evitando limpiezas agresivas o formateos. El módulo detecta causas comunes de degradación y propone acciones seguras y reversibles.

## Principios
- **Solo lectura por defecto** (sin cambios automáticos).
- **Recomendaciones seguras** y reversibles.
- **Trazabilidad** de diagnóstico y recomendaciones.
- **Compatibilidad Windows 11** y cero impacto en estabilidad.

## Alcance funcional (MVP)
1. **Panel de salud general**
   - CPU/RAM/Disco: uso actual y picos recientes.
   - Estado de almacenamiento: espacio libre, tasa de crecimiento de temporales.
   - Índice de salud (0–100) basado en reglas simples.

2. **Detección de causas comunes**
   - **Procesos en segundo plano**: top 5 por CPU/RAM en últimos 2–5 min.
   - **Inicio de Windows**: apps de inicio activas (solo reporte).
   - **Espacio en disco**: alertas si < 15% libre.
   - **Servicios críticos**: estado básico (actualizaciones, seguridad).

3. **Recomendaciones seguras** (sin ejecución automática)
   - Sugerir desactivar apps de inicio (manual, link a Configuración).
   - Sugerir liberar espacio con herramientas nativas (Storage Sense).
   - Sugerir cerrar procesos con alto consumo (requiere confirmación y UI).

4. **Modo reporte**
   - Exportar diagnóstico a JSON/CSV para soporte.

## Fuera de alcance (MVP)
- No se modifica Registro.
- No se tocan carpetas del sistema (System32, WinSxS).
- No se ejecutan scripts de limpieza automáticamente.

## Métricas y señales
- CPU: uso promedio y picos (últimos 2–5 min).
- RAM: porcentaje de uso + memoria disponible.
- Disco: espacio libre y % fragmentación (solo lectura si es viable).
- Startup: cantidad de apps habilitadas.
- Temperatura: si sensores disponibles, mostrar “No disponible” si no.

## UX mínima requerida
- Tarjetas de diagnóstico con estado (OK/Advertencia/Crítico).
- Explicación de cada alerta y acción sugerida.
- Botón “Ver cómo resolver” (abre Configuración/URL nativa).

## Consideraciones técnicas
- Usar Performance Counters / WMI para métricas base.
- Evitar sondas costosas: intervalos configurables (p. ej. 2–5 s).
- Registrar eventos con nivel INFO/WARN en log interno.

## Criterios de aceptación
- Ninguna acción se ejecuta sin confirmación explícita.
- El módulo no degrada el rendimiento (>1–2% CPU en reposo).
- Diagnóstico exportable con un click.
