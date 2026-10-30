using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    private static readonly Color ColorCompletado = new Color32(57, 211, 83, 255);
    private static readonly Color ColorEnMano = new Color32(242, 199, 68, 255);
    private static readonly Color ColorPendiente = new Color32(160, 166, 177, 255);

    private GameObject panelPrompt;
    private RectTransform rectPrompt;
    private GameObject panelMensaje;
    private TextMeshProUGUI textoPrompt;
    private TextMeshProUGUI textoMensaje;
    private TextMeshProUGUI textSalud;
    private TextMeshProUGUI textPuntaje;
    private TextMeshProUGUI textTiempo;
    private TextMeshProUGUI textSubtitulo;
    private RectTransform rellenoSalud;
    private TextMeshProUGUI estadoBateria;
    private TextMeshProUGUI estadoAceite;
    private TextMeshProUGUI estadoGasolina;
    private TextMeshProUGUI estadoLlaves;
    private PlayerHealth saludJugador;

    private void Start()
    {
        RuntimeUI.AsegurarEventSystem();
        Construir();
        saludJugador = UnityEngine.Object.FindAnyObjectByType<PlayerHealth>();

        if (CarEscapeController.Instance != null)
            CarEscapeController.Instance.UsarUIExterna = true;
    }

    private void Update()
    {
        if (LevelStateManager.Instance == null)
            return;

        ActualizarSalud();
        textPuntaje.text = "PUNTAJE   " + LevelStateManager.Instance.Puntaje.ToString("0000");
        textTiempo.text = "TIEMPO   " + FormatearTiempo(LevelStateManager.Instance.TiempoJugado);
        ActualizarObjetivos();
        ActualizarMensajes();
    }

    private void Construir()
    {
        Canvas canvas = RuntimeUI.CrearCanvas("HUD_Nivel2", 10);

        Image panelEstado = RuntimeUI.CrearPanel(canvas.transform, "PanelEstado", new Color(0.03f, 0.03f, 0.05f, 0.82f));
        panelEstado.raycastTarget = false;
        RuntimeUI.Anclar(panelEstado.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -24f), new Vector2(430f, 230f));

        TextMeshProUGUI titulo = RuntimeUI.CrearTexto(panelEstado.transform, "Titulo", "ESTADO", 20f, TextAlignmentOptions.Left, new Color32(214, 24, 24, 255));
        RuntimeUI.Anclar(titulo.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, -12f), new Vector2(398f, 26f));

        TextMeshProUGUI saludEtiqueta = RuntimeUI.CrearTexto(panelEstado.transform, "SaludEtiqueta", "SALUD", 18f, TextAlignmentOptions.Left, new Color(0.85f, 0.87f, 0.9f, 1f));
        RuntimeUI.Anclar(saludEtiqueta.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, -48f), new Vector2(160f, 24f));

        textSalud = RuntimeUI.CrearTexto(panelEstado.transform, "SaludTexto", "100 / 100", 18f, TextAlignmentOptions.Right, Color.white);
        RuntimeUI.Anclar(textSalud.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(254f, -48f), new Vector2(160f, 24f));

        Image barraFondo = RuntimeUI.CrearPanel(panelEstado.transform, "BarraSaludFondo", new Color(0.16f, 0.16f, 0.19f, 1f));
        barraFondo.raycastTarget = false;
        RuntimeUI.Anclar(barraFondo.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, -80f), new Vector2(398f, 16f));

        Image barraRelleno = RuntimeUI.CrearPanel(barraFondo.transform, "BarraSaludRelleno", new Color32(190, 30, 30, 255));
        barraRelleno.raycastTarget = false;
        rellenoSalud = barraRelleno.rectTransform;
        rellenoSalud.anchorMin = Vector2.zero;
        rellenoSalud.anchorMax = Vector2.one;
        rellenoSalud.offsetMin = Vector2.zero;
        rellenoSalud.offsetMax = Vector2.zero;

        textPuntaje = RuntimeUI.CrearTexto(panelEstado.transform, "Puntaje", "PUNTAJE   0000", 20f, TextAlignmentOptions.Left, Color.white);
        RuntimeUI.Anclar(textPuntaje.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, -108f), new Vector2(398f, 28f));

        textTiempo = RuntimeUI.CrearTexto(panelEstado.transform, "Tiempo", "TIEMPO   00:00", 18f, TextAlignmentOptions.Left, new Color(0.85f, 0.87f, 0.9f, 1f));
        RuntimeUI.Anclar(textTiempo.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, -142f), new Vector2(398f, 26f));

        TextMeshProUGUI pista = RuntimeUI.CrearTexto(panelEstado.transform, "Pista", "ESC / START   Pausa", 14f, TextAlignmentOptions.Left, new Color(0.6f, 0.63f, 0.68f, 1f));
        RuntimeUI.Anclar(pista.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, -178f), new Vector2(398f, 22f));

        Image panelObjetivos = RuntimeUI.CrearPanel(canvas.transform, "PanelObjetivos", new Color(0.03f, 0.03f, 0.05f, 0.82f));
        panelObjetivos.raycastTarget = false;
        RuntimeUI.Anclar(panelObjetivos.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(24f, 24f), new Vector2(470f, 230f));

        TextMeshProUGUI tituloObjetivos = RuntimeUI.CrearTexto(panelObjetivos.transform, "Titulo", "OBJETIVOS", 20f, TextAlignmentOptions.Left, new Color32(214, 24, 24, 255));
        RuntimeUI.Anclar(tituloObjetivos.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, -12f), new Vector2(438f, 26f));

        textSubtitulo = RuntimeUI.CrearTexto(panelObjetivos.transform, "Subtitulo", "Repara el vehículo y escapa", 15f, TextAlignmentOptions.Left, new Color(0.78f, 0.8f, 0.84f, 1f));
        RuntimeUI.Anclar(textSubtitulo.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, -40f), new Vector2(438f, 22f));

        CrearFila(panelObjetivos.transform, "Batería", -70f, out estadoBateria);
        CrearFila(panelObjetivos.transform, "Aceite", -102f, out estadoAceite);
        CrearFila(panelObjetivos.transform, "Gasolina", -134f, out estadoGasolina);
        CrearFila(panelObjetivos.transform, "Llaves", -166f, out estadoLlaves);

        Image fondoPrompt = RuntimeUI.CrearPanel(canvas.transform, "PanelPrompt", new Color(0.02f, 0.02f, 0.04f, 0.86f));
        fondoPrompt.raycastTarget = false;
        RuntimeUI.Anclar(fondoPrompt.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 48f), new Vector2(1000f, 70f));
        panelPrompt = fondoPrompt.gameObject;
        rectPrompt = fondoPrompt.rectTransform;

        textoPrompt = RuntimeUI.CrearTexto(fondoPrompt.transform, "TextoPrompt", "", 20f, TextAlignmentOptions.Center, Color.white);
        RuntimeUI.Estirar(textoPrompt.rectTransform, 16f, 16f, 10f, 10f);

        Image fondoMensaje = RuntimeUI.CrearPanel(canvas.transform, "PanelMensaje", new Color(0.02f, 0.02f, 0.04f, 0.86f));
        fondoMensaje.raycastTarget = false;
        RuntimeUI.Anclar(fondoMensaje.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -20f), new Vector2(980f, 60f));
        panelMensaje = fondoMensaje.gameObject;

        textoMensaje = RuntimeUI.CrearTexto(fondoMensaje.transform, "TextoMensaje", "", 22f, TextAlignmentOptions.Center, new Color32(242, 199, 68, 255));
        RuntimeUI.Estirar(textoMensaje.rectTransform, 16f, 16f, 8f, 8f);

        Image mira = RuntimeUI.CrearPanel(canvas.transform, "Mira", new Color(1f, 1f, 1f, 0.7f));
        mira.raycastTarget = false;
        RuntimeUI.Anclar(mira.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(6f, 6f));

        panelPrompt.SetActive(false);
        panelMensaje.SetActive(false);
    }

    private void CrearFila(Transform padre, string etiqueta, float y, out TextMeshProUGUI estado)
    {
        TextMeshProUGUI nombre = RuntimeUI.CrearTexto(padre, "Fila_" + etiqueta, etiqueta, 18f, TextAlignmentOptions.Left, new Color(0.86f, 0.88f, 0.91f, 1f));
        RuntimeUI.Anclar(nombre.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, y), new Vector2(250f, 24f));

        estado = RuntimeUI.CrearTexto(padre, "Estado_" + etiqueta, "FALTA", 18f, TextAlignmentOptions.Right, ColorPendiente);
        RuntimeUI.Anclar(estado.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(266f, y), new Vector2(188f, 24f));
    }

    private void ActualizarSalud()
    {
        if (saludJugador == null)
            saludJugador = UnityEngine.Object.FindAnyObjectByType<PlayerHealth>();

        if (saludJugador == null)
        {
            textSalud.text = "SIN DATOS";
            rellenoSalud.anchorMax = new Vector2(0f, 1f);
            return;
        }

        textSalud.text = Mathf.CeilToInt(saludJugador.Salud) + " / " + Mathf.CeilToInt(saludJugador.SaludMaxima);
        rellenoSalud.anchorMax = new Vector2(saludJugador.SaludNormalizada, 1f);
    }

    private void ActualizarObjetivos()
    {
        CarEscapeController car = CarEscapeController.Instance;
        if (car == null)
        {
            ActualizarFila(estadoBateria, "BUSCANDO", ColorPendiente);
            ActualizarFila(estadoAceite, "BUSCANDO", ColorPendiente);
            ActualizarFila(estadoGasolina, "BUSCANDO", ColorPendiente);
            ActualizarFila(estadoLlaves, "BUSCANDO", ColorPendiente);
            return;
        }

        if (!car.UsarUIExterna)
            car.UsarUIExterna = true;

        ActualizarFila(estadoBateria, car.BateriaInstalada ? "INSTALADA" : car.TieneBateria ? "EN MANO" : "FALTA",
            car.BateriaInstalada ? ColorCompletado : car.TieneBateria ? ColorEnMano : ColorPendiente);

        ActualizarFila(estadoAceite, car.AceiteLlenado ? "RELLENADO" : car.TieneAceite ? "EN MANO" : "FALTA",
            car.AceiteLlenado ? ColorCompletado : car.TieneAceite ? ColorEnMano : ColorPendiente);

        ActualizarFila(estadoGasolina, car.GasolinaCargada ? "CARGADA" : car.TieneGasolina ? "EN MANO" : "FALTA",
            car.GasolinaCargada ? ColorCompletado : car.TieneGasolina ? ColorEnMano : ColorPendiente);

        ActualizarFila(estadoLlaves, car.TieneLlaves ? "EN MANO" : "FALTA",
            car.TieneLlaves ? ColorCompletado : ColorPendiente);

        textSubtitulo.text = car.EstaListoParaArrancar
            ? "¡Escapa del estacionamiento!"
            : "Repara el vehículo y escapa";
    }

    private static void ActualizarFila(TextMeshProUGUI texto, string valor, Color color)
    {
        texto.text = valor;
        texto.color = color;
    }

    private void ActualizarMensajes()
    {
        if (LevelStateManager.Instance == null || !LevelStateManager.Instance.JuegoActivo)
        {
            panelPrompt.SetActive(false);
            panelMensaje.SetActive(false);
            return;
        }

        CarEscapeController car = CarEscapeController.Instance;
        if (car == null)
        {
            panelPrompt.SetActive(false);
            panelMensaje.SetActive(false);
            return;
        }

        textoPrompt.text = car.PromptActual;
        bool hayPrompt = !string.IsNullOrEmpty(car.PromptActual);
        panelPrompt.SetActive(hayPrompt);

        if (hayPrompt)
        {
            int lineas = 1;
            foreach (char caracter in car.PromptActual)
            {
                if (caracter == '\n')
                    lineas++;
            }

            float altura = Mathf.Clamp(48f + (lineas - 1) * 26f, 64f, 190f);
            rectPrompt.sizeDelta = new Vector2(1000f, altura);
        }

        textoMensaje.text = car.MensajeFlotante;
        bool hayMensaje = !string.IsNullOrEmpty(car.MensajeFlotante);
        panelMensaje.SetActive(hayMensaje);
    }

    private static string FormatearTiempo(float segundos)
    {
        int total = Mathf.Max(0, Mathf.FloorToInt(segundos));
        return string.Format("{0:00}:{1:00}", total / 60, total % 60);
    }
}
