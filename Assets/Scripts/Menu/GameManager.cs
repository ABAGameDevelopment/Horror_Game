using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Singleton global que persiste entre escenas.
/// Guarda el estado del juego y expone utilidades de carga.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Nombres de escenas")]
    [Tooltip("Nombre exacto de la escena del Splash")]
    public string sceneSplash    = "Splash";
    [Tooltip("Nombre exacto de la escena del Menú Principal")]
    public string sceneMainMenu  = "MainMenu";
    [Tooltip("Nombre exacto de la escena del Nivel 1")]
    public string sceneLevel1    = "University";

    // ------ Progreso guardado ------
    public bool  PartidaExiste   => PlayerPrefs.GetInt("PartidaExiste", 0) == 1;
    public float MusicaVolumen   => PlayerPrefs.GetFloat("MusicaVol",  0.8f);
    public float EfectosVolumen  => PlayerPrefs.GetFloat("EfectosVol", 1.0f);
    public int   CalidadGrafica  => PlayerPrefs.GetInt("Calidad",      3);
    public int   NivelDesbloqueado => SaveManager.NivelDesbloqueado;

    public bool  NivelCompletado(string nivel) => SaveManager.NivelCompletado(nivel);

    // -----------------------------------------------------------------------
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // -----------------------------------------------------------------------
    //  Navegación
    // -----------------------------------------------------------------------
    public void IrAlSplash()   => CargarEscena(sceneSplash);
    public void IrAlMenu()     => CargarEscena(sceneMainMenu);
    public void NuevaPartida()
    {
        PlayerPrefs.SetInt("PartidaExiste", 1);
        PlayerPrefs.Save();
        CargarEscena(sceneLevel1);
    }
    public void ContinuarPartida()
    {
        if (PartidaExiste)
            CargarEscena(sceneLevel1);
    }
    public void Salir()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // -----------------------------------------------------------------------
    //  Configuración
    // -----------------------------------------------------------------------
    public void GuardarVolumenMusica(float v)
    {
        PlayerPrefs.SetFloat("MusicaVol", v);
        PlayerPrefs.Save();
    }
    public void GuardarVolumenEfectos(float v)
    {
        PlayerPrefs.SetFloat("EfectosVol", v);
        PlayerPrefs.Save();
    }
    public void GuardarCalidad(int index)
    {
        QualitySettings.SetQualityLevel(index, true);
        PlayerPrefs.SetInt("Calidad", index);
        PlayerPrefs.Save();
    }

    // -----------------------------------------------------------------------
    //  Helpers
    // -----------------------------------------------------------------------
    private void CargarEscena(string nombre)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(nombre);
    }
}
