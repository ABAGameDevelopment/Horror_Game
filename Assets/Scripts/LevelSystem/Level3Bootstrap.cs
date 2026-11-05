using UnityEngine;
using UnityEngine.SceneManagement;

public static class Level3Bootstrap
{
    private const string EscenaNivel3 = "Level3";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Registrar()
    {
        SceneManager.sceneLoaded -= AlCargarEscena;
        SceneManager.sceneLoaded += AlCargarEscena;
        AlCargarEscena(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private static void AlCargarEscena(Scene escena, LoadSceneMode modo)
    {
        if (!EsNivel3(escena))
            return;

        CrearSistemas(escena);
    }

    private static bool EsNivel3(Scene escena)
    {
        if (escena.name.Contains("Level2") || (!string.IsNullOrEmpty(escena.path) && escena.path.Contains("Level2")))
            return false;

        if (escena.name == EscenaNivel3 || (!string.IsNullOrEmpty(escena.path) && escena.path.Contains("Level3")))
            return true;

        foreach (GameObject raiz in escena.GetRootGameObjects())
        {
            if (raiz.GetComponentInChildren<Level3Controller>(true) != null)
                return true;
        }

        return false;
    }

    private static void CrearSistemas(Scene escena)
    {
        // Limpiar cualquier UI rezagada del Nivel 2 o Nivel 1 si existiera
        LimpiarUIRezagada();

        if (UnityEngine.Object.FindAnyObjectByType<LevelStateManager>() != null)
            return;

        GameObject sistemas = new GameObject("Sistemas_Nivel3");
        LevelStateManager stateManager = sistemas.AddComponent<LevelStateManager>();
        stateManager.Configurar("Nivel3", "University", 1);

        // Configuración para el Nivel 3
        sistemas.AddComponent<Level3HUDController>();
        sistemas.AddComponent<PauseMenuController>();
        sistemas.AddComponent<LevelCompleteController>();
        sistemas.AddComponent<GameOverController>();
        sistemas.AddComponent<TituloIntroNivel3>();

        // Si no hay Level3Controller en la escena, asegurarlo
        if (UnityEngine.Object.FindAnyObjectByType<Level3Controller>() == null)
        {
            GameObject ctrlObj = new GameObject("Controlador_Nivel3");
            ctrlObj.AddComponent<Level3Controller>();
        }

        AsegurarSaludJugador();

        Debug.Log("[Nivel 3] Sistemas de estado, HUD y menús iniciados correctamente en la escena: " + escena.name);
    }

    private static void LimpiarUIRezagada()
    {
        string[] nombresRezagados = {
            "Sistemas_Nivel2",
            "Canvas_Nivel2",
            "HUD_Nivel2",
            "Canvas_TituloIntroNivel2",
            "CanvasObjetivos",
            "GestorObjetivos"
        };

        foreach (string n in nombresRezagados)
        {
            GameObject go = GameObject.Find(n);
            if (go != null)
            {
                UnityEngine.Object.Destroy(go);
            }
        }
    }

    private static void AsegurarSaludJugador()
    {
        GameObject jugador = GameObject.FindWithTag("Player");
        if (jugador == null)
        {
            PlayerController controlador = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
            if (controlador != null)
                jugador = controlador.gameObject;
        }

        if (jugador != null && jugador.GetComponent<PlayerHealth>() == null)
            jugador.AddComponent<PlayerHealth>();
    }
}
