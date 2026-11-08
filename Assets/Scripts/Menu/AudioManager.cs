using UnityEngine;

/// <summary>
/// Aplica el volumen guardado en GameManager a todos los AudioSources
/// de la escena actual al inicio.
/// Ponlo en el GameManager o en un objeto persistente.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Mezclas de Audio")]
    [SerializeField] private AudioSource musicaActual;
    [SerializeField] private AudioSource[] fuentes;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start() => AplicarConfiguracion();

    // -----------------------------------------------------------------------
    public void AplicarConfiguracion()
    {
        if (GameManager.Instance == null) return;

        float volMusica  = GameManager.Instance.MusicaVolumen;
        float volEfectos = GameManager.Instance.EfectosVolumen;

        if (musicaActual != null) musicaActual.volume = volMusica;

        foreach (var f in fuentes)
            if (f != null) f.volume = volEfectos;
    }

    // -----------------------------------------------------------------------
    /// <summary>Reproduce un clip de efecto de sonido con el volumen correcto.</summary>
    public void ReproducirEfecto(AudioClip clip, float pitchVariacion = 0f)
    {
        if (clip == null) return;

        var src = gameObject.AddComponent<AudioSource>();
        src.clip   = clip;
        src.volume = GameManager.Instance != null
                     ? GameManager.Instance.EfectosVolumen : 1f;
        src.pitch  = 1f + Random.Range(-pitchVariacion, pitchVariacion);
        src.Play();
        Destroy(src, clip.length + 0.1f);
    }
}
