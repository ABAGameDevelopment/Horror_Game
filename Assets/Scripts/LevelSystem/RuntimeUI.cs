using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public static class RuntimeUI
{
    private static TMP_FontAsset fuente;

    public static TMP_FontAsset Fuente
    {
        get
        {
            if (fuente == null)
            {
                fuente = TMP_Settings.defaultFontAsset;
                if (fuente == null)
                    fuente = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            }

            return fuente;
        }
    }

    public static void AsegurarEventSystem()
    {
        if (UnityEngine.Object.FindAnyObjectByType<EventSystem>() != null)
            return;

        GameObject go = new GameObject("EventSystem_Runtime");
        go.AddComponent<EventSystem>();
        InputSystemUIInputModule modulo = go.AddComponent<InputSystemUIInputModule>();
        modulo.AssignDefaultActions();
    }

    public static Canvas CrearCanvas(string nombre, int orden)
    {
        GameObject go = new GameObject(nombre);
        Canvas canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = orden;

        CanvasScaler escalador = go.AddComponent<CanvasScaler>();
        escalador.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        escalador.referenceResolution = new Vector2(1920f, 1080f);
        escalador.matchWidthOrHeight = 0.5f;

        go.AddComponent<GraphicRaycaster>();

        return canvas;
    }

    public static RectTransform CrearRect(Transform padre, string nombre)
    {
        GameObject go = new GameObject(nombre, typeof(RectTransform));
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.SetParent(padre, false);
        return rect;
    }

    public static Image CrearPanel(Transform padre, string nombre, Color color)
    {
        RectTransform rect = CrearRect(padre, nombre);
        Image imagen = rect.gameObject.AddComponent<Image>();
        imagen.color = color;
        return imagen;
    }

    public static TextMeshProUGUI CrearTexto(Transform padre, string nombre, string texto, float tamano, TextAlignmentOptions alineacion, Color color)
    {
        RectTransform rect = CrearRect(padre, nombre);
        TextMeshProUGUI tmp = rect.gameObject.AddComponent<TextMeshProUGUI>();
        if (Fuente != null)
            tmp.font = Fuente;

        tmp.text = texto;
        tmp.fontSize = tamano;
        tmp.alignment = alineacion;
        tmp.color = color;
        tmp.raycastTarget = false;
        return tmp;
    }

    public static Button CrearBoton(Transform padre, string nombre, string texto, UnityEngine.Events.UnityAction accion)
    {
        RectTransform rect = CrearRect(padre, nombre);

        Image imagen = rect.gameObject.AddComponent<Image>();
        imagen.color = new Color(0.12f, 0.12f, 0.15f, 0.96f);

        Button boton = rect.gameObject.AddComponent<Button>();
        boton.targetGraphic = imagen;

        ColorBlock colores = boton.colors;
        colores.normalColor = new Color(0.12f, 0.12f, 0.15f, 0.96f);
        colores.highlightedColor = new Color(0.42f, 0.11f, 0.11f, 1f);
        colores.pressedColor = new Color(0.65f, 0.08f, 0.08f, 1f);
        colores.selectedColor = colores.highlightedColor;
        colores.fadeDuration = 0.08f;
        boton.colors = colores;

        if (accion != null)
            boton.onClick.AddListener(accion);

        TextMeshProUGUI etiqueta = CrearTexto(rect, "Texto", texto, 28f, TextAlignmentOptions.Center, Color.white);
        Estirar(etiqueta.rectTransform, 8f, 8f, 4f, 4f);

        return boton;
    }

    public static Slider CrearSlider(Transform padre, string nombre, float valor, UnityEngine.Events.UnityAction<float> alCambiar)
    {
        RectTransform raiz = CrearRect(padre, nombre);
        Slider slider = raiz.gameObject.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;
        slider.value = Mathf.Clamp01(valor);

        RectTransform fondo = CrearRect(raiz, "Fondo");
        Estirar(fondo, 0f, 0f, 10f, 10f);
        Image imagenFondo = fondo.gameObject.AddComponent<Image>();
        imagenFondo.color = new Color(0.16f, 0.16f, 0.19f, 1f);

        RectTransform areaRelleno = CrearRect(raiz, "AreaRelleno");
        Estirar(areaRelleno, 0f, 0f, 10f, 10f);

        RectTransform relleno = CrearRect(areaRelleno, "Relleno");
        Estirar(relleno, 0f, 0f, 0f, 0f);
        Image imagenRelleno = relleno.gameObject.AddComponent<Image>();
        imagenRelleno.color = new Color(0.66f, 0.12f, 0.12f, 1f);

        RectTransform areaHandle = CrearRect(raiz, "AreaHandle");
        Estirar(areaHandle, 0f, 0f, 10f, 10f);

        RectTransform handle = CrearRect(areaHandle, "Handle");
        handle.sizeDelta = new Vector2(20f, 32f);
        Image imagenHandle = handle.gameObject.AddComponent<Image>();
        imagenHandle.color = new Color(0.85f, 0.85f, 0.9f, 1f);

        slider.fillRect = relleno;
        slider.handleRect = handle;
        slider.targetGraphic = imagenHandle;
        slider.direction = Slider.Direction.LeftToRight;

        if (alCambiar != null)
            slider.onValueChanged.AddListener(alCambiar);

        return slider;
    }

    public static void Anclar(RectTransform rect, Vector2 ancla, Vector2 pivote, Vector2 posicion, Vector2 tamano)
    {
        rect.anchorMin = ancla;
        rect.anchorMax = ancla;
        rect.pivot = pivote;
        rect.anchoredPosition = posicion;
        rect.sizeDelta = tamano;
    }

    public static void Estirar(RectTransform rect, float izquierda, float derecha, float abajo, float arriba)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(izquierda, abajo);
        rect.offsetMax = new Vector2(-derecha, -arriba);
    }
}
