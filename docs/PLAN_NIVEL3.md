# Plan de Diseño y Producción — Nivel 3 "La Calle" (La Carretera)

## 1. Sinopsis Narrativa

> **Nivel 3 — La calle**  
> Después de escapar del campus, el jugador conduce por una carretera solitaria entre el bosque, pero el automóvil se detiene repentinamente tras una falla mecánica en el motor. Obligado a continuar a pie en medio de una densa niebla nocturna, encuentra notas y fotografías vinculadas a la universidad. Las pistas revelan información sobre el origen de la criatura y conducen al descubrimiento de un expediente confidencial que demuestra que el ente nació de experimentos en las instalaciones universitarias. El jugador comprende que para detener definitivamente a la criatura no puede simplemente huir: tendrá que regresar al lugar del que acaba de escapar. Al encontrar la carretera principal bloqueada por un derrumbe, localiza una bicicleta en una vieja parada de autobús y emprende el camino de vuelta hacia la universidad.

**Objetivo principal:** Investigar las pistas, descubrir la conexión entre la criatura y la universidad y encontrar una forma de regresar.

---

## 2. Storyboard Técnico — Escena 03: La Carretera Solitaria

**LOCACIÓN:** Carretera boscosa / Ruta Universitaria Sur  
**AMBIENTACIÓN:** Medianoche, niebla densa volumétrica, asfalto húmedo, postes de luz parpadeantes.

```
========================================================================================================
|                                 STORYBOARD LA CARRETERA — ESCENA 03                                  |
|                                 LOCACIÓN: CARRETERA BOSCOSA / NIEBLA                                 |
========================================================================================================

+-------------------------------------+-------------------------------------+-------------------------------------+
| FRAME 1                             | FRAME 2                             | FRAME 3                             |
| [Auto varado echando vapor/humo]    | [Niebla espesa y carretera oscura]  | [Hito vial con fotografía antigua]  |
|                                     |                                     |                                     |
| DESCRIPCIÓN:                        | DESCRIPCIÓN:                        | DESCRIPCIÓN:                        |
| El automóvil se detiene de forma    | El jugador desciende del auto a la  | Explorando el acotamiento, el       |
| abrupta en el asfalto. El motor     | carretera solitaria. La densa niebla| jugador encuentra una fotografía    |
| echa vapor y las intermitentes de   | cubre el bosque y las pocas farolas | aérea del campus y notas extrañas   |
| emergencia parpadean en la noche.   | parpadean débilmente.               | clavadas en un hito kilométrico.    |
|                                     |                                     |                                     |
| SHOT:                               | SHOT:                               | SHOT:                               |
| Primera persona — plano subjetivo   | Primera persona — plano subjetivo   | Primer plano / Plano detalle        |
|                                     |                                     |                                     |
| EQUIPAMIENTO:                       | EQUIPAMIENTO:                       | EQUIPAMIENTO:                       |
| Linterna                            | Linterna                            | Linterna y fotografías              |
|                                     |                                     |                                     |
| AUDIO:                              | AUDIO:                              | AUDIO:                              |
| Siseo de radiador, motor que muere  | Viento gélido entre los pinos,      | Crujido de papel, susurro distante  |
| y chasquido de luces de advertencia.| grillos cesando repentinamente.     | en el bosque.                       |
+-------------------------------------+-------------------------------------+-------------------------------------+
| FRAME 4                             | FRAME 5                             | FRAME 6                             |
| [Expediente en caseta de autobús]   | [Carretera bloqueada al frente]     | [Bicicleta lista para el regreso]   |
|                                     |                                     |                                     |
| DESCRIPCIÓN:                        | DESCRIPCIÓN:                        | DESCRIPCIÓN:                        |
| En una caseta de autobús abandonada,| El jugador descubre que el camino   | Junto a la caseta, encuentra una    |
| halla el expediente confidencial que| hacia la ciudad está bloqueado por  | bicicleta con luz de dinamo. Al     |
| confirma que la criatura nació en el| derrumbes. Comprende que la única   | montarla, la cámara se aleja mien-  |
| laboratorio subterráneo del campus. | salida es regresar a la universidad.| tras pedalea de vuelta a la niebla. |
|                                     |                                     |                                     |
| SHOT:                               | SHOT:                               | SHOT:                               |
| Primer plano — plano subjetivo      | Plano subjetivo / Gran angular      | Plano general con alejamiento       |
|                                     |                                     | de cámara                           |
| EQUIPAMIENTO:                       | EQUIPAMIENTO:                       | EQUIPAMIENTO:                       |
| Linterna y Expediente clasificado   | Linterna y Expediente               | Linterna y Bicicleta                |
|                                     |                                     |                                     |
| AUDIO:                              | AUDIO:                              | AUDIO:                              |
| Tono siniestro y estática sutil     | Golpe hueco distante, eco de ramas  | Timbre metálico de campana (ring) y |
| al revelar la verdad de la criatura.| quebrándose en la oscuridad.        | sonido rítmico de cadena y pedales. |
+-------------------------------------+-------------------------------------+-------------------------------------+
```

