using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controlador cinemático del Menú Principal estilo Silent Hill 2:
/// - Pasillo en penumbra con linterna viva.
/// - Cámara con respiración lenta y deriva cinematográfica.
/// - Apagones súbitos programados (las luces del techo se apagan de golpe,
///   quedando solo la linterna entre sombras).
/// - Splash cinematográfico inicial (Logo del equipo -> Título -> Menú) todo fluido.
/// - Funciona 100% sin necesidad de audios externos (audio opcional).
/// </summary>
public class HorrorMenuCinematic : MonoBehaviour
{
    [Header("--- CÁMARA Y LINTERNA ---")]
    [SerializeField] private Transform camaraTransform;
    [SerializeField] private Light linternaSpot;
    [SerializeField] private float velocidadRespiracion = 0.5f;
    [SerializeField] private float amplitudRespiracion = 0.08f;
    [SerializeField] private float balanceoCamara = 0.6f;
    [SerializeField] private float avanceLento = 0.015f; // Lentísimo avance hacia el fondo

    [Header("--- APAGONES DE MIEDO ---")]
    [Tooltip("Luces del techo del pasillo que se apagarán en eventos de horror")]
    [SerializeField] private List<Light> lucesPasillo = new List<Light>();
    [SerializeField] private float intervaloMinApagon = 6f;
    [SerializeField] private float intervaloMaxApagon = 14f;
    [SerializeField] private float duracionMinApagon = 1.2f;
    [SerializeField] private float duracionMaxApagon = 2.5f;

    [Header("--- SPLASH Y SECUENCIA DE ENTRADA ---")]
    [SerializeField] private CanvasGroup grupoSplash;
    [SerializeField] private TMP_Text textoLogoEquipo;
    [SerializeField] private CanvasGroup grupoMenuUI;
    [SerializeField] private CanvasGroup overlayNegro;
    [SerializeField] private bool omitirSplash = false;

    [Header("--- AUDIO OPCIONAL (déjalo vacío si no tienes audios) ---")]
    [SerializeField] private AudioSource audioClickLinterna;
    [SerializeField] private AudioSource audioChisporroteo;
    [SerializeField] private AudioSource audioAmbiente;

    private Vector3 posInicialCamara;
    private Quaternion rotInicialCamara;
    private Dictionary<Light, float> intensidadesOriginales = new Dictionary<Light, float>();
    private float linternaIntensidadBase;
    private bool enApagon = false;

    private void Awake()
    {
        if (camaraTransform == null && Camera.main != null)
        {
            camaraTransform = Camera.main.transform;
        }

        if (camaraTransform != null)
        {
            posInicialCamara = camaraTransform.position;
            rotInicialCamara = camaraTransform.rotation;
        }

        if (linternaSpot != null)
        {
            linternaIntensidadBase = linternaSpot.intensity;
        }

        // Si no se asignaron luces manualmente, busca luces cercanas en la escena
        if (lucesPasillo == null || lucesPasillo.Count == 0)
        {
            Light[] todas = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
            foreach (Light l in todas)
            {
                if (l != linternaSpot && l.type != LightType.Directional)
                {
                    lucesPasillo.Add(l);
                }
            }
        }

        // Guardar intensidades originales de cada luz
        foreach (Light l in lucesPasillo)
        {
            if (l != null && !intensidadesOriginales.ContainsKey(l))
            {
                intensidadesOriginales[l] = l.intensity;
            }
        }
    }

    private void Start()
    {
        // Iniciar en negro si hay splash
        if (!omitirSplash && overlayNegro != null)
        {
            overlayNegro.alpha = 1f;
            if (linternaSpot != null) linternaSpot.enabled = false;
            StartCoroutine(SecuenciaSplashCinematica());
        }
        else
        {
            if (overlayNegro != null) overlayNegro.alpha = 0f;
            if (linternaSpot != null) linternaSpot.enabled = true;
            if (grupoMenuUI != null)
            {
                grupoMenuUI.alpha = 1f;
                grupoMenuUI.interactable = true;
                grupoMenuUI.blocksRaycasts = true;
            }
        }

        StartCoroutine(BucleApagonesTerror());
    }

    private void Update()
    {
        ActualizarCamaraCinematica();
        ActualizarFlickerLinterna();
    }

    // -----------------------------------------------------------------------
    //  CÁMARA CON EFECTO DE RESPIRACIÓN Y DERIVA
    // -----------------------------------------------------------------------
    private void ActualizarCamaraCinematica()
    {
        if (camaraTransform == null) return;

        float tiempo = Time.time;

        // Respiración suave en Y
        float offsetY = Mathf.Sin(tiempo * velocidadRespiracion * 2f) * amplitudRespiracion;
        // Balanceo sutil en X
        float offsetX = Mathf.Cos(tiempo * velocidadRespiracion) * (amplitudRespiracion * 0.5f);

        // Deriva lentísima hacia adelante para sensación claustrofóbica
        float avance = Mathf.PingPong(tiempo * avanceLento, 0.4f);

        camaraTransform.position = posInicialCamara +
            (camaraTransform.forward * avance) +
            new Vector3(offsetX, offsetY, 0f);

        // Inclinación leve (Handheld camera feel)
        float balanceoZ = Mathf.Sin(tiempo * velocidadRespiracion * 1.5f) * balanceoCamara;
        float balanceoPitch = Mathf.Cos(tiempo * velocidadRespiracion * 0.8f) * (balanceoCamara * 0.5f);

        camaraTransform.rotation = rotInicialCamara *
            Quaternion.Euler(balanceoPitch, 0f, balanceoZ);
    }

