using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Muestra un título cinemático al iniciar el Nivel 3 (La Calle / Carretera Solitaria)
/// con estética de terror psicológico (Silent Hill).
/// </summary>
public class TituloIntroNivel3 : MonoBehaviour
{
    [Header("Textos")]
    [SerializeField] private string titulo = "UNIVERSIDAD MALDITA";
    [SerializeField] private string subtitulo = "— NIVEL 3: LA CALLE —";
    [SerializeField] private string fraseAmbiente = "El automóvil se detuvo en la niebla... investiga las pistas y descubre cómo regresar.";

    [Header("Tiempos de Animación")]
    [SerializeField] private float retrasoInicial = 0.5f;
    [SerializeField] private float duracionFadeIn = 1.6f;
    [SerializeField] private float duracionVisible = 3.6f;
    [SerializeField] private float duracionFadeOut = 1.4f;

    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private TextMeshProUGUI txtTitulo;
    private TextMeshProUGUI txtSubtitulo;
    private TextMeshProUGUI txtFrase;

    private void Start()
    {
        ConstruirUI();
        StartCoroutine(SecuenciaTitulo());
    }

    private void ConstruirUI()
    {
        GameObject goCanvas = new GameObject("Canvas_TituloIntroNivel3");
        canvas = goCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 30; // Sobre el HUD pero bajo menús de pausa

        CanvasScaler scaler = goCanvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        goCanvas.AddComponent<GraphicRaycaster>();

        canvasGroup = goCanvas.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        // Viñeta oscura de fondo sutil para dar dramatismo cinematográfico
        GameObject goFondo = new GameObject("FondoSombra");
        goFondo.transform.SetParent(goCanvas.transform, false);
        Image imgFondo = goFondo.AddComponent<Image>();
        imgFondo.color = new Color(0.01f, 0.01f, 0.01f, 0.60f);
        imgFondo.raycastTarget = false;
        RectTransform rtFondo = goFondo.GetComponent<RectTransform>();
        rtFondo.anchorMin = Vector2.zero;
        rtFondo.anchorMax = Vector2.one;
        rtFondo.offsetMin = Vector2.zero;
        rtFondo.offsetMax = Vector2.zero;

        // Contenedor centrado para los textos
        GameObject contenedor = new GameObject("ContenedorTextos");
        contenedor.transform.SetParent(goCanvas.transform, false);
        RectTransform rtCont = contenedor.AddComponent<RectTransform>();
        rtCont.anchorMin = new Vector2(0.5f, 0.5f);
        rtCont.anchorMax = new Vector2(0.5f, 0.5f);
        rtCont.pivot = new Vector2(0.5f, 0.5f);
        rtCont.anchoredPosition = new Vector2(0f, 60f);
        rtCont.sizeDelta = new Vector2(1200f, 320f);

        // 1. TÍTULO PRINCIPAL ("UNIVERSIDAD MALDITA")
        GameObject goTit = new GameObject("TextoTitulo");
        goTit.transform.SetParent(contenedor.transform, false);
        RectTransform rtTit = goTit.AddComponent<RectTransform>();
        rtTit.anchorMin = new Vector2(0.5f, 1f);
        rtTit.anchorMax = new Vector2(0.5f, 1f);
        rtTit.pivot = new Vector2(0.5f, 1f);
        rtTit.anchoredPosition = new Vector2(0f, 0f);
        rtTit.sizeDelta = new Vector2(1200f, 110f);

        txtTitulo = goTit.AddComponent<TextMeshProUGUI>();
        txtTitulo.font = RuntimeUI.FuenteTitulo;
        txtTitulo.text = titulo;
        txtTitulo.fontSize = 68f;
        txtTitulo.characterSpacing = 6f;
        txtTitulo.alignment = TextAlignmentOptions.Center;
        txtTitulo.color = new Color(0.96f, 0.96f, 0.96f, 1f);

        // 2. SUBTÍTULO ("— NIVEL 3: LA CALLE —")
        GameObject goSub = new GameObject("TextoSubtitulo");
        goSub.transform.SetParent(contenedor.transform, false);
        RectTransform rtSub = goSub.AddComponent<RectTransform>();
        rtSub.anchorMin = new Vector2(0.5f, 1f);
        rtSub.anchorMax = new Vector2(0.5f, 1f);
        rtSub.pivot = new Vector2(0.5f, 1f);
        rtSub.anchoredPosition = new Vector2(0f, -115f);
        rtSub.sizeDelta = new Vector2(1200f, 60f);

        txtSubtitulo = goSub.AddComponent<TextMeshProUGUI>();
        txtSubtitulo.font = RuntimeUI.FuenteTexto;
        txtSubtitulo.text = subtitulo;
        txtSubtitulo.fontSize = 28f;
        txtSubtitulo.characterSpacing = 4f;
        txtSubtitulo.alignment = TextAlignmentOptions.Center;
        txtSubtitulo.color = RuntimeUI.ColorRojoBrillante;

        // 3. FRASE DE AMBIENTACIÓN / SINOPSIS DEL NIVEL
        GameObject goFrase = new GameObject("TextoFrase");
        goFrase.transform.SetParent(contenedor.transform, false);
        RectTransform rtFrase = goFrase.AddComponent<RectTransform>();
        rtFrase.anchorMin = new Vector2(0.5f, 1f);
        rtFrase.anchorMax = new Vector2(0.5f, 1f);
        rtFrase.pivot = new Vector2(0.5f, 1f);
        rtFrase.anchoredPosition = new Vector2(0f, -180f);
        rtFrase.sizeDelta = new Vector2(1100f, 50f);

        txtFrase = goFrase.AddComponent<TextMeshProUGUI>();
        txtFrase.font = RuntimeUI.FuenteTexto;
        txtFrase.text = fraseAmbiente;
        txtFrase.fontSize = 18f;
        txtFrase.alignment = TextAlignmentOptions.Center;
        txtFrase.color = new Color(0.75f, 0.75f, 0.75f, 0.90f);
    }

