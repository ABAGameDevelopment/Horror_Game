# Plan de mejora — Nivel 2 "Estacionamiento"

## Objetivo

Convertir el Nivel 2 en un nivel propio, más grande, con el mismo estilo visual del Nivel 1,
los 4 estados obligatorios, HUD, audio y una cadena de objetivos completa.

**Sin enemigo/IA por ahora**: solo se conservan los waypoints existentes.

## Reglas de seguridad

- Trabajar en la rama `feature/level2-polish` (no tocar `main` ni `Level1/University.unity`).
- Toda la escenografía se genera desde `Assets/Editor/Level2SceneryBuilder.cs` (reproducible).
- Commits pequeños y PR al final de cada fase.
- No eliminar `Escenografia_Estacionamiento` a mano; el builder lo regenera.

---

## Fase 0 — Preparación (29 oct)

- [x] Crear rama, commit base y tag de respaldo.
- [x] Auditar `Level2.unity` y el builder; confirmar que no se toca nada de enemigo.
- [x] Agregar `Level2` a `ProjectSettings/EditorBuildSettings.asset`.
- [x] Build de prueba para validar que compila (batchmode, 0 errores de compilación).
- **Entregable:** rama lista + build base funcional.

## Fase 1 — Sistemas base compartidos (30 oct – 2 nov) — COMPLETADA

Scripts nuevos en `Assets/Scripts/LevelSystem/`:

- [x] `LevelStateManager.cs`: estados Jugar / Pausado / Fin del Nivel / Game Over, tiempo y puntaje.
- [x] `HUDController.cs`: salud, puntaje, tiempo, objetivos e inventario con TMP.
- [x] `PauseMenuController.cs`: Reanudar, Reiniciar, Opciones (volumen) y Salir.
- [x] `LevelCompleteController.cs`: victoria con puntaje, tiempo, récord y siguiente nivel.
- [x] `GameOverController.cs`: reintentar o menú.
- [x] `SaveManager.cs`: progreso/nivel desbloqueado con `PlayerPrefs` (integrado a `Menu/GameManager.cs`).
- [x] `PlayerHealth.cs`: salud, daño, curación y evento de muerte.
- [x] `RuntimeUI.cs`: fábrica de canvas, paneles, textos, botones y sliders uGUI/TMP.
- [x] `Level2Bootstrap.cs`: crea los sistemas al cargar la escena `Level2`.
- [x] Integrar HUD y estados en el Nivel 2 con estilo TMP igual al `ObjetivoUI` del Nivel 1.
- [x] `CarEscapeController`: expone progreso, desactiva su `OnGUI` cuando el HUD está activo,
      otorga puntaje y completa el nivel al escapar.
- [x] `Level2ItemPickup`: se detiene de forma segura durante la pausa.
- [x] Compilación batchmode validada: 0 errores.

**Aceptación:** los 4 estados funcionan, la pausa detiene el juego, puntaje/tiempo se actualizan,
HUD adaptable.

**Pendiente para Fase 2:** conectar un peligro ambiental o temporizador que dispare Game Over
(sin enemigo/IA).

**Rúbrica:** 20 pts (niveles y estados) + parte de 15 pts (HUD).

## Fase 2 — Expansión y rediseño del Nivel 2 (30 oct – 4 nov)

Ampliar `Level2SceneryBuilder.cs` con:

- Zonas nuevas: entrada/caseta, taller, subestación, depósito de combustible, muelle de carga,
  subterráneo/túnel y azotea.
- Verticalidad: rampas, escaleras, pasarelas y pasillos entre contenedores.
- Rutas/atajos y puertas que se desbloquean con llave o fusible.
- Sala secreta de vigilancia con monitores CCTV y nota de lore.
- Cadena de objetivos: recoger 4 piezas → instalarlas en el coche → accionar generador →
  abrir portón → escapar.
- Condición de Game Over sin enemigo: temporizador (amanecer / batería de linterna) o peligro
  ambiental.
- Puntaje por tiempo, items y secretos; rango S/A/B.

**Aceptación:** nivel jugable de principio a fin, más grande, con rutas claras y objetivo completo.

**Rúbrica:** 15 pts (mecánicas y dificultad) + 10 pts (arte/narrativa).

## Fase 3 — Coherencia visual con Nivel 1 (2 – 5 nov)

- Reutilizar `Global Volume`/post-proceso: Bloom, Vignette, Film Grain, Color Adjustments,
  Chromatic Aberration.
- Paleta y niebla iguales: sodio naranja + luna azul + emergencia roja.
- Materiales URP coherentes: asfalto mojado, charcos, óxido, graffiti, señalética.
- Reutilizar props/TMP de `Assets/Materials/Objects/` y `Assets/Scripts/ObjetivoUI.cs`.
- Iluminación de calidad (mixed/baked) y parpadeos con `HorrorLightFlicker`.

**Aceptación:** capturas del Nivel 2 y Nivel 1 se sienten del mismo juego.

## Fase 4 — Audio y feedback (3 – 5 nov)

- Ambiente multicapa y música con transición menú → nivel.
- Pasos por superficie reutilizando `Assets/Audios/Footsteps.mp3`.
- SFX: recoger, instalar, portón, motor, alarma, luces y UI.
- Partículas: polvo, humo, chispas, vapor, lluvia, salpicaduras.
- Animaciones: cofre, portón, items girando, transiciones.
- Créditos y licencias de recursos.

**Aceptación:** al menos 3 SFX sincronizados + música en loop + créditos.

## Fase 5 — QA, builds y entrega (5 – 6 nov, Avance 3)

- Playtest completo y corrección de bugs.
- Optimización: luces horneadas, occlusion culling, LOD, combinar mallas.
- Builds WebGL + PC (+ Android cuando toque).
- Documentación, video y PR.

**Aceptación:** nivel estable, sin errores críticos, jugable en build.

---

## Cronograma resumido

| Fecha | Entregable |
|---|---|
| 29 oct | Rama + build base + `Level2` en Build Settings |
| 2 nov | 4 estados + HUD + guardado |
| 4 nov | Nivel 2 ampliado y jugable de inicio a fin |
| 5 nov | Coherencia visual + audio + feedback |
| 6 nov | QA, builds y PR (Avance 3) |
| 27 nov | Integración final con Niveles 3/4, enemigo y entrega |

## Riesgos y mitigación

- **Romper el Nivel 1** → no se toca; todo en rama y builder aparte.
- **El builder regenera y pisa cambios manuales** → hacer todo por código en el builder.
- **Escena copia del Nivel 1** → mantener el interior oculto y separar visualmente el
  estacionamiento.
- **Falta de assets de audio** → usar los existentes y generar SFX procedurales como ya hace
  `CarEscapeController`.

## Decisiones pendientes

1. ~~¿El Nivel 2 debe quedar como escena dedicada o seguimos usando la copia de `University`?~~
   Resuelto: escena dedicada `Assets/Scenes/Level2/Level2.unity` (la copia `University.unity` no se toca).
2. ¿Hay assets de audio/modelos con licencia o se generan placeholders?
3. El enemigo/IA queda en pausa hasta tener el asset definitivo.