    // -----------------------------------------------------------------------
    //  FLICKER ORGÁNICO DE LA LINTERNA
    // -----------------------------------------------------------------------
    private void ActualizarFlickerLinterna()
    {
        if (linternaSpot == null || !linternaSpot.enabled) return;

        // Micro-flicker natural usando PerlinNoise
        float ruido = Mathf.PerlinNoise(Time.time * 12f, 0f);
        float factor = Mathf.Lerp(0.92f, 1.05f, ruido);

        // Si estamos en apagón, la linterna parpadea con más inestabilidad
        if (enApagon && Random.value < 0.08f)
        {
            factor = Random.Range(0.2f, 0.8f);
        }

        linternaSpot.intensity = linternaIntensidadBase * factor;
    }

    // -----------------------------------------------------------------------
    //  EVENTO DE TERROR: APAGÓN SÚBITO EN EL PASILLO
    // -----------------------------------------------------------------------
    private IEnumerator BucleApagonesTerror()
    {
        // Esperar unos segundos antes del primer evento
        yield return new WaitForSeconds(8f);

        while (true)
        {
            float espera = Random.Range(intervaloMinApagon, intervaloMaxApagon);
            yield return new WaitForSeconds(espera);

            yield return StartCoroutine(EjecutarApagon());
        }
    }

    private IEnumerator EjecutarApagon()
    {
        enApagon = true;

        // 1. Chisporroteo previo (1 o 2 parpadeos rápidos)
        for (int i = 0; i < 3; i++)
        {
            SetLucesTecho(0.1f);
            yield return new WaitForSeconds(0.06f);
            SetLucesTecho(1f);
            yield return new WaitForSeconds(0.08f);
        }

        // 2. ¡APAGÓN TOTAL! (Se va la luz de golpe)
        SetLucesTecho(0f);
        if (audioChisporroteo != null) audioChisporroteo.Play();

        float duracionApagon = Random.Range(duracionMinApagon, duracionMaxApagon);
        yield return new WaitForSeconds(duracionApagon);

        // 3. Regreso inestable: chisporrotea antes de encender
        for (int i = 0; i < 4; i++)
        {
            SetLucesTecho(Random.Range(0.3f, 0.8f));
            yield return new WaitForSeconds(0.05f);
            SetLucesTecho(0f);
            yield return new WaitForSeconds(0.04f);
        }

        // 4. Se restablecen las luces
        RestablecerLucesTecho();
        enApagon = false;
    }

    private void SetLucesTecho(float factor)
    {
        foreach (Light l in lucesPasillo)
        {
            if (l != null && intensidadesOriginales.ContainsKey(l))
            {
                l.intensity = intensidadesOriginales[l] * factor;
            }
        }
    }

    private void RestablecerLucesTecho()
    {
        foreach (Light l in lucesPasillo)
        {
            if (l != null && intensidadesOriginales.ContainsKey(l))
            {
                l.intensity = intensidadesOriginales[l];
            }
        }
    }

    // -----------------------------------------------------------------------
    //  SECUENCIA SPLASH CINEMÁTICA ESTILO SILENT HILL
    // -----------------------------------------------------------------------
    private IEnumerator SecuenciaSplashCinematica()
    {
        if (grupoMenuUI != null)
        {
            grupoMenuUI.alpha = 0f;
            grupoMenuUI.interactable = false;
            grupoMenuUI.blocksRaycasts = false;
        }

        if (grupoSplash != null)
        {
            grupoSplash.alpha = 0f;

            // 1. Mostrar nombre de estudio / equipo
            yield return new WaitForSeconds(0.6f);
            yield return StartCoroutine(FadeCanvasGroup(grupoSplash, 0f, 1f, 1.4f));
            yield return new WaitForSeconds(1.8f);
            yield return StartCoroutine(FadeCanvasGroup(grupoSplash, 1f, 0f, 1.0f));
            yield return new WaitForSeconds(0.5f);
        }

        // 2. Sonido y encendido de linterna súbito
        if (audioClickLinterna != null) audioClickLinterna.Play();
        if (linternaSpot != null) linternaSpot.enabled = true;

        // 3. Desvanecer la pantalla negra revelando el pasillo
        if (overlayNegro != null)
        {
            yield return StartCoroutine(FadeCanvasGroup(overlayNegro, 1f, 0f, 1.5f));
        }

        // 4. Entrada suave del Menú Principal
        if (grupoMenuUI != null)
        {
            yield return StartCoroutine(FadeCanvasGroup(grupoMenuUI, 0f, 1f, 1.2f));
            grupoMenuUI.interactable = true;
            grupoMenuUI.blocksRaycasts = true;
        }

        if (audioAmbiente != null)
        {
            audioAmbiente.Play();
        }
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float desde, float hasta, float duracion)
    {
        if (cg == null) yield break;
        float t = 0f;
        cg.alpha = desde;
        while (t < duracion)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(desde, hasta, t / duracion);
            yield return null;
        }
        cg.alpha = hasta;
    }
}