---

## 3. Cadena de Objetivos de Juego (Gameplay Loop)

1. **Objetivo 1:** *Investigar la avería del vehículo.*
   - El jugador arranca junto al auto rojo con las intermitentes y el radiador averiado.
2. **Objetivo 2:** *Explorar la carretera y recolectar las pistas (0/3).*
   - **Pista 1 (z = 32m):** Fotografía del campus con registros de vigilancia.
   - **Pista 2 (z = 72m):** Reporte de incidente biológico entre cajas de transporte volcadas.
3. **Objetivo 3:** *Descubrir el expediente clasificado (z = 125m).*
   - Ubicado en la caseta / parada de autobús "Parada Universitaria".
   - Al leerlo se desbloquea la narrativa: para erradicar a la criatura hay que regresar al laboratorio de la universidad.
4. **Objetivo 4:** *Encontrar transporte para el regreso.*
   - La carretera hacia adelante (z = 145m) está cortada por un desprendimiento de rocas.
   - Junto a la caseta se encuentra la bicicleta con faro frontal.
5. **Objetivo 5:** *Montar la bicicleta y regresar.*
   - Interacción con [E] / [X] en la bicicleta.
   - Secuencia de escape y transición hacia la pantalla de nivel completado / Nivel 1 (Universidad).

---

## 4. Componentes y Scripts Creados

| Script | Ubicación | Función |
|---|---|---|
| `Level3Controller.cs` | `Assets/Scripts/Level3/` | Controlador maestro del flujo narrativo, progreso de pistas, desbloqueo de bicicleta y eventos de audio procedural. |
| `PistaInteractuableLevel3.cs` | `Assets/Scripts/Level3/` | Lógica de interacción para notas, fotografías y el expediente con soporte de UI e imágenes en pantalla. |
| `BicicletaInteractuable.cs` | `Assets/Scripts/Level3/` | Vehículo final interactuable con verificación de expediente, iluminación y cierre del nivel. |
| `CarBreakdownSequence.cs` | `Assets/Scripts/Level3/` | Simulación del coche varado con intermitentes ámbar sincronizadas y siseo de vapor. |
| `Level3HUDController.cs` | `Assets/Scripts/LevelSystem/` | HUD temático de terror que muestra la barra de salud, tiempo, puntaje y la lista interactiva de objetivos del Nivel 3. |
| `Level3Bootstrap.cs` | `Assets/Scripts/LevelSystem/` | Inicialización automática en tiempo de ejecución de los sistemas de estado, pausa, HUD y game over al cargar la escena. |
| `Level3SceneryBuilder.cs` | `Assets/Editor/` | Generador automático de la escenografía completa (carretera de 160m, acotamientos, bosque de coníferas, farolas parpadeantes, auto averiado, pistas, caseta y bicicleta). |

---

## 5. Escena y Configuración en Unity

- **Escena creada:** `Assets/Scenes/Level3/Level3.unity`
- **Build Settings:** Registrada con índice correspondiente en `ProjectSettings/EditorBuildSettings.asset`.
- **Menú de Editor:** Disponible en Unity bajo `Tools > Nivel 3 > Construir Escenografia Carretera (Nivel 3)`.
