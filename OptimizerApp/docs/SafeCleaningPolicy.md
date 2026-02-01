# Política de limpieza segura (MVP)

## Principios
- **No destructivo por defecto:** toda operación inicia en modo previsualización.
- **Confirmación explícita:** ningún borrado sin aprobación del usuario.
- **Sin registro ni memoria crítica:** no se toca el registro de Windows ni archivos del sistema.
- **Trazabilidad:** todas las acciones quedan registradas.

## Alcance permitido
- **Temporales del usuario:** %TEMP% y %LocalAppData%\Temp.
- **Cachés no críticas:** thumbnails, font cache (solo si se valida que se puede recrear).
- **Archivos de apps específicas:** solo en ubicaciones conocidas y con listas blancas.

## Alcance no permitido
- Registro de Windows.
- System32, WinSxS, archivos de sistema y drivers.
- Prefetch: solo opcional y con advertencia.

## Mitigaciones
- **Previsualización detallada** con tamaño total y conteo de archivos.
- **Modo reporte** (sin borrado) para diagnóstico.
- **Envío a Papelera** cuando sea posible.
- **Puntos de restauración** opcionales cuando el usuario lo acepte.

## UX mínima requerida
- Avisos claros de riesgo.
- Texto de confirmación con resumen.
- Historial accesible de operaciones.
