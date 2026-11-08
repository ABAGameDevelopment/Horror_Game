using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PauseMenuController : MonoBehaviour
{
    private GameObject raiz;
    private GameObject panelPrincipal;
    private GameObject panelOpciones;
    private Button botonReanudar;
    private TextMeshProUGUI txtValorMusica;
    private TextMeshProUGUI txtValorEfectos;

    private void Start()
    {
        RuntimeUI.AsegurarEventSystem();
        Construir();
        if (LevelStateManager.Instance != null)
            LevelStateManager.Instance.EstadoCambiado += AlCambiarEstado;
        raiz.SetActive(false);
    }

    private void OnDestroy()
    {
        if (LevelStateManager.Instance != null)
            LevelStateManager.Instance.EstadoCambiado -= AlCambiarEstado;
    }

    private void Update()
    {
        if (raiz != null && raiz.activeSelf && panelOpciones != null && panelOpciones.activeSelf)
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                OcultarOpciones();
            }
        }
    }

    private void Construir()
    {
        Canvas canvas = RuntimeUI.CrearCanvas("MenuPausa_Nivel2", 50);
        raiz = canvas.gameObject;

        Image fondo = RuntimeUI.CrearPanel(canvas.transform, "Fondo", new Color(0f, 0f, 0f, 0.88f));
        RuntimeUI.Estirar(fondo.rectTransform, 0f, 0f, 0f, 0f);
        RuntimeUI.CrearVigneta(canvas.transform, 0.50f);

        // -------------------------------------------------------------
        // PANEL PRINCIPAL DE PAUSA (Estilo Silent Hill 2)
        // -------------------------------------------------------------
        Image panel = RuntimeUI.CrearPanelMarcoTerror(canvas.transform, "PanelPausa", new Vector2(620f, 620f), RuntimeUI.ColorPanel);
        RuntimeUI.Anclar(panel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(620f, 620f));
        panelPrincipal = panel.gameObject;

        // Título imponente con efecto Glitch de terror
        TextMeshProUGUI titulo = RuntimeUI.CrearTituloHorror(panel.transform, "TituloPausa", "PAUSA", 84f, RuntimeUI.ColorTitulo);
        RuntimeUI.Anclar(titulo.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -30f), new Vector2(580f, 85f));

        // Subtítulo temático en rojo sangre
        TextMeshProUGUI subtitulo = RuntimeUI.CrearSubtituloHorror(panel.transform, "Subtitulo", "—   N  O     H  A  Y     S  A  L  I  D  A   —");
        RuntimeUI.Anclar(subtitulo.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -112f), new Vector2(580f, 32f));

        // Contenedor de Botones
        botonReanudar = RuntimeUI.CrearBotonHorror(panel.transform, "BotonReanudar", "REANUDAR", Reanudar);
        RuntimeUI.Anclar(botonReanudar.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -175f), new Vector2(420f, 58f));

        Button botonReiniciar = RuntimeUI.CrearBotonHorror(panel.transform, "BotonReiniciar", "REINICIAR NIVEL", Reiniciar);
        RuntimeUI.Anclar(botonReiniciar.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -248f), new Vector2(420f, 58f));

        Button botonOpciones = RuntimeUI.CrearBotonHorror(panel.transform, "BotonOpciones", "OPCIONES", MostrarOpciones);
        RuntimeUI.Anclar(botonOpciones.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -321f), new Vector2(420f, 58f));

        Button botonSalir = RuntimeUI.CrearBotonHorror(panel.transform, "BotonSalir", "SALIR AL MENÚ PRINCIPAL", IrAlMenu);
        RuntimeUI.Anclar(botonSalir.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -394f), new Vector2(420f, 58f));

        TextMeshProUGUI pista = RuntimeUI.CrearTexto(panel.transform, "Pista", "ESC / START   Reanudar la partida", 15f, TextAlignmentOptions.Center, RuntimeUI.ColorTextoTenue);
        RuntimeUI.Anclar(pista.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 22f), new Vector2(580f, 24f));

        // -------------------------------------------------------------
        // PANEL DE OPCIONES COMPLETO (Volumen, Calidad y Controles)
        // -------------------------------------------------------------
        ConstruirOpciones(canvas.transform);
    }

    private void ConstruirOpciones(Transform canvas)
    {
        Image panel = RuntimeUI.CrearPanelMarcoTerror(canvas, "PanelOpciones", new Vector2(740f, 620f), RuntimeUI.ColorPanel);
        RuntimeUI.Anclar(panel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(740f, 620f));
        panelOpciones = panel.gameObject;

        TextMeshProUGUI titulo = RuntimeUI.CrearTituloHorror(panel.transform, "TituloOpciones", "OPCIONES", 58f, RuntimeUI.ColorTitulo);
        RuntimeUI.Anclar(titulo.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -24f), new Vector2(680f, 62f));

        TextMeshProUGUI subtitulo = RuntimeUI.CrearSubtituloHorror(panel.transform, "SubOpciones", "—   AJUSTES DE AUDIO Y CONTROLES   —");
        RuntimeUI.Anclar(subtitulo.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -88f), new Vector2(680f, 28f));

        float musica = PlayerPrefs.GetFloat("MusicaVol", 0.8f);
        float efectos = PlayerPrefs.GetFloat("EfectosVol", 1f);
        if (GameManager.Instance != null)
        {
            musica = GameManager.Instance.MusicaVolumen;
            efectos = GameManager.Instance.EfectosVolumen;
        }

        // Fila Volumen Música
        TextMeshProUGUI etiquetaMusica = RuntimeUI.CrearTexto(panel.transform, "EtiquetaMusica", "VOLUMEN DE MÚSICA", 18f, TextAlignmentOptions.Left, RuntimeUI.ColorTexto);
        RuntimeUI.Anclar(etiquetaMusica.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(40f, -135f), new Vector2(300f, 26f));

        txtValorMusica = RuntimeUI.CrearTexto(panel.transform, "ValorMusica", Mathf.RoundToInt(musica * 100f) + "%", 18f, TextAlignmentOptions.Right, RuntimeUI.ColorAviso);
        RuntimeUI.Anclar(txtValorMusica.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-40f, -135f), new Vector2(120f, 26f));

        Slider sliderMusica = RuntimeUI.CrearSliderHorror(panel.transform, "SliderMusica", musica, CambiarMusica);
        RuntimeUI.Anclar(sliderMusica.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(40f, -168f), new Vector2(660f, 38f));

        // Fila Volumen Efectos
        TextMeshProUGUI etiquetaEfectos = RuntimeUI.CrearTexto(panel.transform, "EtiquetaEfectos", "VOLUMEN DE EFECTOS (SFX)", 18f, TextAlignmentOptions.Left, RuntimeUI.ColorTexto);
        RuntimeUI.Anclar(etiquetaEfectos.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(40f, -225f), new Vector2(300f, 26f));

        txtValorEfectos = RuntimeUI.CrearTexto(panel.transform, "ValorEfectos", Mathf.RoundToInt(efectos * 100f) + "%", 18f, TextAlignmentOptions.Right, RuntimeUI.ColorAviso);
        RuntimeUI.Anclar(txtValorEfectos.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-40f, -225f), new Vector2(120f, 26f));

        Slider sliderEfectos = RuntimeUI.CrearSliderHorror(panel.transform, "SliderEfectos", efectos, CambiarEfectos);
        RuntimeUI.Anclar(sliderEfectos.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(40f, -258f), new Vector2(660f, 38f));

        // Guía Rápida de Controles
        Image cuadroControles = RuntimeUI.CrearPanel(panel.transform, "CuadroControles", new Color(0.02f, 0.02f, 0.02f, 0.90f));
        RuntimeUI.Anclar(cuadroControles.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -325f), new Vector2(660f, 120f));

        TextMeshProUGUI titControles = RuntimeUI.CrearTexto(cuadroControles.transform, "TitControles", "CONTROLES DE SUPERVIVENCIA", 16f, TextAlignmentOptions.Center, RuntimeUI.ColorRojoSangre);
        titControles.fontStyle = FontStyles.Bold;
        RuntimeUI.Anclar(titControles.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -8f), new Vector2(640f, 22f));

        TextMeshProUGUI txtControles = RuntimeUI.CrearTexto(cuadroControles.transform, "TxtControles",
            "<b>[WASD]</b> Desplazarse   |   <b>[RATÓN]</b> Mirar   |   <b>[E]</b> Interactuar / Recoger\n" +
            "<b>[F]</b> Linterna   |   <b>[SHIFT]</b> Correr   |   <b>[ESC]</b> Pausa",
            15f, TextAlignmentOptions.Center, RuntimeUI.ColorTexto);
        RuntimeUI.Anclar(txtControles.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -10f), new Vector2(640f, 60f));

        // Botón < VOLVER
        Button botonVolver = RuntimeUI.CrearBotonHorror(panel.transform, "BotonVolver", "< VOLVER", OcultarOpciones);
        RuntimeUI.Anclar(botonVolver.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 26f), new Vector2(360f, 54f));

        sliderMusica.value = musica;
        sliderEfectos.value = efectos;
        panelOpciones.SetActive(false);
    }

    private void AlCambiarEstado(EstadoNivel estado)
    {
        bool pausado = estado == EstadoNivel.Pausado;
        if (raiz != null)
            raiz.SetActive(pausado);

        if (panelPrincipal != null)
            panelPrincipal.SetActive(pausado);

        if (panelOpciones != null)
            panelOpciones.SetActive(false);

        if (pausado && EventSystem.current != null && botonReanudar != null)
            EventSystem.current.SetSelectedGameObject(botonReanudar.gameObject);
    }

    private void Reanudar()
    {
        if (LevelStateManager.Instance != null)
            LevelStateManager.Instance.Reanudar();
    }

    private void Reiniciar()
    {
        if (LevelStateManager.Instance != null)
            LevelStateManager.Instance.Reiniciar();
    }

    private void IrAlMenu()
    {
        if (LevelStateManager.Instance != null)
            LevelStateManager.Instance.IrAlMenu();
    }

    private void MostrarOpciones()
    {
        if (panelPrincipal != null)
            panelPrincipal.SetActive(false);

        if (panelOpciones != null)
            panelOpciones.SetActive(true);
    }

    private void OcultarOpciones()
    {
        if (panelOpciones != null)
            panelOpciones.SetActive(false);

        if (panelPrincipal != null)
            panelPrincipal.SetActive(true);

        if (EventSystem.current != null && botonReanudar != null)
            EventSystem.current.SetSelectedGameObject(botonReanudar.gameObject);
    }

    private void CambiarMusica(float valor)
    {
        if (txtValorMusica != null)
            txtValorMusica.text = Mathf.RoundToInt(valor * 100f) + "%";

        if (GameManager.Instance != null)
            GameManager.Instance.GuardarVolumenMusica(valor);

        AudioSource[] fuentes = UnityEngine.Object.FindObjectsByType<AudioSource>(FindObjectsInactive.Exclude);
        foreach (AudioSource fuente in fuentes)
        {
            if (fuente.loop)
                fuente.volume = valor;
        }
    }

    private void CambiarEfectos(float valor)
    {
        if (txtValorEfectos != null)
            txtValorEfectos.text = Mathf.RoundToInt(valor * 100f) + "%";

        if (GameManager.Instance != null)
            GameManager.Instance.GuardarVolumenEfectos(valor);

        AudioSource[] fuentes = UnityEngine.Object.FindObjectsByType<AudioSource>(FindObjectsInactive.Exclude);
        foreach (AudioSource fuente in fuentes)
        {
            if (!fuente.loop)
                fuente.volume = valor;
        }
    }
}
