
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SprintUI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private PlayerController jugador;

    [Header("Configuración visual")]
    [SerializeField] private Vector2 posicion = new Vector2(35f, 35f);

    private Image barraRelleno;
    private TMP_Text textoPorcentaje;

    private void Start()
    {
        if (jugador == null)
        {
            jugador = GetComponent<PlayerController>();
        }

        if (jugador == null)
        {
            Debug.LogError("SprintUI: No se encontró PlayerController.");
            return;
        }

        CrearInterfaz();

        jugador.SprintEnergyChanged += ActualizarBarra;

        ActualizarBarra(jugador.SprintEnergy);
    }

    private void CrearInterfaz()
    {
        // Canvas principal
        GameObject canvasGO = new GameObject(
            "Canvas_Sprint",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster)
        );

        Canvas canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 20;

        CanvasScaler scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // Panel de energía
        GameObject panelGO = new GameObject(
            "Panel_Sprint",
            typeof(RectTransform),
            typeof(Image)
        );

        panelGO.transform.SetParent(canvasGO.transform, false);

        RectTransform panelRect = panelGO.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.zero;
        panelRect.pivot = Vector2.zero;
        panelRect.anchoredPosition = posicion;
        panelRect.sizeDelta = new Vector2(280, 65);

        Image panelImage = panelGO.GetComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.55f);

        // Texto SPRINT
        CrearTexto(
            "Texto_Sprint",
            panelGO.transform,
            "SPRINT",
            new Vector2(12, 20),
            new Vector2(65, 25),
            17,
            TextAlignmentOptions.Center
        );

        // Fondo de la barra
        GameObject fondoGO = new GameObject(
            "Fondo_Barra",
            typeof(RectTransform),
            typeof(Image)
        );

        fondoGO.transform.SetParent(panelGO.transform, false);

        RectTransform fondoRect = fondoGO.GetComponent<RectTransform>();
        fondoRect.anchorMin = Vector2.zero;
        fondoRect.anchorMax = Vector2.zero;
        fondoRect.pivot = Vector2.zero;
        fondoRect.anchoredPosition = new Vector2(85, 23);
        fondoRect.sizeDelta = new Vector2(145, 18);

        fondoGO.GetComponent<Image>().color =
            new Color(0.25f, 0.25f, 0.25f, 1f);

        // Relleno de la barra
        GameObject rellenoGO = new GameObject(
            "Relleno_Barra",
            typeof(RectTransform),
            typeof(Image)
        );

        rellenoGO.transform.SetParent(fondoGO.transform, false);

        RectTransform rellenoRect = rellenoGO.GetComponent<RectTransform>();
        rellenoRect.anchorMin = Vector2.zero;
        rellenoRect.anchorMax = Vector2.one;
        rellenoRect.offsetMin = Vector2.zero;
        rellenoRect.offsetMax = Vector2.zero;

        barraRelleno = rellenoGO.GetComponent<Image>();
        barraRelleno.color = new Color(0.2f, 0.8f, 0.3f, 1f);
        barraRelleno.type = Image.Type.Filled;
        barraRelleno.fillMethod = Image.FillMethod.Horizontal;
        barraRelleno.fillOrigin = (int)Image.OriginHorizontal.Left;

        // Porcentaje
        textoPorcentaje = CrearTexto(
            "Texto_Porcentaje",
            panelGO.transform,
            "100%",
            new Vector2(232, 20),
            new Vector2(42, 25),
            15,
            TextAlignmentOptions.Center
        );
    }

    private TMP_Text CrearTexto(
        string nombre,
        Transform padre,
        string contenido,
        Vector2 posicionLocal,
        Vector2 tamaño,
        float tamañoFuente,
        TextAlignmentOptions alineacion)
    {
        GameObject textoGO = new GameObject(
            nombre,
            typeof(RectTransform),
            typeof(TextMeshProUGUI)
        );

        textoGO.transform.SetParent(padre, false);

        RectTransform rect = textoGO.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.zero;
        rect.pivot = Vector2.zero;
        rect.anchoredPosition = posicionLocal;
        rect.sizeDelta = tamaño;

        TMP_Text texto = textoGO.GetComponent<TMP_Text>();
        texto.text = contenido;
        texto.fontSize = tamañoFuente;
        texto.color = Color.white;
        texto.alignment = alineacion;
        texto.fontStyle = FontStyles.Bold;

        return texto;
    }

    private void ActualizarBarra(float energia)
    {
        if (barraRelleno == null || textoPorcentaje == null)
            return;

        energia = Mathf.Clamp01(energia);

        barraRelleno.fillAmount = energia;

        int porcentaje = Mathf.RoundToInt(energia * 100f);
        textoPorcentaje.text = porcentaje + "%";

        if (energia > 0.5f)
        {
            barraRelleno.color = new Color(0.2f, 0.8f, 0.3f);
        }
        else if (energia > 0.2f)
        {
            barraRelleno.color = new Color(1f, 0.7f, 0.1f);
        }
        else
        {
            barraRelleno.color = new Color(0.9f, 0.2f, 0.2f);
        }
    }

    private void OnDestroy()
    {
        if (jugador != null)
        {
            jugador.SprintEnergyChanged -= ActualizarBarra;
        }
    }
}
