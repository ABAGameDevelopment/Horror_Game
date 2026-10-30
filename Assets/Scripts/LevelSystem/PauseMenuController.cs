using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PauseMenuController : MonoBehaviour
{
    private GameObject raiz;
    private GameObject panelOpciones;
    private Button botonReanudar;

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

    private void Construir()
    {
        Canvas canvas = RuntimeUI.CrearCanvas("MenuPausa_Nivel2", 50);
        raiz = canvas.gameObject;

        Image fondo = RuntimeUI.CrearPanel(canvas.transform, "Fondo", new Color(0f, 0f, 0f, 0.82f));
        RuntimeUI.Estirar(fondo.rectTransform, 0f, 0f, 0f, 0f);

        Image panel = RuntimeUI.CrearPanel(canvas.transform, "PanelPausa", new Color(0.05f, 0.05f, 0.07f, 0.98f));
        RuntimeUI.Anclar(panel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(560f, 560f));

        TextMeshProUGUI titulo = RuntimeUI.CrearTexto(panel.transform, "Titulo", "PAUSA", 48f, TextAlignmentOptions.Center, new Color32(214, 24, 24, 255));
        RuntimeUI.Anclar(titulo.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -26f), new Vector2(520f, 60f));

        TextMeshProUGUI subtitulo = RuntimeUI.CrearTexto(panel.transform, "Subtitulo", "El estacionamiento te espera", 18f, TextAlignmentOptions.Center, new Color(0.72f, 0.75f, 0.8f, 1f));
        RuntimeUI.Anclar(subtitulo.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -86f), new Vector2(520f, 30f));

        botonReanudar = RuntimeUI.CrearBoton(panel.transform, "BotonReanudar", "REANUDAR", Reanudar);
        RuntimeUI.Anclar(botonReanudar.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -150f), new Vector2(380f, 56f));

        Button botonReiniciar = RuntimeUI.CrearBoton(panel.transform, "BotonReiniciar", "REINICIAR NIVEL", Reiniciar);
        RuntimeUI.Anclar(botonReiniciar.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -220f), new Vector2(380f, 56f));

        Button botonOpciones = RuntimeUI.CrearBoton(panel.transform, "BotonOpciones", "OPCIONES", MostrarOpciones);
        RuntimeUI.Anclar(botonOpciones.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -290f), new Vector2(380f, 56f));

        Button botonSalir = RuntimeUI.CrearBoton(panel.transform, "BotonSalir", "SALIR AL MENÚ", IrAlMenu);
        RuntimeUI.Anclar(botonSalir.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -360f), new Vector2(380f, 56f));

        TextMeshProUGUI pista = RuntimeUI.CrearTexto(panel.transform, "Pista", "ESC / START   Reanudar", 14f, TextAlignmentOptions.Center, new Color(0.6f, 0.63f, 0.68f, 1f));
        RuntimeUI.Anclar(pista.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 24f), new Vector2(520f, 24f));

        ConstruirOpciones(canvas.transform);
    }

    private void ConstruirOpciones(Transform canvas)
    {
        Image panel = RuntimeUI.CrearPanel(canvas, "PanelOpciones", new Color(0.05f, 0.05f, 0.07f, 0.99f));
        RuntimeUI.Anclar(panel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(620f, 480f));
        panelOpciones = panel.gameObject;

        TextMeshProUGUI titulo = RuntimeUI.CrearTexto(panel.transform, "Titulo", "OPCIONES", 38f, TextAlignmentOptions.Center, new Color32(214, 24, 24, 255));
        RuntimeUI.Anclar(titulo.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -24f), new Vector2(560f, 50f));

        float musica = PlayerPrefs.GetFloat("MusicaVol", 0.8f);
        float efectos = PlayerPrefs.GetFloat("EfectosVol", 1f);
        if (GameManager.Instance != null)
        {
            musica = GameManager.Instance.MusicaVolumen;
            efectos = GameManager.Instance.EfectosVolumen;
        }

        TextMeshProUGUI etiquetaMusica = RuntimeUI.CrearTexto(panel.transform, "EtiquetaMusica", "MÚSICA", 18f, TextAlignmentOptions.Left, new Color(0.86f, 0.88f, 0.91f, 1f));
        RuntimeUI.Anclar(etiquetaMusica.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(30f, -110f), new Vector2(560f, 26f));

        Slider sliderMusica = RuntimeUI.CrearSlider(panel.transform, "SliderMusica", musica, CambiarMusica);
        RuntimeUI.Anclar(sliderMusica.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(30f, -146f), new Vector2(560f, 44f));

        TextMeshProUGUI etiquetaEfectos = RuntimeUI.CrearTexto(panel.transform, "EtiquetaEfectos", "EFECTOS", 18f, TextAlignmentOptions.Left, new Color(0.86f, 0.88f, 0.91f, 1f));
        RuntimeUI.Anclar(etiquetaEfectos.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(30f, -214f), new Vector2(560f, 26f));

        Slider sliderEfectos = RuntimeUI.CrearSlider(panel.transform, "SliderEfectos", efectos, CambiarEfectos);
        RuntimeUI.Anclar(sliderEfectos.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(30f, -250f), new Vector2(560f, 44f));

        Button botonVolver = RuntimeUI.CrearBoton(panel.transform, "BotonVolver", "VOLVER", OcultarOpciones);
        RuntimeUI.Anclar(botonVolver.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 28f), new Vector2(360f, 54f));

        sliderMusica.value = musica;
        sliderEfectos.value = efectos;
        panelOpciones.SetActive(false);
    }

    private void AlCambiarEstado(EstadoNivel estado)
    {
        bool pausado = estado == EstadoNivel.Pausado;
        if (raiz != null)
            raiz.SetActive(pausado);

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
        if (panelOpciones != null)
            panelOpciones.SetActive(true);
    }

    private void OcultarOpciones()
    {
        if (panelOpciones != null)
            panelOpciones.SetActive(false);

        if (EventSystem.current != null && botonReanudar != null)
            EventSystem.current.SetSelectedGameObject(botonReanudar.gameObject);
    }

    private void CambiarMusica(float valor)
    {
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
