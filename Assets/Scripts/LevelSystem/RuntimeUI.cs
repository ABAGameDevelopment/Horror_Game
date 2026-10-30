using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public static class RuntimeUI
{
    // Paleta de terror estilo Silent Hill 2 (coincidente con el Menú Principal)
    public static readonly Color ColorTitulo = new Color(0.95f, 0.95f, 0.95f, 1f);
    public static readonly Color ColorRojoSangre = new Color(0.78f, 0.12f, 0.12f, 1f);
    public static readonly Color ColorRojoBrillante = new Color(0.95f, 0.18f, 0.18f, 1f);
    public static readonly Color ColorTexto = new Color(0.92f, 0.92f, 0.92f, 1f);
    public static readonly Color ColorTextoTenue = new Color(0.65f, 0.65f, 0.65f, 1f);
    public static readonly Color ColorPanel = new Color(0.035f, 0.035f, 0.035f, 0.94f);
    public static readonly Color ColorPanelHud = new Color(0.02f, 0.02f, 0.02f, 0.88f);
    public static readonly Color ColorBoton = new Color(0.06f, 0.06f, 0.06f, 0.92f);
    public static readonly Color ColorBotonHover = new Color(0.50f, 0.08f, 0.08f, 0.95f);
    public static readonly Color ColorBotonPresion = new Color(0.25f, 0.0f, 0.0f, 1f);
    public static readonly Color ColorExito = new Color(0.35f, 0.82f, 0.35f, 1f);
    public static readonly Color ColorAviso = new Color(0.95f, 0.75f, 0.22f, 1f);
    public static readonly Color ColorBordeRojo = new Color(0.60f, 0.10f, 0.10f, 0.85f);

    private static TMP_FontAsset fuenteTexto;
    private static TMP_FontAsset fuenteTitulo;

    public static TMP_FontAsset FuenteTexto
    {
        get
        {
            if (fuenteTexto == null)
            {
#if UNITY_EDITOR
                fuenteTexto = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Examples & Extras/Resources/Fonts & Materials/Oswald Bold SDF.asset");
#endif
                if (fuenteTexto == null)
                    fuenteTexto = Resources.Load<TMP_FontAsset>("Fonts & Materials/Oswald Bold SDF");
                if (fuenteTexto == null)
                    fuenteTexto = FuenteTitulo;
            }

            return fuenteTexto;
        }
    }

    public static TMP_FontAsset FuenteTitulo
    {
        get
        {
            if (fuenteTitulo == null)
            {
#if UNITY_EDITOR
                fuenteTitulo = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Examples & Extras/Resources/Fonts & Materials/Anton SDF.asset");
#endif
                if (fuenteTitulo == null)
                    fuenteTitulo = Resources.Load<TMP_FontAsset>("Fonts & Materials/Anton SDF");
                if (fuenteTitulo == null)
                    fuenteTitulo = TMP_Settings.defaultFontAsset;
                if (fuenteTitulo == null)
                    fuenteTitulo = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            }

            return fuenteTitulo;
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

    public static Image CrearPanelMarcoTerror(Transform padre, string nombre, Vector2 tamano, Color colorFondo)
    {
        RectTransform rect = CrearRect(padre, nombre);
        rect.sizeDelta = tamano;

        Image panel = rect.gameObject.AddComponent<Image>();
        panel.color = colorFondo;

        // Borde superior sangriento
        Image bordeSup = CrearPanel(rect, "Borde_Superior", ColorBordeRojo);
        bordeSup.raycastTarget = false;
        Anclar(bordeSup.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), Vector2.zero, new Vector2(tamano.x, 3f));
        Estirar(bordeSup.rectTransform, 0f, 0f, tamano.y - 3f, 0f);

        // Borde inferior sangriento
        Image bordeInf = CrearPanel(rect, "Borde_Inferior", ColorBordeRojo);
        bordeInf.raycastTarget = false;
        Anclar(bordeInf.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), Vector2.zero, new Vector2(tamano.x, 3f));
        Estirar(bordeInf.rectTransform, 0f, 0f, 0f, tamano.y - 3f);

        return panel;
    }

    public static TextMeshProUGUI CrearTexto(Transform padre, string nombre, string texto, float tamano, TextAlignmentOptions alineacion, Color color)
    {
        RectTransform rect = CrearRect(padre, nombre);
        TextMeshProUGUI tmp = rect.gameObject.AddComponent<TextMeshProUGUI>();
        if (FuenteTexto != null)
            tmp.font = FuenteTexto;

        tmp.text = texto;
        tmp.fontSize = tamano;
        tmp.alignment = alineacion;
        tmp.color = color;
        tmp.raycastTarget = false;
        return tmp;
    }

    public static TextMeshProUGUI CrearTitulo(Transform padre, string nombre, string texto, float tamano, TextAlignmentOptions alineacion, Color color)
    {
        RectTransform rect = CrearRect(padre, nombre);
        TextMeshProUGUI tmp = rect.gameObject.AddComponent<TextMeshProUGUI>();
        if (FuenteTitulo != null)
            tmp.font = FuenteTitulo;

        tmp.text = texto;
        tmp.fontSize = tamano;
        tmp.fontStyle = FontStyles.Bold;
        tmp.characterSpacing = 8f;
        tmp.alignment = alineacion;
        tmp.color = color;
        tmp.raycastTarget = false;
        return tmp;
    }

    public static TextMeshProUGUI CrearTituloHorror(Transform padre, string nombre, string texto, float tamano, Color color)
    {
        TextMeshProUGUI tmp = CrearTitulo(padre, nombre, texto, tamano, TextAlignmentOptions.Center, color);
        tmp.gameObject.AddComponent<GlitchTitulo>();
        return tmp;
    }

    public static TextMeshProUGUI CrearSubtituloHorror(Transform padre, string nombre, string texto)
    {
        RectTransform rect = CrearRect(padre, nombre);
        TextMeshProUGUI tmp = rect.gameObject.AddComponent<TextMeshProUGUI>();
        if (FuenteTexto != null)
            tmp.font = FuenteTexto;

        tmp.text = texto;
        tmp.fontSize = 22f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.characterSpacing = 10f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = ColorRojoSangre;
        tmp.raycastTarget = false;
        return tmp;
    }

    public static VignetaHorror CrearVigneta(Transform padre, float intensidad = 0.45f)
    {
        Image imagen = CrearPanel(padre, "VignetaHorror", new Color(0f, 0f, 0f, intensidad));
        imagen.raycastTarget = false;
        Estirar(imagen.rectTransform, 0f, 0f, 0f, 0f);
        VignetaHorror vigneta = imagen.gameObject.AddComponent<VignetaHorror>();
        vigneta.Configurar(intensidad);
        return vigneta;
    }

    public static Button CrearBotonHorror(Transform padre, string nombre, string texto, UnityEngine.Events.UnityAction accion)
    {
        RectTransform rect = CrearRect(padre, nombre);

        Image imagen = rect.gameObject.AddComponent<Image>();
        imagen.color = ColorBoton;

        Button boton = rect.gameObject.AddComponent<Button>();
        boton.targetGraphic = imagen;

        ColorBlock colores = boton.colors;
        colores.normalColor = ColorBoton;
        colores.highlightedColor = ColorBotonHover;
        colores.pressedColor = ColorBotonPresion;
        colores.selectedColor = ColorBotonHover;
        colores.fadeDuration = 0.08f;
        boton.colors = colores;

        if (accion != null)
            boton.onClick.AddListener(accion);

        TextMeshProUGUI etiqueta = CrearTexto(rect, "Texto", texto, 24f, TextAlignmentOptions.Center, ColorTexto);
        etiqueta.fontStyle = FontStyles.Bold;
        etiqueta.characterSpacing = 4f;
        Estirar(etiqueta.rectTransform, 8f, 8f, 4f, 4f);

        return boton;
    }

    public static Slider CrearSliderHorror(Transform padre, string nombre, float valor, UnityEngine.Events.UnityAction<float> alCambiar)
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
        imagenFondo.color = new Color(0.12f, 0.12f, 0.12f, 0.95f);

        RectTransform areaRelleno = CrearRect(raiz, "AreaRelleno");
        Estirar(areaRelleno, 0f, 0f, 10f, 10f);

        RectTransform relleno = CrearRect(areaRelleno, "Relleno");
        Estirar(relleno, 0f, 0f, 0f, 0f);
        Image imagenRelleno = relleno.gameObject.AddComponent<Image>();
        imagenRelleno.color = ColorRojoSangre;

        RectTransform areaHandle = CrearRect(raiz, "AreaHandle");
        Estirar(areaHandle, 0f, 0f, 10f, 10f);

        RectTransform handle = CrearRect(areaHandle, "Handle");
        handle.sizeDelta = new Vector2(22f, 34f);
        Image imagenHandle = handle.gameObject.AddComponent<Image>();
        imagenHandle.color = new Color(0.92f, 0.92f, 0.92f, 1f);

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