    private IEnumerator SecuenciaTitulo()
    {
        yield return new WaitForSeconds(retrasoInicial);

        ReproducirTonoTerrorIntro();

        // 1. FADE IN SUAVE
        float t = 0f;
        while (t < duracionFadeIn)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.SmoothStep(0f, 1f, t / duracionFadeIn);
            yield return null;
        }
        canvasGroup.alpha = 1f;

        // 2. MANTENER VISIBLE CON LEVE PULSO
        float transcurrido = 0f;
        while (transcurrido < duracionVisible)
        {
            transcurrido += Time.deltaTime;
            float respiracion = 0.92f + Mathf.Sin(transcurrido * 2.2f) * 0.08f;
            canvasGroup.alpha = respiracion;
            yield return null;
        }

        // 3. FADE OUT ELEGANTE
        t = 0f;
        while (t < duracionFadeOut)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.SmoothStep(1f, 0f, t / duracionFadeOut);
            yield return null;
        }
        canvasGroup.alpha = 0f;

        if (canvas != null)
        {
            Destroy(canvas.gameObject);
        }
        Destroy(this);
    }

    private void ReproducirTonoTerrorIntro()
    {
        GameObject goAudio = new GameObject("AudioIntroNivel3");
        AudioSource audio = goAudio.AddComponent<AudioSource>();
        audio.spatialBlend = 0f;
        audio.volume = 0.55f;

        int sampleRate = 44100;
        float duracion = 3.8f;
        int totalSamples = (int)(sampleRate * duracion);
        float[] samples = new float[totalSamples];

        for (int i = 0; i < totalSamples; i++)
        {
            float tiempo = (float)i / sampleRate;
            float envolvente = Mathf.Exp(-tiempo * 1.0f);
            // Tonos oscuros disonantes misteriosos de carretera solitaria (48Hz + 96Hz + 102Hz)
            float onda = Mathf.Sin(2f * Mathf.PI * 48f * tiempo) * 0.5f +
                         Mathf.Sin(2f * Mathf.PI * 96f * tiempo) * 0.25f +
                         Mathf.Sin(2f * Mathf.PI * 102f * tiempo) * 0.25f;
            samples[i] = onda * envolvente * 0.42f;
        }

        AudioClip clip = AudioClip.Create("StingNivel3", totalSamples, 1, sampleRate, false);
        clip.SetData(samples, 0);
        audio.clip = clip;
        audio.Play();

        Destroy(goAudio, duracion + 0.5f);
    }
}
