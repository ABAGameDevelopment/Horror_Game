using UnityEngine;
using UnityEngine.SceneManagement;

public static class Level2Bootstrap
{
    private const string EscenaNivel2 = "Level2";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Registrar()
    {
        SceneManager.sceneLoaded -= AlCargarEscena;
        SceneManager.sceneLoaded += AlCargarEscena;
        AlCargarEscena(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private static void AlCargarEscena(Scene escena, LoadSceneMode modo)
    {
        if (escena.name != EscenaNivel2)
            return;

        CrearSistemas();
    }

    private static void CrearSistemas()
    {
        if (UnityEngine.Object.FindAnyObjectByType<LevelStateManager>() != null)
            return;

        GameObject sistemas = new GameObject("Sistemas_Nivel2");
        sistemas.AddComponent<LevelStateManager>();
        sistemas.AddComponent<HUDController>();
        sistemas.AddComponent<PauseMenuController>();
        sistemas.AddComponent<LevelCompleteController>();
        sistemas.AddComponent<GameOverController>();

        AsegurarSaludJugador();
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
