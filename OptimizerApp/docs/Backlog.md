# Backlog (MVP) - Diagnóstico seguro

## Épica 1: Panel de salud
- **Historia 1.1:** Como usuario quiero ver CPU/RAM/Disco en tiempo real para entender el estado actual.
  - Criterios: refresco configurable, sin afectar rendimiento.
- **Historia 1.2:** Como usuario quiero un puntaje de salud y estado (OK/Advertencia/Crítico).
  - Criterios: reglas simples y trazables.

## Épica 2: Detección de causas comunes
- **Historia 2.1:** Como usuario quiero ver los procesos con mayor consumo reciente.
  - Criterios: top 5, solo lectura.
- **Historia 2.2:** Como usuario quiero ver apps de inicio habilitadas.
  - Criterios: solo lectura; link a Configuración.
- **Historia 2.3:** Como usuario quiero alertas por poco espacio en disco.
  - Criterios: umbrales 20/15/10%.

## Épica 3: Recomendaciones seguras
- **Historia 3.1:** Como usuario quiero recomendaciones concretas y reversibles.
  - Criterios: no ejecutar acciones automáticamente.
- **Historia 3.2:** Como usuario quiero abrir rápidamente la Configuración correcta.
  - Criterios: deep links ms-settings.

## Épica 4: Exportación y auditoría
- **Historia 4.1:** Como usuario quiero exportar un reporte JSON/CSV.
  - Criterios: un clic, incluye timestamp y métricas.
- **Historia 4.2:** Como usuario quiero un historial de diagnósticos.
  - Criterios: log local rotativo.

## Épica 5: Calidad y performance
- **Historia 5.1:** Como usuario quiero que el módulo consuma < 2% CPU en reposo.
- **Historia 5.2:** Como equipo quiero tests de reglas de scoring.
