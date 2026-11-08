#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace HorrorGame.Mobile.Editor
{
    public static class MobileControlsEditorSetup
    {
        [MenuItem("Tools/Controles Móviles/Crear Canvas de Controles Móviles en Escena Actual", false, 10)]
        public static void CrearCanvasControlesMoviles()
        {
            MobileControlsUI existente = Object.FindAnyObjectByType<MobileControlsUI>();
            if (existente != null)
            {
                Selection.activeGameObject = existente.gameObject;
                EditorUtility.DisplayDialog(
                    "Controles Móviles",
                    "Ya existe un objeto con MobileControlsUI en la escena actual: '" + existente.gameObject.name + "'.",
                    "Aceptar"
                );
                return;
            }

            GameObject canvasGO = new GameObject("MobileControlsCanvas");
            MobileControlsUI ui = canvasGO.AddComponent<MobileControlsUI>();

            Undo.RegisterCreatedObjectUndo(canvasGO, "Crear Canvas de Controles Móviles");
            Selection.activeGameObject = canvasGO;

            EditorUtility.DisplayDialog(
                "¡Controles Móviles Creados!",
                "Se ha creado el Canvas de Controles Móviles exitosamente.\n\n" +
                "Incluye:\n" +
                "• Joystick virtual circular (La Rueda)\n" +
                "• Cruceta D-Pad con 4 flechas (Arriba, Abajo, Izq, Der)\n" +
                "• Botón para alternar entre Rueda, Flechas o Ambos\n" +
                "• Touchpad para rotar cámara (mitad derecha)\n" +
                "• Botones de acción: Saltar, Correr, Linterna, Interactuar, Pausa.",
                "Entendido"
            );
        }

        [MenuItem("Tools/Controles Móviles/Seleccionar Jugador y Activar Modo Móvil", false, 11)]
        public static void ConfigurarJugadorModoMovil()
        {
            PlayerController jugador = Object.FindAnyObjectByType<PlayerController>();
            if (jugador == null)
            {
                EditorUtility.DisplayDialog("Aviso", "No se encontró ningún PlayerController en la escena activa.", "OK");
                return;
            }

            Selection.activeGameObject = jugador.gameObject;
            EditorGUIUtility.PingObject(jugador.gameObject);

            EditorUtility.DisplayDialog(
                "Jugador Seleccionado",
                "Se ha seleccionado al Jugador en la jerarquía.\n" +
                "Verifica en el Inspector que 'Usar Controles Móviles' y 'Simular En Editor' estén activados para probar con el ratón en la vista Game.",
                "Entendido"
            );
        }
    }
}
#endif
