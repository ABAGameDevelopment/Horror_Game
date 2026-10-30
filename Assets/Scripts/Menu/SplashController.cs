using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controla la pantalla de Splash:
///   1. Fade-in del logo del equipo.
///   2. Fade-out y fade-in del título del juego.
///   3. Transición automática al Menú Principal.
/// </summary>
public class SplashController : MonoBehaviour
{
    [Header("Logo del equipo")]
    [SerializeField] private CanvasGroup grupoLogoEquipo;
    [SerializeField] private float duracionFadeLogoIn  = 1.5f;
    [SerializeField] private float tiempoLogoVisible   = 2.0f;
    [SerializeField] private float duracionFadeLogoOut = 1.0f;

    [Header("Título del juego")]
    [SerializeField] private CanvasGroup grupoTituloJuego;
    [SerializeField] private TMP_Text    textoSubtitulo;       // "Presiona cualquier tecla..."
    [SerializeField] private float duracionFadeTituloIn  = 2.0f;
    [SerializeField] private float tiempoEsperaMinima    = 1.5f;

    [Header("Overlay de pantalla negra")]
    [SerializeField] private CanvasGroup grupoOverlayNegro;

    [Header("Audio")]
    [SerializeField] private AudioSource audioAmbiente;   // loop tétrico de fondo
    [SerializeField] private AudioSource audioLogoSonido; // efecto al aparecer logo

    [Header("Ajustes")]
    [SerializeField] private bool saltarConTecla = true;

    private bool puedeInteractuar = false;
    private bool saltando         = false;

    // -----------------------------------------------------------------------
    private void Start()
    {
        grupoLogoEquipo.alpha  = 0f;
        grupoTituloJuego.alpha = 0f;
        if (textoSubtitulo) textoSubtitulo.alpha = 0f;

        // Aplicar volúmenes guardados
        if (GameManager.Instance != null && audioAmbiente != null)
            audioAmbiente.volume = GameManager.Instance.MusicaVolumen;

        if (audioAmbiente != null) audioAmbiente.Play();

        StartCoroutine(SecuenciaSplash());
    }

    // -----------------------------------------------------------------------
    private void Update()
    {
        if (!saltarConTecla || !puedeInteractuar || saltando) return;

        if (Input.anyKeyDown || Input.GetMouseButtonDown(0))
        {
            saltando = true;
            StopAllCoroutines();
            StartCoroutine(IrAlMenuConFade());
        }
    }

    // -----------------------------------------------------------------------
    private IEnumerator SecuenciaSplash()
    {
        // ── 1. Fade-in logo equipo ──────────────────────────────────────────
        if (audioLogoSonido != null) audioLogoSonido.Play();
        yield return StartCoroutine(Fade(grupoLogoEquipo, 0f, 1f, duracionFadeLogoIn));
        yield return new WaitForSeconds(tiempoLogoVisible);
        yield return StartCoroutine(Fade(grupoLogoEquipo, 1f, 0f, duracionFadeLogoOut));

        // ── 2. Fade-in título del juego ────────────────────────────────────
        yield return StartCoroutine(Fade(grupoTituloJuego, 0f, 1f, duracionFadeTituloIn));

        // ── 3. Parpadeo del subtítulo ──────────────────────────────────────
        puedeInteractuar = true;
        StartCoroutine(Parpadear(textoSubtitulo));

        // Espera mínima antes de ir al menú automáticamente
        yield return new WaitForSeconds(tiempoEsperaMinima + 4f);

        if (!saltando)
        {
            saltando = true;
            yield return StartCoroutine(IrAlMenuConFade());
        }
    }

    // -----------------------------------------------------------------------
    private IEnumerator IrAlMenuConFade()
    {
        yield return StartCoroutine(Fade(grupoOverlayNegro, 0f, 1f, 1.0f));
        if (GameManager.Instance != null)
            GameManager.Instance.IrAlMenu();
    }

    // -----------------------------------------------------------------------
    private IEnumerator Parpadear(TMP_Text texto)
    {
        if (texto == null) yield break;
        while (true)
        {
            yield return StartCoroutine(FadeTexto(texto, 0f, 1f, 0.6f));
            yield return new WaitForSeconds(0.3f);
            yield return StartCoroutine(FadeTexto(texto, 1f, 0f, 0.6f));
            yield return new WaitForSeconds(0.2f);
        }
    }

    private IEnumerator FadeTexto(TMP_Text texto, float desde, float hasta, float duracion)
    {
        float t = 0f;
        while (t < duracion)
        {
            t += Time.deltaTime;
            texto.alpha = Mathf.Lerp(desde, hasta, t / duracion);
            yield return null;
        }
        texto.alpha = hasta;
    }

    // -----------------------------------------------------------------------
    private IEnumerator Fade(CanvasGroup grupo, float desde, float hasta, float duracion)
    {
        if (grupo == null) yield break;
        float t = 0f;
        grupo.alpha = desde;
        while (t < duracion)
        {
            t += Time.deltaTime;
            grupo.alpha = Mathf.Lerp(desde, hasta, t / duracion);
            yield return null;
        }
        grupo.alpha = hasta;
    }
}
