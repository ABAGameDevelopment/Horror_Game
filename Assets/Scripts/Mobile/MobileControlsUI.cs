using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public enum TipoControlMovimiento
{
    RuedaJoystick,
    FlechasDPad,
    Ambos
}

/// <summary>
/// Administrador de la Interfaz Táctil para Móviles (Virtual Joystick, Flechas y Botones de Acción).
/// Genera y coordina todos los controles móviles ergonómicos para el Nivel 1.
/// </summary>
public class MobileControlsUI : MonoBehaviour
{
    public static MobileControlsUI Instance { get; private set; }

    [Header("Modo de Movimiento")]
    [SerializeField] private TipoControlMovimiento modoInicial = TipoControlMovimiento.RuedaJoystick;
    [SerializeField] private bool recordarPreferencia = true;
    [SerializeField] private bool mantenerCursorLibreEnEditor = true;

    [Header("Referencias de Jugador")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private InteraccionJugador interaccionJugador;
    [SerializeField] private MenuPausa menuPausa;

    [Header("Configuración Visual")]
    [SerializeField] [Range(0.2f, 1f)] private float opacidadUI = 0.88f;
    [SerializeField] private Color colorPrimario = new Color(0.12f, 0.14f, 0.18f, 0.8f);
    [SerializeField] private Color colorAcento = new Color(0.25f, 0.65f, 1f, 0.95f);
    [SerializeField] private Color colorActivo = new Color(1f, 0.8f, 0.2f, 0.95f);

    // Componentes internos creados
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private GameObject panelMovimiento;
    private VirtualJoystick virtualJoystick;
    private VirtualDPad virtualDPad;
    private TouchLookZone touchLookZone;

    // Botones de acción
    private Image imagenBotonLinterna;
    private Image imagenBotonSprint;
    private Image imagenBotonInteractuar;
    private TMP_Text textoBotonModo;

    private TipoControlMovimiento modoActual;
    private bool sprintToggleActivo = false;

    private const string PREF_KEY_MODO = "HorrorGame_MobileControlMode";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (recordarPreferencia && PlayerPrefs.HasKey(PREF_KEY_MODO))
        {
            modoActual = (TipoControlMovimiento)PlayerPrefs.GetInt(PREF_KEY_MODO, (int)modoInicial);
        }
        else
        {
            modoActual = modoInicial;
        }

        AsegurarEventSystem();
        BuscarReferencias();
    }

    private void Start()
    {
        BuscarReferencias();
        ConstruirInterfaz();
        ActualizarModoVisual();
        DesbloquearCursor();
    }

    private void Update()
    {
        // En editor o pruebas, forzar que el ratón no se bloquee para poder agarrar el joystick
        if (mantenerCursorLibreEnEditor)
        {
            DesbloquearCursor();
        }

        if (playerController == null)
        {
            BuscarReferencias();
            if (playerController == null) return;
        }

        // Ocultar / atenuar en cinemática inicial o leyendo notas
        bool enPausaOCinematica = (NotasUI.Instance != null && NotasUI.Instance.MostrandoNota) ||
                                 (Time.timeScale < 0.01f);

        if (canvasGroup != null)
        {
            float targetAlpha = enPausaOCinematica ? 0f : opacidadUI;
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, Time.unscaledDeltaTime * 4f);
            canvasGroup.blocksRaycasts = !enPausaOCinematica;
        }

        // Procesar y enviar entrada de movimiento al PlayerController
        ActualizarEntradaMovimiento();

