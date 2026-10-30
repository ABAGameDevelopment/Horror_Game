#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class GeneradorMenuSilentHill : EditorWindow
{
    private const string PATH_FONT_ANTON = "Assets/TextMesh Pro/Examples & Extras/Resources/Fonts & Materials/Anton SDF.asset";
    private const string PATH_FONT_OSWALD = "Assets/TextMesh Pro/Examples & Extras/Resources/Fonts & Materials/Oswald Bold SDF.asset";

    [MenuItem("Tools/Horror Game/Generar Menú Silent Hill 2 Automático")]
    public static void GenerarMenuCompleto()
    {
        // 1. Cargar fuentes de terror
        TMP_FontAsset fontTitulo = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(PATH_FONT_ANTON);
        TMP_FontAsset fontBotones = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(PATH_FONT_OSWALD);

        // 2. Limpieza de elementos viejos de gameplay
        EliminarSiExiste("Player");
        EliminarSiExiste("CanvasObjetivos");
        EliminarSiExiste("GestorObjetivos");

        // 3. Configurar la Cámara en las coordenadas exactas del pasillo
        Camera cam = ConfigurarCamara();

        // 4. Configurar la Linterna (Spotlight)
        Light linterna = ConfigurarLinterna(cam.transform);

        // 5. Configurar EventSystem para New Input System
        ConfigurarEventSystem();

        // 6. Configurar GameManager
        ConfigurarGameManager();

        // 7. Construir la UI Cinemática de Silent Hill
        GameObject canvasGO = ConstruirUICinematica(cam, fontTitulo, fontBotones);

        // 8. Configurar MenuManager con efectos de terror y Splash
        ConfigurarMenuManager(cam.transform, linterna, canvasGO);

        // 9. Asegurar Build Settings (0: MainMenu, 1: University)
        ConfigurarBuildSettings();

        // Marcar escena como modificada y guardar
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        EditorUtility.DisplayDialog("¡Menú Silent Hill 2 Completo!",
            "El menú ha sido actualizado y configurado al 100%:\n\n" +
            "1. Splash Inicial: Logo/Nombre del equipo y título cinematográfico.\n" +
            "2. Menú Principal: Jugar, Continuar, Opciones, Créditos, Salir.\n" +
            "3. Opciones completas: Sliders de Música y Efectos, Dropdown de Calidad y Guía de Controles.\n" +
            "4. Botón < VOLVER 100% funcional y compatible con tecla ESCAPE.\n" +
            "5. Tipografía de terror Anton & Oswald (extra grande e imponente).\n" +
            "6. Cámara y linterna alineadas al pasillo (X: 345, Z: 272).\n\n" +
            "¡Guarda con Cmd + S y dale a Play!", "¡Excelente!");
    }

    private static void EliminarSiExiste(string nombre)
    {
        GameObject go = GameObject.Find(nombre);
        if (go != null)
        {
            Undo.DestroyObjectImmediate(go);
        }
    }

    private static Camera ConfigurarCamara()
    {
        GameObject camGO = GameObject.Find("CameraMenu");
        if (camGO == null) camGO = GameObject.Find("Camera");
        if (camGO == null)
        {
            camGO = new GameObject("CameraMenu");
            Undo.RegisterCreatedObjectUndo(camGO, "Crear CameraMenu");
        }
        else
        {
            camGO.name = "CameraMenu";
        }

        // Coordenadas reales exactas del pasillo de la universidad
        camGO.transform.position = new Vector3(345.18f, 1.6f, 272.41f);
        camGO.transform.rotation = Quaternion.Euler(0f, -15.28f, 0f);

        Camera cam = camGO.GetComponent<Camera>();
        if (cam == null) cam = camGO.AddComponent<Camera>();

        cam.clearFlags = CameraClearFlags.Skybox;
        cam.backgroundColor = Color.black;
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 1000f;
        cam.fieldOfView = 60f;

        var urpCam = camGO.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
        if (urpCam != null)
        {
            urpCam.renderPostProcessing = true;
        }

        if (camGO.GetComponent<AudioListener>() == null)
        {
            camGO.AddComponent<AudioListener>();
        }

        return cam;
    }

    private static Light ConfigurarLinterna(Transform parent)
    {
        Transform childLight = parent.Find("Spotlight");
        if (childLight == null) childLight = parent.Find("Spot Light");

        GameObject lightGO;
        if (childLight == null)
        {
            lightGO = new GameObject("Spotlight");
            lightGO.transform.SetParent(parent, false);
            Undo.RegisterCreatedObjectUndo(lightGO, "Crear Spotlight Linterna");
        }
        else
        {
            lightGO = childLight.gameObject;
            lightGO.name = "Spotlight";
        }

        lightGO.transform.localPosition = new Vector3(0.2f, -0.1f, 0.1f);
        lightGO.transform.localRotation = Quaternion.identity;

        Light light = lightGO.GetComponent<Light>();
        if (light == null) light = lightGO.AddComponent<Light>();

        light.type = LightType.Spot;
        light.range = 28f;
        light.spotAngle = 54f;
        light.innerSpotAngle = 26f;
        light.color = new Color(1f, 0.94f, 0.82f); // blanco cálido linterna
        light.intensity = 4.2f;
        light.shadows = LightShadows.Soft;

        return light;
    }

    private static void ConfigurarEventSystem()
    {
        var es = Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
        if (es == null)
        {
            GameObject esGO = new GameObject("EventSystem");
            es = esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGO.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            Undo.RegisterCreatedObjectUndo(esGO, "Crear EventSystem");
        }
        else
        {
            var oldModule = es.GetComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            if (oldModule != null) DestroyImmediate(oldModule);

            if (es.GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>() == null)
            {
                es.gameObject.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            }
        }
    }

    private static void ConfigurarGameManager()
    {
        if (Object.FindFirstObjectByType<GameManager>() == null)
        {
            GameObject gmGO = new GameObject("GameManager");
            gmGO.AddComponent<GameManager>();
            Undo.RegisterCreatedObjectUndo(gmGO, "Crear GameManager");
        }
    }

    private static GameObject ConstruirUICinematica(Camera cam, TMP_FontAsset fontTitulo, TMP_FontAsset fontBotones)
    {
        EliminarSiExiste("CanvasMenu");

        GameObject canvasGO = new GameObject("CanvasMenu");
        Undo.RegisterCreatedObjectUndo(canvasGO, "Crear CanvasMenu");

        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();
        MainMenuController menuCtrl = canvasGO.AddComponent<MainMenuController>();

        // 1. Viñeta oscura de terror en los bordes
        GameObject vineta = new GameObject("VignetaHorror");
        vineta.transform.SetParent(canvasGO.transform, false);
        RectTransform rtVin = vineta.AddComponent<RectTransform>();
        rtVin.anchorMin = Vector2.zero;
        rtVin.anchorMax = Vector2.one;
        rtVin.sizeDelta = Vector2.zero;
        Image imgVin = vineta.AddComponent<Image>();
        imgVin.color = new Color(0f, 0f, 0f, 0.45f);
        imgVin.raycastTarget = false;
        vineta.AddComponent<VignetaHorror>();

        // 2. Overlay negro para transiciones y Splash
        GameObject overlay = new GameObject("OverlayNegro");
        overlay.transform.SetParent(canvasGO.transform, false);
        RectTransform rtOver = overlay.AddComponent<RectTransform>();
        rtOver.anchorMin = Vector2.zero;
        rtOver.anchorMax = Vector2.one;
        rtOver.sizeDelta = Vector2.zero;
        Image imgOver = overlay.AddComponent<Image>();
        imgOver.color = Color.black;
        CanvasGroup cgOver = overlay.AddComponent<CanvasGroup>();
        cgOver.alpha = 0f;
        cgOver.blocksRaycasts = false;

        // 3. Splash Screen Inicial (Logo de equipo y título)
        GameObject splashGO = new GameObject("PanelSplash");
        splashGO.transform.SetParent(canvasGO.transform, false);
        RectTransform rtSplash = splashGO.AddComponent<RectTransform>();
        rtSplash.anchorMin = Vector2.zero;
        rtSplash.anchorMax = Vector2.one;
        rtSplash.sizeDelta = Vector2.zero;
        CanvasGroup cgSplash = splashGO.AddComponent<CanvasGroup>();
        cgSplash.alpha = 0f;
        cgSplash.blocksRaycasts = false;

        GameObject splashTxtGO = new GameObject("TextoLogoEquipo");
        splashTxtGO.transform.SetParent(splashGO.transform, false);
        RectTransform rtSplashTxt = splashTxtGO.AddComponent<RectTransform>();
        rtSplashTxt.anchoredPosition = Vector2.zero;
        rtSplashTxt.sizeDelta = new Vector2(1200f, 200f);
        TextMeshProUGUI txtSplash = splashTxtGO.AddComponent<TextMeshProUGUI>();
        if (fontTitulo != null) txtSplash.font = fontTitulo;
        txtSplash.text = "HORROR STUDIOS\n<size=32><color=#AA3333>P R E S E N T A</color></size>";
        txtSplash.fontSize = 65;
        txtSplash.alignment = TextAlignmentOptions.Center;
        txtSplash.color = Color.white;

        // 4. Panel Principal
        GameObject pnlPrincipal = new GameObject("PanelPrincipal");
        pnlPrincipal.transform.SetParent(canvasGO.transform, false);
        RectTransform rtPrin = pnlPrincipal.AddComponent<RectTransform>();
        rtPrin.anchorMin = Vector2.zero;
        rtPrin.anchorMax = Vector2.one;
        rtPrin.sizeDelta = Vector2.zero;
        CanvasGroup cgPrin = pnlPrincipal.AddComponent<CanvasGroup>();

        // TÍTULO DEL JUEGO (Estilo Silent Hill: Imponente, grande y sangriento)
        GameObject tituloGO = new GameObject("TextoTitulo");
        tituloGO.transform.SetParent(pnlPrincipal.transform, false);
        RectTransform rtTit = tituloGO.AddComponent<RectTransform>();
        rtTit.anchorMin = new Vector2(0.5f, 0.72f);
        rtTit.anchorMax = new Vector2(0.5f, 0.72f);
        rtTit.anchoredPosition = new Vector2(0f, 40f);
        rtTit.sizeDelta = new Vector2(1400f, 160f);

        TextMeshProUGUI txtTit = tituloGO.AddComponent<TextMeshProUGUI>();
        if (fontTitulo != null) txtTit.font = fontTitulo;
        txtTit.text = "UNIVERSIDAD MALDITA";
        txtTit.fontSize = 94; // ¡Mucho más grande!
        txtTit.fontStyle = FontStyles.Bold;
        txtTit.alignment = TextAlignmentOptions.Center;
        txtTit.characterSpacing = 8;
        txtTit.color = new Color(0.95f, 0.95f, 0.95f, 1f);
        tituloGO.AddComponent<GlitchTitulo>();

        // SUBTÍTULO
        GameObject subGO = new GameObject("TextoSubtitulo");
        subGO.transform.SetParent(pnlPrincipal.transform, false);
        RectTransform rtSub = subGO.AddComponent<RectTransform>();
        rtSub.anchorMin = new Vector2(0.5f, 0.72f);
        rtSub.anchorMax = new Vector2(0.5f, 0.72f);
        rtSub.anchoredPosition = new Vector2(0f, -45f);
        rtSub.sizeDelta = new Vector2(800f, 50f);

        TextMeshProUGUI txtSub = subGO.AddComponent<TextMeshProUGUI>();
        if (fontBotones != null) txtSub.font = fontBotones;
        txtSub.text = "—   N  O     H  A  Y     S  A  L  I  D  A   —";
        txtSub.fontSize = 24;
        txtSub.characterSpacing = 12;
        txtSub.alignment = TextAlignmentOptions.Center;
        txtSub.color = new Color(0.78f, 0.12f, 0.12f, 0.95f); // Rojo sangre

        // CONTENEDOR DE BOTONES
        GameObject pnlBotones = new GameObject("ContenedorBotones");
        pnlBotones.transform.SetParent(pnlPrincipal.transform, false);
        RectTransform rtBtns = pnlBotones.AddComponent<RectTransform>();
        rtBtns.anchorMin = new Vector2(0.5f, 0.35f);
        rtBtns.anchorMax = new Vector2(0.5f, 0.35f);
        rtBtns.anchoredPosition = new Vector2(0f, -30f);
        rtBtns.sizeDelta = new Vector2(360f, 320f);

        VerticalLayoutGroup layout = pnlBotones.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 16;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        Button btnJugar = CrearBoton(pnlBotones.transform, "BotonJugar", "NUEVA PARTIDA", fontBotones);
        Button btnContinuar = CrearBoton(pnlBotones.transform, "BotonContinuar", "CONTINUAR", fontBotones);
        Button btnOpciones = CrearBoton(pnlBotones.transform, "BotonOpciones", "OPCIONES", fontBotones);
        Button btnCreditos = CrearBoton(pnlBotones.transform, "BotonCreditos", "CRÉDITOS", fontBotones);
        Button btnSalir = CrearBoton(pnlBotones.transform, "BotonSalir", "SALIR", fontBotones);

        // 5. Panel de Opciones (Completo con Sliders, Dropdown y Controles)
        Slider sMusica, sEfectos;
        TMP_Dropdown ddCalidad;
        TMP_Text txtMus, txtEf;
        Button btnVolverOpc;
        GameObject pnlOpciones = CrearPanelOpcionesCompleto(canvasGO.transform, fontTitulo, fontBotones,
            out sMusica, out sEfectos, out ddCalidad, out txtMus, out txtEf, out btnVolverOpc);
        CanvasGroup cgOpc = pnlOpciones.GetComponent<CanvasGroup>();

        // 6. Panel de Créditos
        Button btnVolverCred;
        GameObject pnlCreditos = CrearPanelCreditosCompleto(canvasGO.transform, fontTitulo, fontBotones, out btnVolverCred);
        CanvasGroup cgCred = pnlCreditos.GetComponent<CanvasGroup>();

        // Serializar referencias en MainMenuController
        SerializedObject so = new SerializedObject(menuCtrl);
        so.FindProperty("panelPrincipal").objectReferenceValue = cgPrin;
        so.FindProperty("panelOpciones").objectReferenceValue = cgOpc;
        so.FindProperty("panelCreditos").objectReferenceValue = cgCred;
        so.FindProperty("panelOverlayNegro").objectReferenceValue = cgOver;

        so.FindProperty("botonJugar").objectReferenceValue = btnJugar;
        so.FindProperty("botonContinuar").objectReferenceValue = btnContinuar;
        so.FindProperty("botonOpciones").objectReferenceValue = btnOpciones;
        so.FindProperty("botonCreditos").objectReferenceValue = btnCreditos;
        so.FindProperty("botonSalir").objectReferenceValue = btnSalir;

        so.FindProperty("sliderMusica").objectReferenceValue = sMusica;
        so.FindProperty("sliderEfectos").objectReferenceValue = sEfectos;
        so.FindProperty("dropdownCalidad").objectReferenceValue = ddCalidad;
        so.FindProperty("textoMusica").objectReferenceValue = txtMus;
        so.FindProperty("textoEfectos").objectReferenceValue = txtEf;
        so.FindProperty("botonVolverOpciones").objectReferenceValue = btnVolverOpc;
        so.FindProperty("botonVolverCreditos").objectReferenceValue = btnVolverCred;

        so.FindProperty("tituloRect").objectReferenceValue = rtTit;
        so.ApplyModifiedProperties();

        return canvasGO;
    }

    private static Button CrearBoton(Transform parent, string nombre, string texto, TMP_FontAsset font)
    {
        GameObject btnGO = new GameObject(nombre);
        btnGO.transform.SetParent(parent, false);

        RectTransform rt = btnGO.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(340f, 50f);

        Image img = btnGO.AddComponent<Image>();
        img.color = new Color(0.06f, 0.06f, 0.06f, 0.85f);

        Button btn = btnGO.AddComponent<Button>();
        btn.targetGraphic = img;

        ColorBlock cb = btn.colors;
        cb.normalColor = new Color(0.08f, 0.08f, 0.08f, 0.85f);
        cb.highlightedColor = new Color(0.5f, 0.08f, 0.08f, 0.95f); // Rojo sangre en hover
        cb.pressedColor = new Color(0.25f, 0.0f, 0.0f, 1f);
        cb.selectedColor = cb.highlightedColor;
        btn.colors = cb;

        GameObject txtGO = new GameObject("Text");
        txtGO.transform.SetParent(btnGO.transform, false);

        RectTransform rtTxt = txtGO.AddComponent<RectTransform>();
        rtTxt.anchorMin = Vector2.zero;
        rtTxt.anchorMax = Vector2.one;
        rtTxt.sizeDelta = Vector2.zero;

        TextMeshProUGUI tmp = txtGO.AddComponent<TextMeshProUGUI>();
        if (font != null) tmp.font = font;
        tmp.text = texto;
        tmp.fontSize = 24;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(0.92f, 0.92f, 0.92f);
        tmp.characterSpacing = 4;

        return btn;
    }

    private static GameObject CrearPanelOpcionesCompleto(Transform parent, TMP_FontAsset fontTit, TMP_FontAsset fontTxt,
        out Slider sMusica, out Slider sEfectos, out TMP_Dropdown ddCalidad, out TMP_Text txtMus, out TMP_Text txtEf, out Button btnVolver)
    {
        GameObject pnl = new GameObject("PanelOpciones");
        pnl.transform.SetParent(parent, false);

        RectTransform rt = pnl.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(750f, 620f);

        Image bg = pnl.AddComponent<Image>();
        bg.color = new Color(0.03f, 0.03f, 0.03f, 0.94f);

        CanvasGroup cg = pnl.AddComponent<CanvasGroup>();
        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;

        // Título Opciones
        GameObject titGO = new GameObject("TituloOpciones");
        titGO.transform.SetParent(pnl.transform, false);
        RectTransform rtTit = titGO.AddComponent<RectTransform>();
        rtTit.anchoredPosition = new Vector2(0f, 260f);
        rtTit.sizeDelta = new Vector2(500f, 50f);
        var txtTit = titGO.AddComponent<TextMeshProUGUI>();
        if (fontTit != null) txtTit.font = fontTit;
        txtTit.text = "—   O P C I O N E S   —";
        txtTit.fontSize = 36;
        txtTit.alignment = TextAlignmentOptions.Center;
        txtTit.color = new Color(0.85f, 0.18f, 0.18f);

        // 1. Slider Música
        CrearFilaSlider(pnl.transform, "FilaMusica", "MÚSICA", 190f, fontTxt, out sMusica, out txtMus);

        // 2. Slider Efectos
        CrearFilaSlider(pnl.transform, "FilaEfectos", "EFECTOS", 130f, fontTxt, out sEfectos, out txtEf);

        // 3. Dropdown Calidad
        GameObject rowCal = new GameObject("FilaCalidad");
        rowCal.transform.SetParent(pnl.transform, false);
        RectTransform rtCal = rowCal.AddComponent<RectTransform>();
        rtCal.anchoredPosition = new Vector2(0f, 70f);
        rtCal.sizeDelta = new Vector2(620f, 40f);

        GameObject lblCal = new GameObject("Label");
        lblCal.transform.SetParent(rowCal.transform, false);
        RectTransform rtLblCal = lblCal.AddComponent<RectTransform>();
        rtLblCal.anchorMin = new Vector2(0f, 0.5f);
        rtLblCal.anchorMax = new Vector2(0f, 0.5f);
        rtLblCal.anchoredPosition = new Vector2(100f, 0f);
        rtLblCal.sizeDelta = new Vector2(200f, 30f);
        var txtLblCal = lblCal.AddComponent<TextMeshProUGUI>();
        if (fontTxt != null) txtLblCal.font = fontTxt;
        txtLblCal.text = "CALIDAD GRÁFICA";
        txtLblCal.fontSize = 20;
        txtLblCal.color = Color.white;

        GameObject ddGO = new GameObject("DropdownCalidad");
        ddGO.transform.SetParent(rowCal.transform, false);
        RectTransform rtDD = ddGO.AddComponent<RectTransform>();
        rtDD.anchorMin = new Vector2(1f, 0.5f);
        rtDD.anchorMax = new Vector2(1f, 0.5f);
        rtDD.anchoredPosition = new Vector2(-120f, 0f);
        rtDD.sizeDelta = new Vector2(200f, 34f);

        Image imgDD = ddGO.AddComponent<Image>();
        imgDD.color = new Color(0.12f, 0.12f, 0.12f, 0.9f);
        ddCalidad = ddGO.AddComponent<TMP_Dropdown>();
        ddCalidad.targetGraphic = imgDD;
        ddCalidad.options.Clear();
        ddCalidad.options.Add(new TMP_Dropdown.OptionData("Baja"));
        ddCalidad.options.Add(new TMP_Dropdown.OptionData("Media"));
        ddCalidad.options.Add(new TMP_Dropdown.OptionData("Alta"));
        ddCalidad.options.Add(new TMP_Dropdown.OptionData("Ultra"));

        GameObject captionGO = new GameObject("Label");
        captionGO.transform.SetParent(ddGO.transform, false);
        RectTransform rtCap = captionGO.AddComponent<RectTransform>();
        rtCap.anchorMin = Vector2.zero;
        rtCap.anchorMax = Vector2.one;
        rtCap.sizeDelta = Vector2.zero;
        var txtCap = captionGO.AddComponent<TextMeshProUGUI>();
        if (fontTxt != null) txtCap.font = fontTxt;
        txtCap.text = "Alta";
        txtCap.fontSize = 18;
        txtCap.alignment = TextAlignmentOptions.Center;
        txtCap.color = Color.white;
        ddCalidad.captionText = txtCap;

        // 4. Guía de Controles
        GameObject ctrlGO = new GameObject("TextoControles");
        ctrlGO.transform.SetParent(pnl.transform, false);
        RectTransform rtCtrl = ctrlGO.AddComponent<RectTransform>();
        rtCtrl.anchoredPosition = new Vector2(0f, -80f);
        rtCtrl.sizeDelta = new Vector2(620f, 180f);
        var txtCtrl = ctrlGO.AddComponent<TextMeshProUGUI>();
        if (fontTxt != null) txtCtrl.font = fontTxt;
        txtCtrl.text = "<b><color=#AA2222>C O N T R O L E S</color></b>\n\n" +
                       "<color=#FF5555>W A S D</color> : Desplazarse por el edificio\n" +
                       "<color=#FF5555>RATÓN</color> : Mirar alrededor\n" +
                       "<color=#FF5555>ESPACIO</color> : Saltar\n" +
                       "<color=#FF5555>F</color> : Linterna (Encender / Apagar)\n\n" +
                       "<color=#777777>Objetivo: Encuentra las llaves antes de que sea tarde.</color>";
        txtCtrl.fontSize = 19;
        txtCtrl.lineSpacing = 18;
        txtCtrl.alignment = TextAlignmentOptions.Center;
        txtCtrl.color = new Color(0.85f, 0.85f, 0.85f);

        // 5. Botón VOLVER (100% interactivo)
        btnVolver = CrearBoton(pnl.transform, "BotonVolverOpciones", "< VOLVER", fontTxt);
        RectTransform rtV = btnVolver.GetComponent<RectTransform>();
        rtV.anchoredPosition = new Vector2(0f, -245f);
        rtV.sizeDelta = new Vector2(260f, 46f);

        return pnl;
    }

    private static void CrearFilaSlider(Transform parent, string nombre, string label, float posY, TMP_FontAsset font,
        out Slider slider, out TMP_Text textValue)
    {
        GameObject row = new GameObject(nombre);
        row.transform.SetParent(parent, false);
        RectTransform rtRow = row.AddComponent<RectTransform>();
        rtRow.anchoredPosition = new Vector2(0f, posY);
        rtRow.sizeDelta = new Vector2(620f, 40f);

        // Label
        GameObject lbl = new GameObject("Label");
        lbl.transform.SetParent(row.transform, false);
        RectTransform rtLbl = lbl.AddComponent<RectTransform>();
        rtLbl.anchorMin = new Vector2(0f, 0.5f);
        rtLbl.anchorMax = new Vector2(0f, 0.5f);
        rtLbl.anchoredPosition = new Vector2(90f, 0f);
        rtLbl.sizeDelta = new Vector2(180f, 30f);
        var txtLbl = lbl.AddComponent<TextMeshProUGUI>();
        if (font != null) txtLbl.font = font;
        txtLbl.text = label;
        txtLbl.fontSize = 20;
        txtLbl.color = Color.white;

        // Slider GO
        GameObject slGO = new GameObject("Slider");
        slGO.transform.SetParent(row.transform, false);
        RectTransform rtSl = slGO.AddComponent<RectTransform>();
        rtSl.anchorMin = new Vector2(0.5f, 0.5f);
        rtSl.anchorMax = new Vector2(0.5f, 0.5f);
        rtSl.anchoredPosition = new Vector2(30f, 0f);
        rtSl.sizeDelta = new Vector2(240f, 20f);

        slider = slGO.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0.8f;

        // Background
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(slGO.transform, false);
        RectTransform rtBg = bg.AddComponent<RectTransform>();
        rtBg.anchorMin = Vector2.zero;
        rtBg.anchorMax = Vector2.one;
        rtBg.sizeDelta = Vector2.zero;
        Image imgBg = bg.AddComponent<Image>();
        imgBg.color = new Color(0.15f, 0.15f, 0.15f, 0.8f);

        // Fill Area
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(slGO.transform, false);
        RectTransform rtFillArea = fillArea.AddComponent<RectTransform>();
        rtFillArea.anchorMin = Vector2.zero;
        rtFillArea.anchorMax = Vector2.one;
        rtFillArea.sizeDelta = Vector2.zero;

        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        RectTransform rtFill = fill.AddComponent<RectTransform>();
        rtFill.sizeDelta = Vector2.zero;
        Image imgFill = fill.AddComponent<Image>();
        imgFill.color = new Color(0.75f, 0.15f, 0.15f, 1f); // Barra roja horror
        slider.fillRect = rtFill;

        // Percentage Text
        GameObject valGO = new GameObject("TextoPorcentaje");
        valGO.transform.SetParent(row.transform, false);
        RectTransform rtVal = valGO.AddComponent<RectTransform>();
        rtVal.anchorMin = new Vector2(1f, 0.5f);
        rtVal.anchorMax = new Vector2(1f, 0.5f);
        rtVal.anchoredPosition = new Vector2(-45f, 0f);
        rtVal.sizeDelta = new Vector2(70f, 30f);
        textValue = valGO.AddComponent<TextMeshProUGUI>();
        if (font != null) textValue.font = font;
        textValue.text = "80%";
        textValue.fontSize = 20;
        textValue.alignment = TextAlignmentOptions.Center;
        textValue.color = new Color(0.85f, 0.85f, 0.85f);
    }

    private static GameObject CrearPanelCreditosCompleto(Transform parent, TMP_FontAsset fontTit, TMP_FontAsset fontTxt, out Button btnVolver)
    {
        GameObject pnl = new GameObject("PanelCreditos");
        pnl.transform.SetParent(parent, false);

        RectTransform rt = pnl.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(750f, 620f);

        Image bg = pnl.AddComponent<Image>();
        bg.color = new Color(0.03f, 0.03f, 0.03f, 0.94f);

        CanvasGroup cg = pnl.AddComponent<CanvasGroup>();
        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;

        // Título Créditos
        GameObject titGO = new GameObject("TituloCreditos");
        titGO.transform.SetParent(pnl.transform, false);
        RectTransform rtTit = titGO.AddComponent<RectTransform>();
        rtTit.anchoredPosition = new Vector2(0f, 260f);
        rtTit.sizeDelta = new Vector2(500f, 50f);
        var txtTit = titGO.AddComponent<TextMeshProUGUI>();
        if (fontTit != null) txtTit.font = fontTit;
        txtTit.text = "—   C R É D I T O S   —";
        txtTit.fontSize = 36;
        txtTit.alignment = TextAlignmentOptions.Center;
        txtTit.color = new Color(0.85f, 0.18f, 0.18f);

        // Texto Créditos
        GameObject txtGO = new GameObject("TextoCreditos");
        txtGO.transform.SetParent(pnl.transform, false);
        RectTransform rtCred = txtGO.AddComponent<RectTransform>();
        rtCred.anchoredPosition = new Vector2(0f, 10f);
        rtCred.sizeDelta = new Vector2(620f, 360f);
        var txtCred = txtGO.AddComponent<TextMeshProUGUI>();
        if (fontTxt != null) txtCred.font = fontTxt;
        txtCred.text = "<b><size=30>UNIVERSIDAD MALDITA</size></b>\n\n" +
                       "<color=#888888>Un videojuego de terror psicológico en primera persona.</color>\n\n" +
                       "<b><color=#AA2222>EQUIPO DE DESARROLLO</color></b>\n" +
                       "Jose Armando Ruano Mascorro\n" +
                       "Cecilia Aurora Robelo Hernandez\n" +
                       "Bryan Emilio Arredondo López\n" +
                       "GRUPO GIDS6103 — Ingeniería en Software\n\n" +
                       "<b><color=#AA2222>TECNOLOGÍA</color></b>\n" +
                       "Motor Gráfico • Unity 6\n" +
                       "Pipeline • Universal Render Pipeline (URP)\n\n" +
                       "<color=#555555>Octubre 2026 — Proyecto Final</color>";
        txtCred.fontSize = 20;
        txtCred.lineSpacing = 16;
        txtCred.alignment = TextAlignmentOptions.Center;
        txtCred.color = new Color(0.88f, 0.88f, 0.88f);

        // Botón VOLVER
        btnVolver = CrearBoton(pnl.transform, "BotonVolverCreditos", "< VOLVER", fontTxt);
        RectTransform rtV = btnVolver.GetComponent<RectTransform>();
        rtV.anchoredPosition = new Vector2(0f, -245f);
        rtV.sizeDelta = new Vector2(260f, 46f);

        return pnl;
    }

    private static void ConfigurarMenuManager(Transform cam, Light linterna, GameObject canvasGO)
    {
        GameObject mmGO = GameObject.Find("MenuManager");
        if (mmGO == null)
        {
            mmGO = new GameObject("MenuManager");
            Undo.RegisterCreatedObjectUndo(mmGO, "Crear MenuManager");
        }

        HorrorMenuCinematic cinematic = mmGO.GetComponent<HorrorMenuCinematic>();
        if (cinematic == null) cinematic = mmGO.AddComponent<HorrorMenuCinematic>();

        CanvasGroup cgMenu = canvasGO.transform.Find("PanelPrincipal")?.GetComponent<CanvasGroup>();
        CanvasGroup cgOverlay = canvasGO.transform.Find("OverlayNegro")?.GetComponent<CanvasGroup>();
        CanvasGroup cgSplash = canvasGO.transform.Find("PanelSplash")?.GetComponent<CanvasGroup>();
        TMP_Text txtSplash = cgSplash?.GetComponentInChildren<TMP_Text>();

        SerializedObject so = new SerializedObject(cinematic);
        so.FindProperty("camaraTransform").objectReferenceValue = cam;
        so.FindProperty("linternaSpot").objectReferenceValue = linterna;
        so.FindProperty("grupoMenuUI").objectReferenceValue = cgMenu;
        so.FindProperty("overlayNegro").objectReferenceValue = cgOverlay;
        so.FindProperty("grupoSplash").objectReferenceValue = cgSplash;
        so.FindProperty("textoLogoEquipo").objectReferenceValue = txtSplash;
        so.FindProperty("omitirSplash").boolValue = false; // ¡Splash activo con el logo de tu equipo!
        so.ApplyModifiedProperties();
    }

    private static void ConfigurarBuildSettings()
    {
        string sceneMenu = "Assets/Scenes/MainMenu/MainMenu.unity";
        string sceneUniv = "Assets/Scenes/Level1/University.unity";

        var scenes = new List<EditorBuildSettingsScene>();
        scenes.Add(new EditorBuildSettingsScene(sceneMenu, true));
        scenes.Add(new EditorBuildSettingsScene(sceneUniv, true));

        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
#endif
