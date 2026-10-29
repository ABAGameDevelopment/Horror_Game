using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controla el Menú Principal.
/// La escena de Level1 se carga en background como skybox/fondo en la cámara.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    // -----------------------------------------------------------------------
    //  Paneles
    // -----------------------------------------------------------------------
    [Header("Paneles")]
    [SerializeField] private CanvasGroup panelPrincipal;
    [SerializeField] private CanvasGroup panelOpciones;
    [SerializeField] private CanvasGroup panelCreditos;
    [SerializeField] private CanvasGroup panelOverlayNegro;
    [SerializeField] private CanvasGroup panelTutorial;

    // -----------------------------------------------------------------------
    //  Menú principal – botones
    // -----------------------------------------------------------------------
    [Header("Botones del Menú Principal")]
    [SerializeField] private Button botonJugar;
    [SerializeField] private Button botonContinuar;
    [SerializeField] private Button botonOpciones;
    [SerializeField] private Button botonCreditos;
    [SerializeField] private Button botonSalir;

    // -----------------------------------------------------------------------
    //  Opciones
    // -----------------------------------------------------------------------
    [Header("Opciones")]
    [SerializeField] private Slider sliderMusica;
    [SerializeField] private Slider sliderEfectos;
    [SerializeField] private TMP_Dropdown dropdownCalidad;
    [SerializeField] private TMP_Text textoMusica;
    [SerializeField] private TMP_Text textoEfectos;
    [SerializeField] private Button botonVolverOpciones;
    [SerializeField] private Button botonVolverCreditos;

    // -----------------------------------------------------------------------
    //  Animación del título
    // -----------------------------------------------------------------------
    [Header("Animación de título")]
    [SerializeField] private RectTransform tituloRect;
    [SerializeField] private float oscilacionTitulo = 6f;
    [SerializeField] private float velocidadOscilacion = 0.8f;

    // -----------------------------------------------------------------------
    //  Efectos de hover en botones
    // -----------------------------------------------------------------------
    [Header("Colores de botones")]
    [SerializeField] private Color colorNormal  = new Color(0.85f, 0.85f, 0.85f);
    [SerializeField] private Color colorHover   = new Color(1.0f,  0.25f, 0.25f);
    [SerializeField] private Color colorPresion = new Color(0.7f,  0.0f,  0.0f);

    // -----------------------------------------------------------------------
    //  Audio
    // -----------------------------------------------------------------------
    [Header("Audio")]
    [SerializeField] private AudioSource musicaMenu;
    [SerializeField] private AudioSource sfxHover;
    [SerializeField] private AudioSource sfxClick;
    [SerializeField] private AudioSource sfxTransicion;

    // -----------------------------------------------------------------------
    private CanvasGroup panelActivo;
    private Vector3 posicionOriginalTitulo;

    // -----------------------------------------------------------------------
    private void Start()
    {
        // Configurar volúmenes guardados
        if (GameManager.Instance != null)
        {
            if (sliderMusica != null) sliderMusica.value = GameManager.Instance.MusicaVolumen;
            if (sliderEfectos != null) sliderEfectos.value = GameManager.Instance.EfectosVolumen;
            if (dropdownCalidad != null) dropdownCalidad.value = GameManager.Instance.CalidadGrafica;

            // Mostrar / ocultar botón Continuar según si hay partida guardada
            if (botonContinuar != null)
                botonContinuar.interactable = GameManager.Instance.PartidaExiste;
        }

        if (musicaMenu != null)
        {
            musicaMenu.volume = GameManager.Instance != null
                ? GameManager.Instance.MusicaVolumen : 0.8f;
            musicaMenu.Play();
        }

        // Estado inicial de paneles
        MostrarPanel(panelPrincipal, true);
        OcultarPanel(panelOpciones);
        OcultarPanel(panelCreditos);
        if (panelTutorial != null) OcultarPanel(panelTutorial);

        if (panelOverlayNegro != null)
        {
            panelOverlayNegro.alpha          = 1f;
            panelOverlayNegro.blocksRaycasts = true;
            StartCoroutine(Fade(panelOverlayNegro, 1f, 0f, 1.2f,
                () => panelOverlayNegro.blocksRaycasts = false));
        }

        if (tituloRect != null)
            posicionOriginalTitulo = tituloRect.localPosition;

        // Sliders
        if (sliderMusica != null) sliderMusica.onValueChanged.AddListener(OnMusicaChanged);
        if (sliderEfectos != null) sliderEfectos.onValueChanged.AddListener(OnEfectosChanged);
        if (dropdownCalidad != null) dropdownCalidad.onValueChanged.AddListener(OnCalidadChanged);

        ActualizarTextoSliders();
        ConfigurarBotones();
    }

    // -----------------------------------------------------------------------
    private void Update()
    {
        // Tecla Escape para volver rápidamente al menú principal
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (panelActivo != null)
            {
                OnCerrarSubpanel();
            }
        }

        // Efecto flotante del título
        if (tituloRect != null)
        {
            float y = Mathf.Sin(Time.time * velocidadOscilacion) * oscilacionTitulo;
            tituloRect.localPosition = posicionOriginalTitulo + Vector3.up * y;
        }
    }

    // -----------------------------------------------------------------------
    //  Botones del Menú Principal
    // -----------------------------------------------------------------------
    private void ConfigurarBotones()
    {
        RegistrarBotonesHover(botonJugar,          OnJugar);
        RegistrarBotonesHover(botonContinuar,      OnContinuar);
        RegistrarBotonesHover(botonOpciones,       OnAbrirOpciones);
        RegistrarBotonesHover(botonCreditos,       OnAbrirCreditos);
        RegistrarBotonesHover(botonSalir,          OnSalir);
        RegistrarBotonesHover(botonVolverOpciones, OnCerrarSubpanel);
        RegistrarBotonesHover(botonVolverCreditos, OnCerrarSubpanel);
    }

    private void RegistrarBotonesHover(Button btn, UnityEngine.Events.UnityAction accion)
    {
        if (btn == null) return;

        var trigger = btn.GetComponent<UnityEngine.EventSystems.EventTrigger>();
        if (trigger == null)
            trigger = btn.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();

        // Click
        var entryClick = new UnityEngine.EventSystems.EventTrigger.Entry
            { eventID = UnityEngine.EventSystems.EventTriggerType.PointerClick };
        entryClick.callback.AddListener(_ => { ReproducirSFX(sfxClick); accion(); });
        trigger.triggers.Add(entryClick);

        // Hover enter
        var entryEnter = new UnityEngine.EventSystems.EventTrigger.Entry
            { eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter };
        entryEnter.callback.AddListener(_ =>
        {
            ReproducirSFX(sfxHover);
            var img = btn.GetComponent<Image>();
            if (img) img.color = colorHover;
        });
        trigger.triggers.Add(entryEnter);

        // Hover exit
        var entryExit = new UnityEngine.EventSystems.EventTrigger.Entry
            { eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit };
        entryExit.callback.AddListener(_ =>
        {
            var img = btn.GetComponent<Image>();
            if (img) img.color = colorNormal;
        });
        trigger.triggers.Add(entryExit);

        // Down
        var entryDown = new UnityEngine.EventSystems.EventTrigger.Entry
            { eventID = UnityEngine.EventSystems.EventTriggerType.PointerDown };
        entryDown.callback.AddListener(_ =>
        {
            var img = btn.GetComponent<Image>();
            if (img) img.color = colorPresion;
        });
        trigger.triggers.Add(entryDown);
    }

    // -----------------------------------------------------------------------
    //  Acciones de los botones
    // -----------------------------------------------------------------------
    private void OnJugar()
    {
        ReproducirSFX(sfxTransicion);
        StartCoroutine(TransicionHaciaEscena(() =>
        {
            if (GameManager.Instance != null)
                GameManager.Instance.NuevaPartida();
        }));
    }

    private void OnContinuar()
    {
        ReproducirSFX(sfxTransicion);
        StartCoroutine(TransicionHaciaEscena(() =>
        {
            if (GameManager.Instance != null)
                GameManager.Instance.ContinuarPartida();
        }));
    }

    private void OnAbrirOpciones()
    {
        OcultarPanel(panelPrincipal);
        MostrarPanel(panelOpciones);
        panelActivo = panelOpciones;
        StartCoroutine(FadeIn(panelOpciones, 0.2f));
    }

    private void OnAbrirCreditos()
    {
        OcultarPanel(panelPrincipal);
        MostrarPanel(panelCreditos);
        panelActivo = panelCreditos;
        StartCoroutine(FadeIn(panelCreditos, 0.2f));
    }

    public void OnCerrarSubpanel()
    {
        ReproducirSFX(sfxClick);
        if (panelOpciones != null) OcultarPanel(panelOpciones);
        if (panelCreditos != null) OcultarPanel(panelCreditos);
        if (panelTutorial != null) OcultarPanel(panelTutorial);
        panelActivo = null;

        MostrarPanel(panelPrincipal);
        StartCoroutine(FadeIn(panelPrincipal, 0.2f));
    }

    private void OnSalir()
    {
        ReproducirSFX(sfxTransicion);
        StartCoroutine(TransicionHaciaEscena(() =>
        {
            if (GameManager.Instance != null)
                GameManager.Instance.Salir();
        }));
    }

    // -----------------------------------------------------------------------
    //  Opciones
    // -----------------------------------------------------------------------
    private void OnMusicaChanged(float v)
    {
        if (musicaMenu != null) musicaMenu.volume = v;
        if (GameManager.Instance != null) GameManager.Instance.GuardarVolumenMusica(v);
        ActualizarTextoSliders();
    }

    private void OnEfectosChanged(float v)
    {
        if (GameManager.Instance != null) GameManager.Instance.GuardarVolumenEfectos(v);
        ActualizarTextoSliders();
    }

    private void OnCalidadChanged(int index)
    {
        if (GameManager.Instance != null) GameManager.Instance.GuardarCalidad(index);
    }

    private void ActualizarTextoSliders()
    {
        if (textoMusica && sliderMusica)   textoMusica.text   = $"{Mathf.RoundToInt(sliderMusica.value  * 100)}%";
        if (textoEfectos && sliderEfectos)  textoEfectos.text  = $"{Mathf.RoundToInt(sliderEfectos.value * 100)}%";
    }

    // -----------------------------------------------------------------------
    //  Utilidades UI
    // -----------------------------------------------------------------------
    private void MostrarPanel(CanvasGroup g, bool inmediato = false)
    {
        if (g == null) return;
        g.alpha          = inmediato ? 1f : 0f;
        g.interactable   = true;
        g.blocksRaycasts = true;
    }

    private void OcultarPanel(CanvasGroup g)
    {
        if (g == null) return;
        g.alpha          = 0f;
        g.interactable   = false;
        g.blocksRaycasts = false;
    }

    private IEnumerator FadeIn(CanvasGroup g, float dur = 0.35f)
    {
        if (g == null) yield break;
        g.interactable   = false;
        g.blocksRaycasts = false;
        yield return StartCoroutine(Fade(g, g.alpha, 1f, dur));
        g.interactable   = true;
        g.blocksRaycasts = true;
    }

    private IEnumerator FadeOut(CanvasGroup g, float dur = 0.25f)
    {
        if (g == null) yield break;
        g.interactable   = false;
        g.blocksRaycasts = false;
        yield return StartCoroutine(Fade(g, g.alpha, 0f, dur));
    }

    private IEnumerator Fade(CanvasGroup g, float desde, float hasta,
                             float dur, System.Action onFinish = null)
    {
        if (g == null) { onFinish?.Invoke(); yield break; }
        float t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            g.alpha = Mathf.Lerp(desde, hasta, t / dur);
            yield return null;
        }
        g.alpha = hasta;
        onFinish?.Invoke();
    }

    private IEnumerator TransicionHaciaEscena(System.Action cargaEscena)
    {
        if (panelOverlayNegro != null)
        {
            panelOverlayNegro.blocksRaycasts = true;
            yield return StartCoroutine(Fade(panelOverlayNegro, 0f, 1f, 0.8f));
        }
        yield return new WaitForSeconds(0.1f);
        cargaEscena?.Invoke();
    }

    // -----------------------------------------------------------------------
    private void ReproducirSFX(AudioSource src)
    {
        if (src != null) src.Play();
    }
}
