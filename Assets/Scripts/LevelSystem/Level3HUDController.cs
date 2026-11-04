using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Level3HUDController : MonoBehaviour
{
    private static readonly Color ColorCompletado = new Color(0.35f, 0.85f, 0.35f, 1f);
    private static readonly Color ColorEnProgreso = new Color(0.95f, 0.75f, 0.22f, 1f);
    private static readonly Color ColorPendiente = new Color(0.60f, 0.60f, 0.60f, 1f);

    private GameObject panelPrompt;
    private GameObject panelMensaje;
    private TextMeshProUGUI textoPrompt;
    private TextMeshProUGUI textoMensaje;
    private TextMeshProUGUI textSalud;
    private TextMeshProUGUI textPuntaje;
    private TextMeshProUGUI textTiempo;
    private RectTransform rellenoSalud;
    private Image imagenSalud;

    // Indicadores de objetivos
    private TextMeshProUGUI objVehiculo;
    private TextMeshProUGUI objPistas;
    private TextMeshProUGUI objExpediente;
    private TextMeshProUGUI objBicicleta;

    private PlayerHealth saludJugador;
    private VignetaHorror vignetaPantalla;
    private float tiempoUltimoPulsoHerida;

    private void Start()
    {
        RuntimeUI.AsegurarEventSystem();
        Construir();
        saludJugador = UnityEngine.Object.FindAnyObjectByType<PlayerHealth>();
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
        Canvas canvas = RuntimeUI.CrearCanvas("HUD_Nivel3", 10);
        vignetaPantalla = RuntimeUI.CrearVigneta(canvas.transform, 0.38f);

        // PANEL ESTADO VITAL (Superior Izquierda)
        Image panelSalud = RuntimeUI.CrearPanelMarcoTerror(canvas.transform, "PanelSalud", new Vector2(400f, 135f), RuntimeUI.ColorPanelHud);
        panelSalud.raycastTarget = false;
        RuntimeUI.Anclar(panelSalud.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -24f), new Vector2(400f, 135f));

        TextMeshProUGUI titSalud = RuntimeUI.CrearTitulo(panelSalud.transform, "TituloSalud", "ESTADO VITAL", 22f, TextAlignmentOptions.Left, RuntimeUI.ColorRojoBrillante);
        RuntimeUI.Anclar(titSalud.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, -12f), new Vector2(360f, 26f));

        TextMeshProUGUI lblSalud = RuntimeUI.CrearTexto(panelSalud.transform, "LabelSalud", "SALUD", 18f, TextAlignmentOptions.Left, RuntimeUI.ColorTexto);
        RuntimeUI.Anclar(lblSalud.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, -42f), new Vector2(120f, 22f));

        textSalud = RuntimeUI.CrearTexto(panelSalud.transform, "SaludTexto", "100%", 18f, TextAlignmentOptions.Right, RuntimeUI.ColorTexto);
        RuntimeUI.Anclar(textSalud.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(230f, -42f), new Vector2(150f, 22f));

        Image barraFondo = RuntimeUI.CrearPanel(panelSalud.transform, "BarraFondo", new Color(0.10f, 0.10f, 0.10f, 0.95f));
        barraFondo.raycastTarget = false;
        RuntimeUI.Anclar(barraFondo.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, -70f), new Vector2(368f, 16f));

        Image barraRelleno = RuntimeUI.CrearPanel(barraFondo.transform, "BarraRelleno", RuntimeUI.ColorRojoSangre);
        barraRelleno.raycastTarget = false;
        imagenSalud = barraRelleno;
        rellenoSalud = barraRelleno.rectTransform;
        rellenoSalud.anchorMin = Vector2.zero;
        rellenoSalud.anchorMax = Vector2.one;
        rellenoSalud.offsetMin = Vector2.zero;
        rellenoSalud.offsetMax = Vector2.zero;

        TextMeshProUGUI pistaPausa = RuntimeUI.CrearTexto(panelSalud.transform, "PistaPausa", "ESC / START   Pausa", 13f, TextAlignmentOptions.Left, RuntimeUI.ColorTextoTenue);
        RuntimeUI.Anclar(pistaPausa.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, -98f), new Vector2(360f, 20f));

        // PANEL OBJETIVOS (Superior Derecha)
        Image panelObjetivos = RuntimeUI.CrearPanelMarcoTerror(canvas.transform, "PanelObjetivos", new Vector2(480f, 280f), RuntimeUI.ColorPanelHud);
        panelObjetivos.raycastTarget = false;
        RuntimeUI.Anclar(panelObjetivos.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-24f, -24f), new Vector2(480f, 280f));

        textPuntaje = RuntimeUI.CrearTexto(panelObjetivos.transform, "Puntaje", "PUNTAJE   0000", 20f, TextAlignmentOptions.Left, RuntimeUI.ColorTexto);
        RuntimeUI.Anclar(textPuntaje.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(18f, -12f), new Vector2(220f, 24f));

        textTiempo = RuntimeUI.CrearTexto(panelObjetivos.transform, "Tiempo", "TIEMPO   00:00", 20f, TextAlignmentOptions.Right, RuntimeUI.ColorTexto);
        RuntimeUI.Anclar(textTiempo.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-18f, -12f), new Vector2(220f, 24f));

        TextMeshProUGUI titObj = RuntimeUI.CrearTitulo(panelObjetivos.transform, "TituloObj", "OBJETIVOS: NIVEL 3 — LA CALLE", 18f, TextAlignmentOptions.Left, RuntimeUI.ColorRojoBrillante);
        RuntimeUI.Anclar(titObj.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(18f, -44f), new Vector2(440f, 24f));

        // 4 Objetivos
        objVehiculo = RuntimeUI.CrearTexto(panelObjetivos.transform, "ObjVehiculo", "• Investigar la avería del vehículo", 15f, TextAlignmentOptions.Left, ColorCompletado);
        RuntimeUI.Anclar(objVehiculo.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(20f, -80f), new Vector2(440f, 22f));

        objPistas = RuntimeUI.CrearTexto(panelObjetivos.transform, "ObjPistas", "• Encontrar pistas y fotos en la carretera (0/3)", 15f, TextAlignmentOptions.Left, ColorEnProgreso);
        RuntimeUI.Anclar(objPistas.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(20f, -112f), new Vector2(440f, 22f));

        objExpediente = RuntimeUI.CrearTexto(panelObjetivos.transform, "ObjExpediente", "• Descubrir el expediente del origen del ente", 15f, TextAlignmentOptions.Left, ColorPendiente);
        RuntimeUI.Anclar(objExpediente.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(20f, -144f), new Vector2(440f, 22f));

        objBicicleta = RuntimeUI.CrearTexto(panelObjetivos.transform, "ObjBicicleta", "• Encontrar transporte para volver a la universidad", 15f, TextAlignmentOptions.Left, ColorPendiente);
        RuntimeUI.Anclar(objBicicleta.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(20f, -176f), new Vector2(440f, 22f));

        TextMeshProUGUI pistaLore = RuntimeUI.CrearTexto(panelObjetivos.transform, "PistaLore", "Destino: El misterio se originó en el campus", 13f, TextAlignmentOptions.Left, RuntimeUI.ColorTextoTenue);
        RuntimeUI.Anclar(pistaLore.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(20f, -220f), new Vector2(440f, 20f));

        // PANEL INFERIOR: PROMPT DE INTERACCIÓN
        Image imgPrompt = RuntimeUI.CrearPanelMarcoTerror(canvas.transform, "PromptInteraccion", new Vector2(680f, 60f), new Color(0.04f, 0.04f, 0.05f, 0.92f));
        imgPrompt.raycastTarget = false;
        panelPrompt = imgPrompt.gameObject;
        RuntimeUI.Anclar(imgPrompt.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 65f), new Vector2(680f, 60f));

        textoPrompt = RuntimeUI.CrearTexto(imgPrompt.transform, "TextoPrompt", "", 20f, TextAlignmentOptions.Center, RuntimeUI.ColorTexto);
        textoPrompt.fontStyle = FontStyles.Bold;
        RuntimeUI.Estirar(textoPrompt.rectTransform, 14f, 8f, 14f, 8f);
        panelPrompt.SetActive(false);

        // PANEL MENSAJE FLOTANTE NARRATIVO
        Image imgMensaje = RuntimeUI.CrearPanelMarcoTerror(canvas.transform, "MensajeFlotante", new Vector2(800f, 75f), new Color(0.08f, 0.02f, 0.02f, 0.92f));
        imgMensaje.raycastTarget = false;
        panelMensaje = imgMensaje.gameObject;
        RuntimeUI.Anclar(imgMensaje.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -140f), new Vector2(800f, 75f));

        textoMensaje = RuntimeUI.CrearTexto(imgMensaje.transform, "TextoMensaje", "", 18f, TextAlignmentOptions.Center, RuntimeUI.ColorTexto);
        RuntimeUI.Estirar(textoMensaje.rectTransform, 14f, 8f, 14f, 8f);
        panelMensaje.SetActive(false);
    }

    private void ActualizarObjetivos()
    {
        if (Level3Controller.Instance == null) return;

        int recolectadas = Level3Controller.Instance.PistasRecolectadas;
        int total = Level3Controller.Instance.TotalPistas;
        bool exp = Level3Controller.Instance.ExpedienteEncontrado;
        bool bici = Level3Controller.Instance.PuedeUsarBicicleta();

        objPistas.text = $"• Encontrar pistas y fotos en la carretera ({recolectadas}/{total})";
        objPistas.color = recolectadas >= total ? ColorCompletado : ColorEnProgreso;

        objExpediente.text = exp ? "• ¡Expediente encontrado! Debes regresar a la universidad" : "• Descubrir el expediente del origen del ente";
        objExpediente.color = exp ? ColorCompletado : ColorPendiente;

        objBicicleta.text = bici ? "• Montar la bicicleta para volver al campus [E]" : "• Encontrar transporte para volver a la universidad";
        objBicicleta.color = bici ? ColorEnProgreso : ColorPendiente;
    }

    private void ActualizarMensajes()
    {
        if (Level3Controller.Instance == null) return;

        string prompt = Level3Controller.Instance.PromptActual;
        if (!string.IsNullOrEmpty(prompt))
        {
            panelPrompt.SetActive(true);
            textoPrompt.text = prompt;
        }
        else
        {
            panelPrompt.SetActive(false);
        }

        string msg = Level3Controller.Instance.MensajeFlotante;
        if (!string.IsNullOrEmpty(msg))
        {
            panelMensaje.SetActive(true);
            textoMensaje.text = msg;
        }
        else
        {
            panelMensaje.SetActive(false);
        }
    }

    private void ActualizarSalud()
    {
        if (saludJugador == null) return;

        float fraccion = saludJugador.SaludNormalizada;
        textSalud.text = Mathf.CeilToInt(fraccion * 100f) + "%";

        if (rellenoSalud != null)
        {
            rellenoSalud.anchorMax = new Vector2(Mathf.Clamp01(fraccion), 1f);
        }

        if (fraccion < 0.35f && vignetaPantalla != null && Time.time - tiempoUltimoPulsoHerida > 0.8f)
        {
            tiempoUltimoPulsoHerida = Time.time;
            vignetaPantalla.PulsoMiedo(0.7f, 0.85f);
        }
    }

    private static string FormatearTiempo(float segundos)
    {
        int total = Mathf.Max(0, Mathf.FloorToInt(segundos));
        return string.Format("{0:00}:{1:00}", total / 60, total % 60);
    }
}
