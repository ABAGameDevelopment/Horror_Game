using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    private static readonly Color ColorCompletado = new Color(0.35f, 0.85f, 0.35f, 1f);
    private static readonly Color ColorEnMano = new Color(0.95f, 0.75f, 0.22f, 1f);
    private static readonly Color ColorPendiente = new Color(0.60f, 0.60f, 0.60f, 1f);

    private GameObject panelPrompt;
    private RectTransform rectPrompt;
    private GameObject panelMensaje;
    private TextMeshProUGUI textoPrompt;
    private TextMeshProUGUI textoMensaje;
    private TextMeshProUGUI textSalud;
    private TextMeshProUGUI textPuntaje;
    private TextMeshProUGUI textTiempo;
    private TextMeshProUGUI textSubtitulo;
    private TextMeshProUGUI textProgresoAuto;
    private RectTransform rellenoSalud;
    private Image imagenSalud;
    private TextMeshProUGUI estadoBateria;
    private TextMeshProUGUI estadoAceite;
    private TextMeshProUGUI estadoGasolina;
    private TextMeshProUGUI estadoLlaves;
    private PlayerHealth saludJugador;
    private VignetaHorror vignetaPantalla;
    private float tiempoUltimoPulsoHerida;

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
        vignetaPantalla = RuntimeUI.CrearVigneta(canvas.transform, 0.38f);

        // -------------------------------------------------------------
        // PANEL SUPERIOR IZQUIERDO: SALUD Y CONDICIÓN (Estilo Menú Principal)
        // -------------------------------------------------------------
        Image panelSalud = RuntimeUI.CrearPanelMarcoTerror(canvas.transform, "PanelSalud", new Vector2(420f, 150f), RuntimeUI.ColorPanelHud);
        panelSalud.raycastTarget = false;
        RuntimeUI.Anclar(panelSalud.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -24f), new Vector2(420f, 150f));

        TextMeshProUGUI titSalud = RuntimeUI.CrearTitulo(panelSalud.transform, "TituloSalud", "ESTADO VITAL", 24f, TextAlignmentOptions.Left, RuntimeUI.ColorRojoBrillante);
        RuntimeUI.Anclar(titSalud.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, -14f), new Vector2(380f, 28f));

        TextMeshProUGUI lblSalud = RuntimeUI.CrearTexto(panelSalud.transform, "LabelSalud", "SALUD", 20f, TextAlignmentOptions.Left, RuntimeUI.ColorTexto);
        RuntimeUI.Anclar(lblSalud.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, -48f), new Vector2(140f, 24f));

        textSalud = RuntimeUI.CrearTexto(panelSalud.transform, "SaludTexto", "100%", 20f, TextAlignmentOptions.Right, RuntimeUI.ColorTexto);
        RuntimeUI.Anclar(textSalud.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(240f, -48f), new Vector2(160f, 24f));

        // Barra de Salud con fondo oscuro y relleno rojo sangre
        Image barraFondo = RuntimeUI.CrearPanel(panelSalud.transform, "BarraFondo", new Color(0.10f, 0.10f, 0.10f, 0.95f));
        barraFondo.raycastTarget = false;
        RuntimeUI.Anclar(barraFondo.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, -80f), new Vector2(388f, 18f));

        Image barraRelleno = RuntimeUI.CrearPanel(barraFondo.transform, "BarraRelleno", RuntimeUI.ColorRojoSangre);
        barraRelleno.raycastTarget = false;
        imagenSalud = barraRelleno;
        rellenoSalud = barraRelleno.rectTransform;
        rellenoSalud.anchorMin = Vector2.zero;
        rellenoSalud.anchorMax = Vector2.one;
        rellenoSalud.offsetMin = Vector2.zero;
        rellenoSalud.offsetMax = Vector2.zero;

        TextMeshProUGUI pistaPausa = RuntimeUI.CrearTexto(panelSalud.transform, "PistaPausa", "ESC / START   Pausa", 14f, TextAlignmentOptions.Left, RuntimeUI.ColorTextoTenue);
        RuntimeUI.Anclar(pistaPausa.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, -112f), new Vector2(380f, 22f));

        // -------------------------------------------------------------
        // PANEL SUPERIOR DERECHO: PUNTAJE, TIEMPO Y OBJETIVOS (Estilo Menú Principal)
        // -------------------------------------------------------------
        Image panelObjetivos = RuntimeUI.CrearPanelMarcoTerror(canvas.transform, "PanelObjetivos", new Vector2(460f, 290f), RuntimeUI.ColorPanelHud);
        panelObjetivos.raycastTarget = false;
        RuntimeUI.Anclar(panelObjetivos.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-24f, -24f), new Vector2(460f, 290f));

        textPuntaje = RuntimeUI.CrearTexto(panelObjetivos.transform, "Puntaje", "PUNTAJE   0000", 22f, TextAlignmentOptions.Left, RuntimeUI.ColorTexto);
        RuntimeUI.Anclar(textPuntaje.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(18f, -14f), new Vector2(210f, 26f));

        textTiempo = RuntimeUI.CrearTexto(panelObjetivos.transform, "Tiempo", "TIEMPO   00:00", 20f, TextAlignmentOptions.Right, RuntimeUI.ColorTextoTenue);
        RuntimeUI.Anclar(textTiempo.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-18f, -14f), new Vector2(200f, 26f));

        TextMeshProUGUI titObjetivos = RuntimeUI.CrearTitulo(panelObjetivos.transform, "TituloObjetivos", "PIEZAS DEL VEHÍCULO", 24f, TextAlignmentOptions.Left, RuntimeUI.ColorRojoBrillante);
        RuntimeUI.Anclar(titObjetivos.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(18f, -48f), new Vector2(420f, 26f));

        textProgresoAuto = RuntimeUI.CrearTexto(panelObjetivos.transform, "ProgresoAuto", "AUTO DE ESCAPE: [0/4 PIEZAS]", 17f, TextAlignmentOptions.Left, RuntimeUI.ColorAviso);
        RuntimeUI.Anclar(textProgresoAuto.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(18f, -76f), new Vector2(420f, 22f));

        CrearFilaObjetivo(panelObjetivos.transform, "⚡ BATERÍA (Taller)", -110f, out estadoBateria);
        CrearFilaObjetivo(panelObjetivos.transform, "🛢️ ACEITE (Mecánica)", -144f, out estadoAceite);
        CrearFilaObjetivo(panelObjetivos.transform, "⛽ GASOLINA (Depósito)", -178f, out estadoGasolina);
        CrearFilaObjetivo(panelObjetivos.transform, "🔑 LLAVES (Caseta)", -212f, out estadoLlaves);

        textSubtitulo = RuntimeUI.CrearTexto(panelObjetivos.transform, "Subtitulo", "Repara el vehículo y escapa", 16f, TextAlignmentOptions.Left, RuntimeUI.ColorTextoTenue);
        RuntimeUI.Anclar(textSubtitulo.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(18f, -250f), new Vector2(420f, 24f));

        // -------------------------------------------------------------
        // BANNER DE INTERACCIÓN / PROMPT (Inferior Central)
        // -------------------------------------------------------------
        Image fondoPrompt = RuntimeUI.CrearPanelMarcoTerror(canvas.transform, "PanelPrompt", new Vector2(900f, 68f), new Color(0.04f, 0.04f, 0.04f, 0.95f));
        fondoPrompt.raycastTarget = false;
        RuntimeUI.Anclar(fondoPrompt.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 100f), new Vector2(900f, 68f));
        rectPrompt = fondoPrompt.rectTransform;
        panelPrompt = fondoPrompt.gameObject;

        textoPrompt = RuntimeUI.CrearTexto(fondoPrompt.transform, "TextoPrompt", "", 24f, TextAlignmentOptions.Center, RuntimeUI.ColorTexto);
        textoPrompt.fontStyle = FontStyles.Bold;
        RuntimeUI.Estirar(textoPrompt.rectTransform, 16f, 16f, 8f, 8f);

        // Mensaje flotante de aviso temporal
        Image fondoMensaje = RuntimeUI.CrearPanel(canvas.transform, "PanelMensaje", new Color(0.02f, 0.02f, 0.02f, 0.90f));
        fondoMensaje.raycastTarget = false;
        RuntimeUI.Anclar(fondoMensaje.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -120f), new Vector2(700f, 50f));
        panelMensaje = fondoMensaje.gameObject;

        textoMensaje = RuntimeUI.CrearTexto(fondoMensaje.transform, "TextoMensaje", "", 22f, TextAlignmentOptions.Center, RuntimeUI.ColorAviso);
        RuntimeUI.Estirar(textoMensaje.rectTransform, 16f, 16f, 8f, 8f);

        // Retícula / Punto de mira sutil
        Image mira = RuntimeUI.CrearPanel(canvas.transform, "Mira", new Color(1f, 1f, 1f, 0.65f));
        mira.raycastTarget = false;
        RuntimeUI.Anclar(mira.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(5f, 5f));

        panelPrompt.SetActive(false);
        panelMensaje.SetActive(false);
    }

    private void CrearFilaObjetivo(Transform padre, string etiqueta, float y, out TextMeshProUGUI estado)
    {
        TextMeshProUGUI nombre = RuntimeUI.CrearTexto(padre, "Fila_" + etiqueta, etiqueta, 18f, TextAlignmentOptions.Left, RuntimeUI.ColorTexto);
        RuntimeUI.Anclar(nombre.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(18f, y), new Vector2(260f, 24f));

        estado = RuntimeUI.CrearTexto(padre, "Estado_" + etiqueta, "FALTA", 17f, TextAlignmentOptions.Right, ColorPendiente);
        estado.fontStyle = FontStyles.Bold;
        RuntimeUI.Anclar(estado.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(280f, y), new Vector2(160f, 24f));
    }

    private void ActualizarSalud()
    {
        if (saludJugador == null)
            saludJugador = UnityEngine.Object.FindAnyObjectByType<PlayerHealth>();

        if (saludJugador == null)
        {
            textSalud.text = "100%";
            rellenoSalud.anchorMax = new Vector2(1f, 1f);
            return;
        }

        float normalizada = saludJugador.SaludNormalizada;
        textSalud.text = Mathf.CeilToInt(normalizada * 100f) + "%";
        rellenoSalud.anchorMax = new Vector2(normalizada, 1f);

        // Color de la barra de salud según estado
        if (normalizada > 0.6f)
            imagenSalud.color = RuntimeUI.ColorRojoSangre;
        else if (normalizada > 0.3f)
            imagenSalud.color = new Color(0.85f, 0.40f, 0.10f, 1f);
        else
            imagenSalud.color = new Color(0.95f, 0.05f, 0.05f, 1f);

        // Pulso de viñeta sangrienta si la salud es crítica (< 35%)
        if (normalizada < 0.35f && vignetaPantalla != null && Time.time - tiempoUltimoPulsoHerida > 1.2f)
        {
            tiempoUltimoPulsoHerida = Time.time;
            vignetaPantalla.PulsoMiedo(0.8f, 0.88f);
        }
    }

    private void ActualizarObjetivos()
    {
        CarEscapeController car = CarEscapeController.Instance;
        if (car == null)
        {
            ActualizarFila(estadoBateria, "FALTA", ColorPendiente);
            ActualizarFila(estadoAceite, "FALTA", ColorPendiente);
            ActualizarFila(estadoGasolina, "FALTA", ColorPendiente);
            ActualizarFila(estadoLlaves, "FALTA", ColorPendiente);
            return;
        }

        if (!car.UsarUIExterna)
            car.UsarUIExterna = true;

        int instaladas = 0;
        if (car.BateriaInstalada) instaladas++;
        if (car.AceiteLlenado) instaladas++;
        if (car.GasolinaCargada) instaladas++;
        if (car.TieneLlaves) instaladas++;

        textProgresoAuto.text = car.EstaListoParaArrancar
            ? "<color=#55FF55>¡AUTO REPARADO! [4/4 PIEZAS]</color>"
            : "AUTO DE ESCAPE: [" + instaladas + "/4 PIEZAS INSTALADAS]";

        ActualizarFila(estadoBateria, car.BateriaInstalada ? "INSTALADA" : car.TieneBateria ? "EN MANO" : "FALTA",
            car.BateriaInstalada ? ColorCompletado : car.TieneBateria ? ColorEnMano : ColorPendiente);

        ActualizarFila(estadoAceite, car.AceiteLlenado ? "RELLENADO" : car.TieneAceite ? "EN MANO" : "FALTA",
            car.AceiteLlenado ? ColorCompletado : car.TieneAceite ? ColorEnMano : ColorPendiente);

        ActualizarFila(estadoGasolina, car.GasolinaCargada ? "CARGADA" : car.TieneGasolina ? "EN MANO" : "FALTA",
            car.GasolinaCargada ? ColorCompletado : car.TieneGasolina ? ColorEnMano : ColorPendiente);

        ActualizarFila(estadoLlaves, car.TieneLlaves ? "EN MANO" : "FALTA",
            car.TieneLlaves ? ColorCompletado : ColorPendiente);

        textSubtitulo.text = car.EstaListoParaArrancar
            ? "¡Sube al auto y escapa por el portón!"
            : "Encuentra las piezas en las zonas del estacionamiento";
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

            float altura = Mathf.Clamp(54f + (lineas - 1) * 26f, 68f, 190f);
            rectPrompt.sizeDelta = new Vector2(900f, altura);
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