        // Actualizar feedback visual de linterna, interactuar y correr
        ActualizarFeedbackBotones();
    }

    private void DesbloquearCursor()
    {
        if (Cursor.lockState != CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.None;
        }
        if (!Cursor.visible)
        {
            Cursor.visible = true;
        }
    }

    private void AsegurarEventSystem()
    {
        if (FindAnyObjectByType<EventSystem>() == null)
        {
            GameObject esGO = new GameObject("EventSystem", typeof(EventSystem), typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));
            Debug.Log("[MobileControlsUI] EventSystem creado automáticamente.");
        }
    }

    private void BuscarReferencias()
    {
        if (playerController == null)
            playerController = FindAnyObjectByType<PlayerController>();

        if (interaccionJugador == null)
            interaccionJugador = FindAnyObjectByType<InteraccionJugador>();

        if (menuPausa == null)
            menuPausa = FindAnyObjectByType<MenuPausa>();

        if (touchLookZone != null && playerController != null)
            touchLookZone.ConfigurarJugador(playerController);
    }

    private void ActualizarEntradaMovimiento()
    {
        if (playerController == null) return;

        Vector2 moveVector = Vector2.zero;

        if (modoActual == TipoControlMovimiento.RuedaJoystick && virtualJoystick != null)
        {
            moveVector = virtualJoystick.InputDirection;
        }
        else if (modoActual == TipoControlMovimiento.FlechasDPad && virtualDPad != null)
        {
            moveVector = virtualDPad.InputDirection;
        }
        else if (modoActual == TipoControlMovimiento.Ambos)
        {
            Vector2 jInput = virtualJoystick != null ? virtualJoystick.InputDirection : Vector2.zero;
            Vector2 dInput = virtualDPad != null ? virtualDPad.InputDirection : Vector2.zero;

            moveVector = jInput.sqrMagnitude > 0.01f ? jInput : dInput;
        }

        playerController.SetVirtualMoveInput(moveVector);
    }

    private void ActualizarFeedbackBotones()
    {
        if (playerController != null && imagenBotonLinterna != null)
        {
            imagenBotonLinterna.color = playerController.LinternaEncendida ? colorActivo : colorPrimario;
        }

        if (imagenBotonSprint != null)
        {
            imagenBotonSprint.color = sprintToggleActivo ? new Color(0.2f, 0.9f, 0.4f, 0.9f) : colorPrimario;
        }

        if (interaccionJugador != null && imagenBotonInteractuar != null)
        {
            imagenBotonInteractuar.color = interaccionJugador.HayInteraccionDisponible ? colorAcento : colorPrimario;
        }
    }

    #region Construcción de la Interfaz Procedural

    private void ConstruirInterfaz()
    {
        // Canvas
        canvas = gameObject.GetComponent<Canvas>();
        if (canvas == null)
            canvas = gameObject.AddComponent<Canvas>();

        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 90;

        CanvasScaler scaler = gameObject.GetComponent<CanvasScaler>();
        if (scaler == null)
            scaler = gameObject.AddComponent<CanvasScaler>();

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        if (gameObject.GetComponent<GraphicRaycaster>() == null)
            gameObject.AddComponent<GraphicRaycaster>();

        canvasGroup = gameObject.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        canvasGroup.alpha = opacidadUI;

        // Sprites procedurales
        Sprite spriteCirculoBase = GenerarSpriteCirculo(256, new Color(0.08f, 0.1f, 0.14f, 0.7f), colorAcento, 6);
        Sprite spriteCirculoPomo = GenerarSpriteCirculo(128, new Color(0.25f, 0.65f, 1f, 0.9f), Color.white, 4);
        Sprite spriteBotonRedondo = GenerarSpriteCirculo(160, colorPrimario, colorAcento, 5);
        Sprite spriteFlecha = GenerarSpriteFlecha(128, Color.white);

        // 1. Zona táctil de cámara (únicamente la mitad derecha para no interferir con el joystick)
        CrearZonaGiroCamara();

        // 2. Panel de Movimiento (Esquina inferior izquierda)
        CrearPanelMovimiento(spriteCirculoBase, spriteCirculoPomo, spriteBotonRedondo, spriteFlecha);

        // 3. Botones de Acción (Esquina inferior derecha)
        CrearBotonesAccion(spriteBotonRedondo);

        // 4. Botón de Pausa (Esquina superior derecha)
        CrearBotonPausa(spriteBotonRedondo);
    }

    private void CrearZonaGiroCamara()
    {
        GameObject lookZoneGO = new GameObject("TouchLookZone", typeof(RectTransform), typeof(Image), typeof(TouchLookZone));
        lookZoneGO.transform.SetParent(transform, false);

        RectTransform rt = lookZoneGO.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.55f, 0f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image img = lookZoneGO.GetComponent<Image>();
        img.color = Color.clear;
        img.raycastTarget = true;

        touchLookZone = lookZoneGO.GetComponent<TouchLookZone>();
        touchLookZone.SensibilidadX = 0.16f;
        touchLookZone.SensibilidadY = 0.14f;

        if (playerController != null)
            touchLookZone.ConfigurarJugador(playerController);
    }

    private void CrearPanelMovimiento(Sprite spriteBase, Sprite spritePomo, Sprite spriteBoton, Sprite spriteFlecha)
    {
        panelMovimiento = new GameObject("Panel_Movimiento", typeof(RectTransform));
        panelMovimiento.transform.SetParent(transform, false);

        RectTransform panelRT = panelMovimiento.GetComponent<RectTransform>();
        panelRT.anchorMin = Vector2.zero;
        panelRT.anchorMax = Vector2.zero;
        panelRT.pivot = Vector2.zero;
        panelRT.anchoredPosition = new Vector2(40f, 40f);
        panelRT.sizeDelta = new Vector2(360f, 380f);

        // Botón selector de Modo: Rueda / Flechas / Ambos
        CrearBotonSelectorModo(panelMovimiento.transform, spriteBoton);

        // A) Joystick Virtual ("La Rueda")
        CrearJoystickVirtual(panelMovimiento.transform, spriteBase, spritePomo);

        // B) Cruceta D-Pad ("Las Flechas: Arriba, Abajo, Izquierda, Derecha")
        CrearDPadVirtual(panelMovimiento.transform, spriteBoton, spriteFlecha);
    }

    private void CrearBotonSelectorModo(Transform padre, Sprite spriteBoton)
    {
        GameObject botonModoGO = new GameObject("Boton_ModoControles", typeof(RectTransform), typeof(Image), typeof(Button));
        botonModoGO.transform.SetParent(padre, false);

        RectTransform rt = botonModoGO.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(20f, -10f);
        rt.sizeDelta = new Vector2(240f, 48f);

        Image img = botonModoGO.GetComponent<Image>();
        img.sprite = spriteBoton;
        img.type = Image.Type.Simple;
        img.color = new Color(0.1f, 0.12f, 0.16f, 0.9f);
        img.raycastTarget = true;

        Button btn = botonModoGO.GetComponent<Button>();
        btn.onClick.AddListener(AlternarModoControl);

        GameObject txtGO = new GameObject("Texto_Modo", typeof(RectTransform), typeof(TextMeshProUGUI));
        txtGO.transform.SetParent(botonModoGO.transform, false);
        RectTransform txtRT = txtGO.GetComponent<RectTransform>();
        txtRT.anchorMin = Vector2.zero;
        txtRT.anchorMax = Vector2.one;
        txtRT.offsetMin = Vector2.zero;
        txtRT.offsetMax = Vector2.zero;

        textoBotonModo = txtGO.GetComponent<TMP_Text>();
        textoBotonModo.alignment = TextAlignmentOptions.Center;
        textoBotonModo.fontSize = 17;
        textoBotonModo.fontStyle = FontStyles.Bold;
        textoBotonModo.color = Color.white;
        textoBotonModo.raycastTarget = false;
    }

    private void CrearJoystickVirtual(Transform padre, Sprite spriteBase, Sprite spritePomo)
    {
        GameObject joystickGO = new GameObject("VirtualJoystick", typeof(RectTransform), typeof(Image), typeof(VirtualJoystick));
        joystickGO.transform.SetParent(padre, false);

        RectTransform baseRT = joystickGO.GetComponent<RectTransform>();
        baseRT.anchorMin = new Vector2(0f, 0f);
        baseRT.anchorMax = new Vector2(0f, 0f);
        baseRT.pivot = new Vector2(0.5f, 0.5f);
        baseRT.anchoredPosition = new Vector2(145f, 150f);
        baseRT.sizeDelta = new Vector2(240f, 240f);

        Image baseImg = joystickGO.GetComponent<Image>();
        baseImg.sprite = spriteBase;
        baseImg.color = Color.white;
        baseImg.raycastTarget = true;

        // Handle (Pomo interior)
        GameObject handleGO = new GameObject("Handle", typeof(RectTransform), typeof(Image));
        handleGO.transform.SetParent(joystickGO.transform, false);

        RectTransform handleRT = handleGO.GetComponent<RectTransform>();
        handleRT.anchorMin = new Vector2(0.5f, 0.5f);
        handleRT.anchorMax = new Vector2(0.5f, 0.5f);
        handleRT.pivot = new Vector2(0.5f, 0.5f);
        handleRT.anchoredPosition = Vector2.zero;
        handleRT.sizeDelta = new Vector2(95f, 95f);

        Image handleImg = handleGO.GetComponent<Image>();
        handleImg.sprite = spritePomo;
        handleImg.color = Color.white;
        // IMPORTANTE: raycastTarget = false para que hacer clic en el centro NO bloquee el agarre del joystick
        handleImg.raycastTarget = false;

        virtualJoystick = joystickGO.GetComponent<VirtualJoystick>();
        virtualJoystick.BaseRect = baseRT;
        virtualJoystick.HandleRect = handleRT;
        virtualJoystick.HandleLimit = 85f;
    }

    private void CrearDPadVirtual(Transform padre, Sprite spriteBoton, Sprite spriteFlecha)
    {
        GameObject dpadGO = new GameObject("VirtualDPad", typeof(RectTransform), typeof(VirtualDPad));
        dpadGO.transform.SetParent(padre, false);

        RectTransform dpadRT = dpadGO.GetComponent<RectTransform>();
        dpadRT.anchorMin = new Vector2(0f, 0f);
        dpadRT.anchorMax = new Vector2(0f, 0f);
        dpadRT.pivot = new Vector2(0.5f, 0.5f);
        dpadRT.anchoredPosition = new Vector2(145f, 150f);
        dpadRT.sizeDelta = new Vector2(240f, 240f);

        virtualDPad = dpadGO.GetComponent<VirtualDPad>();

        float dist = 74f;
        Vector2 tamañoFlecha = new Vector2(70f, 70f);

        // Flecha Arriba
        virtualDPad.FlechaArriba = CrearBotonFlecha(dpadGO.transform, "Flecha_Arriba", new Vector2(0f, dist), tamañoFlecha, spriteBoton, spriteFlecha, 0f);

        // Flecha Abajo
        virtualDPad.FlechaAbajo = CrearBotonFlecha(dpadGO.transform, "Flecha_Abajo", new Vector2(0f, -dist), tamañoFlecha, spriteBoton, spriteFlecha, 180f);

        // Flecha Izquierda
        virtualDPad.FlechaIzquierda = CrearBotonFlecha(dpadGO.transform, "Flecha_Izquierda", new Vector2(-dist, 0f), tamañoFlecha, spriteBoton, spriteFlecha, 90f);

        // Flecha Derecha
        virtualDPad.FlechaDerecha = CrearBotonFlecha(dpadGO.transform, "Flecha_Derecha", new Vector2(dist, 0f), tamañoFlecha, spriteBoton, spriteFlecha, 270f);
    }

    private DPadButton CrearBotonFlecha(Transform padre, string nombre, Vector2 posicion, Vector2 tamaño, Sprite spriteFondo, Sprite spriteIcono, float rotacionIcono)
    {
        GameObject btnGO = new GameObject(nombre, typeof(RectTransform), typeof(Image), typeof(DPadButton));
        btnGO.transform.SetParent(padre, false);

        RectTransform rt = btnGO.GetComponent<RectTransform>();
        rt.anchoredPosition = posicion;
        rt.sizeDelta = tamaño;

        Image img = btnGO.GetComponent<Image>();
        img.sprite = spriteFondo;
        img.color = colorPrimario;
        img.raycastTarget = true;

        DPadButton dpadBtn = btnGO.GetComponent<DPadButton>();
        dpadBtn.TargetImage = img;
        dpadBtn.NormalColor = colorPrimario;
        dpadBtn.PressedColor = colorAcento;

        // Icono de flecha
        GameObject iconGO = new GameObject("Icono", typeof(RectTransform), typeof(Image));
        iconGO.transform.SetParent(btnGO.transform, false);

        RectTransform iconRT = iconGO.GetComponent<RectTransform>();
        iconRT.anchorMin = new Vector2(0.5f, 0.5f);
        iconRT.anchorMax = new Vector2(0.5f, 0.5f);
        iconRT.anchoredPosition = Vector2.zero;
        iconRT.sizeDelta = new Vector2(tamaño.x * 0.55f, tamaño.y * 0.55f);
        iconRT.localRotation = Quaternion.Euler(0f, 0f, rotacionIcono);

        Image iconImg = iconGO.GetComponent<Image>();
        iconImg.sprite = spriteIcono;
        iconImg.color = Color.white;
        iconImg.raycastTarget = false;

        return dpadBtn;
    }

    private void CrearBotonesAccion(Sprite spriteBoton)
    {
        GameObject panelAcciones = new GameObject("Panel_Acciones", typeof(RectTransform));
        panelAcciones.transform.SetParent(transform, false);

        RectTransform paRT = panelAcciones.GetComponent<RectTransform>();
        paRT.anchorMin = new Vector2(1f, 0f);
        paRT.anchorMax = new Vector2(1f, 0f);
        paRT.pivot = new Vector2(1f, 0f);
        paRT.anchoredPosition = new Vector2(-40f, 40f);
        paRT.sizeDelta = new Vector2(350f, 350f);

        // 1. Botón SALTO (Abajo a la derecha)
        CrearBotonGenerico(
            panelAcciones.transform,
            "Boton_Salto",
            new Vector2(-40f, 50f),
            new Vector2(90f, 90f),
            spriteBoton,
            "SALTAR",
            EjecutarSalto
        );

        // 2. Botón SPRINT / CORRER (Al lado de salto)
        imagenBotonSprint = CrearBotonGenerico(
            panelAcciones.transform,
            "Boton_Correr",
            new Vector2(-155f, 50f),
            new Vector2(85f, 85f),
            spriteBoton,
            "CORRER",
            AlternarSprint
        );

        // 3. Botón INTERACTUAR / MANO (Arriba del salto)
        imagenBotonInteractuar = CrearBotonGenerico(
            panelAcciones.transform,
            "Boton_Interactuar",
            new Vector2(-40f, 165f),
            new Vector2(95f, 95f),
            spriteBoton,
            "ACCIÓN\n[ E ]",
            EjecutarInteraccion
        );

        // 4. Botón LINTERNA (Arriba de correr)
        imagenBotonLinterna = CrearBotonGenerico(
            panelAcciones.transform,
            "Boton_Linterna",
            new Vector2(-155f, 165f),
            new Vector2(85f, 85f),
            spriteBoton,
            "LINTERNA\n[ F ]",
            AlternarLinterna
        );
    }

    private Image CrearBotonGenerico(Transform padre, string nombre, Vector2 posicion, Vector2 tamaño, Sprite spriteBoton, string etiqueta, Action onClickAccion)
    {
        GameObject btnGO = new GameObject(nombre, typeof(RectTransform), typeof(Image), typeof(Button));
        btnGO.transform.SetParent(padre, false);

        RectTransform rt = btnGO.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1f, 0f);
        rt.anchorMax = new Vector2(1f, 0f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = posicion;
        rt.sizeDelta = tamaño;

        Image img = btnGO.GetComponent<Image>();
        img.sprite = spriteBoton;
        img.color = colorPrimario;
        img.raycastTarget = true;

        Button btn = btnGO.GetComponent<Button>();
        btn.onClick.AddListener(() => onClickAccion?.Invoke());

        GameObject txtGO = new GameObject("Texto", typeof(RectTransform), typeof(TextMeshProUGUI));
        txtGO.transform.SetParent(btnGO.transform, false);

        RectTransform txtRT = txtGO.GetComponent<RectTransform>();
        txtRT.anchorMin = Vector2.zero;
        txtRT.anchorMax = Vector2.one;
        txtRT.offsetMin = Vector2.zero;
        txtRT.offsetMax = Vector2.zero;

        TMP_Text txt = txtGO.GetComponent<TMP_Text>();
        txt.text = etiqueta;
        txt.alignment = TextAlignmentOptions.Center;
        txt.fontSize = 15;
        txt.fontStyle = FontStyles.Bold;
        txt.color = Color.white;
        txt.raycastTarget = false;

        return img;
    }

    private void CrearBotonPausa(Sprite spriteBoton)
    {
        GameObject btnPausaGO = new GameObject("Boton_Pausa", typeof(RectTransform), typeof(Image), typeof(Button));
        btnPausaGO.transform.SetParent(transform, false);

        RectTransform rt = btnPausaGO.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(1f, 1f);
        rt.anchoredPosition = new Vector2(-40f, -40f);
        rt.sizeDelta = new Vector2(65f, 65f);

        Image img = btnPausaGO.GetComponent<Image>();
        img.sprite = spriteBoton;
        img.color = new Color(0.12f, 0.14f, 0.18f, 0.8f);
        img.raycastTarget = true;

        Button btn = btnPausaGO.GetComponent<Button>();
        btn.onClick.AddListener(AbrirPausa);

        GameObject txtGO = new GameObject("Icono_Pausa", typeof(RectTransform), typeof(TextMeshProUGUI));
        txtGO.transform.SetParent(btnPausaGO.transform, false);
        RectTransform txtRT = txtGO.GetComponent<RectTransform>();
        txtRT.anchorMin = Vector2.zero;
        txtRT.anchorMax = Vector2.one;
        txtRT.offsetMin = Vector2.zero;
        txtRT.offsetMax = Vector2.zero;

        TMP_Text txt = txtGO.GetComponent<TMP_Text>();
        txt.text = "❚❚";
        txt.alignment = TextAlignmentOptions.Center;
        txt.fontSize = 24;
        txt.color = Color.white;
        txt.raycastTarget = false;
    }

    #endregion

    #region Acciones de Control

    public void AlternarModoControl()
    {
        switch (modoActual)
        {
            case TipoControlMovimiento.RuedaJoystick:
                modoActual = TipoControlMovimiento.FlechasDPad;
                break;
            case TipoControlMovimiento.FlechasDPad:
                modoActual = TipoControlMovimiento.Ambos;
                break;
            case TipoControlMovimiento.Ambos:
                modoActual = TipoControlMovimiento.RuedaJoystick;
                break;
        }

        if (recordarPreferencia)
        {
            PlayerPrefs.SetInt(PREF_KEY_MODO, (int)modoActual);
            PlayerPrefs.Save();
        }

        ActualizarModoVisual();
    }

    private void ActualizarModoVisual()
    {
        if (virtualJoystick != null)
            virtualJoystick.gameObject.SetActive(modoActual == TipoControlMovimiento.RuedaJoystick || modoActual == TipoControlMovimiento.Ambos);

        if (virtualDPad != null)
            virtualDPad.gameObject.SetActive(modoActual == TipoControlMovimiento.FlechasDPad || modoActual == TipoControlMovimiento.Ambos);

        if (textoBotonModo != null)
        {
            switch (modoActual)
            {
                case TipoControlMovimiento.RuedaJoystick:
                    textoBotonModo.text = "🕹️ MODO: RUEDA";
                    break;
                case TipoControlMovimiento.FlechasDPad:
                    textoBotonModo.text = "🔀 MODO: FLECHAS";
                    break;
                case TipoControlMovimiento.Ambos:
                    textoBotonModo.text = "🔄 MODO: AMBOS";
                    break;
            }
        }

        if (virtualJoystick != null) virtualJoystick.ResetInput();
        if (virtualDPad != null) virtualDPad.ResetInput();
    }

    public void EjecutarSalto()
    {
        if (playerController != null)
            playerController.EjecutarSalto();
    }

    public void AlternarSprint()
    {
        sprintToggleActivo = !sprintToggleActivo;
        if (playerController != null)
            playerController.SetVirtualSprint(sprintToggleActivo);
    }

    public void AlternarLinterna()
    {
        if (playerController != null)
            playerController.AlternarLinterna();
    }

    public void EjecutarInteraccion()
    {
        if (interaccionJugador != null)
            interaccionJugador.Interactuar();
    }

    public void AbrirPausa()
    {
        if (menuPausa != null)
        {
            menuPausa.Pausar();
        }
    }

    #endregion

    #region Generación de Sprites Procedurales

    private Sprite GenerarSpriteCirculo(int resolucion, Color colorRelleno, Color colorBorde, int grosorBorde)
    {
        Texture2D tex = new Texture2D(resolucion, resolucion, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;

        float centro = (resolucion - 1) / 2f;
        float radio = centro - 1f;
        float radioInterior = radio - grosorBorde;

        Color[] pixeles = new Color[resolucion * resolucion];

        for (int y = 0; y < resolucion; y++)
        {
            for (int x = 0; x < resolucion; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(centro, centro));

                if (dist > radio)
                {
                    pixeles[y * resolucion + x] = Color.clear;
                }
                else if (dist > radioInterior && grosorBorde > 0)
                {
                    float alphaBorde = Mathf.Clamp01(radio - dist + 0.5f);
                    Color c = colorBorde;
                    c.a *= alphaBorde;
                    pixeles[y * resolucion + x] = c;
                }
                else
                {
                    float alphaRelleno = Mathf.Clamp01(radioInterior - dist + 0.5f);
                    Color c = Color.Lerp(colorBorde, colorRelleno, alphaRelleno);
                    pixeles[y * resolucion + x] = c;
                }
            }
        }

        tex.SetPixels(pixeles);
        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, resolucion, resolucion), new Vector2(0.5f, 0.5f), 100f);
    }

    private Sprite GenerarSpriteFlecha(int resolucion, Color colorFlecha)
    {
        Texture2D tex = new Texture2D(resolucion, resolucion, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;

        Color[] pixeles = new Color[resolucion * resolucion];
        for (int i = 0; i < pixeles.Length; i++)
            pixeles[i] = Color.clear;

        float centroX = (resolucion - 1) / 2f;
        float topY = resolucion * 0.85f;
        float botY = resolucion * 0.2f;
        float halfWidth = resolucion * 0.4f;

        for (int y = 0; y < resolucion; y++)
        {
            if (y < botY || y > topY) continue;

            float progreso = (y - botY) / (topY - botY);
            float anchoActual = halfWidth * (1f - progreso);

            int minX = Mathf.RoundToInt(centroX - anchoActual);
            int maxX = Mathf.RoundToInt(centroX + anchoActual);

            for (int x = minX; x <= maxX; x++)
            {
                if (x >= 0 && x < resolucion)
                {
                    pixeles[y * resolucion + x] = colorFlecha;
                }
            }
        }

        tex.SetPixels(pixeles);
        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, resolucion, resolucion), new Vector2(0.5f, 0.5f), 100f);
    }

    #endregion
}
