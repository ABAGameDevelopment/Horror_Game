using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class NotasUI : MonoBehaviour
{
    public static NotasUI Instance { get; private set; }

    [Header("Referencias")]
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private GameObject panelNota;
    [SerializeField] private Image imagenNota;
    [SerializeField] private TMP_Text textoContinuar;

    public bool MostrandoNota { get; private set; }

    private float tiempoApertura = -1f;
    private int frameApertura = -1;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;

        if (playerInput == null)
            playerInput = UnityEngine.Object.FindAnyObjectByType<PlayerInput>();

        AsegurarUI();
    }

    private void Start()
    {
        AsegurarUI();
        if (!MostrandoNota && panelNota != null)
            panelNota.SetActive(false);
    }

    private void Update()
    {
        if (!MostrandoNota) return;

        // Evitar que el mismo frame de apertura o la misma pulsacion de tecla cierre la nota inmediatamente
        if (Time.frameCount == frameApertura || Time.unscaledTime - tiempoApertura < 0.35f)
            return;

        bool cerrar = false;
        if (Keyboard.current != null && (Keyboard.current.eKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.escapeKey.wasPressedThisFrame))
            cerrar = true;
        if (Gamepad.current != null && (Gamepad.current.buttonWest.wasPressedThisFrame || Gamepad.current.buttonSouth.wasPressedThisFrame || Gamepad.current.buttonEast.wasPressedThisFrame))
            cerrar = true;

        if (cerrar)
        {
            CerrarNota();
        }
    }

    public void MostrarNota(Sprite imagen)
    {
        AsegurarUI();

        if (imagen == null)
        {
#if UNITY_EDITOR
            imagen = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Photos/Nota1.png");
#endif
            if (imagen == null)
            {
                var todosSprites = Resources.FindObjectsOfTypeAll<Sprite>();
                imagen = System.Linq.Enumerable.FirstOrDefault(todosSprites, s => s != null && s.name.Contains("Nota"));
            }
        }

        Time.timeScale = 0;

        if (imagenNota != null && imagen != null)
        {
            imagenNota.sprite = imagen;
            imagenNota.color = Color.white;
            imagenNota.preserveAspect = true;
        }

        ActualizarTextoContinuar();

        if (panelNota != null)
        {
            panelNota.SetActive(true);
            panelNota.transform.SetAsLastSibling();
        }

        tiempoApertura = Time.unscaledTime;
        frameApertura = Time.frameCount;
        MostrandoNota = true;
        Debug.Log("<color=green>[NotasUI]</color> Nota desplegada con exito: " + (imagen != null ? imagen.name : "null"));
    }

    private void ActualizarTextoContinuar()
    {
        if (textoContinuar == null) return;

        bool usaGamepad = Gamepad.current != null &&
            (Keyboard.current == null || Gamepad.current.lastUpdateTime > Keyboard.current.lastUpdateTime);
        bool esXbox = usaGamepad || (playerInput != null &&
            !string.IsNullOrEmpty(playerInput.currentControlScheme) &&
            (playerInput.currentControlScheme.ToLower().Contains("gamepad") ||
             playerInput.currentControlScheme.ToLower().Contains("xbox")));

        string boton = esXbox ? "X" : "E";
        textoContinuar.text = $"Presiona {boton} para continuar";
    }

    public void CerrarNota()
    {
        // Prevenir cierre involuntario inmediato
        if (Time.frameCount == frameApertura || Time.unscaledTime - tiempoApertura < 0.3f)
            return;

        if (panelNota != null)
            panelNota.SetActive(false);

        MostrandoNota = false;
        Time.timeScale = 1;
        Debug.Log("<color=yellow>[NotasUI]</color> Nota cerrada.");
    }

    public void AsegurarUI()
    {
        if (panelNota != null && imagenNota != null && textoContinuar != null)
            return;

        GameObject cGo = GameObject.Find("Canvas_NotasUI_Modal");
        Canvas canvas = null;
        if (cGo != null)
        {
            canvas = cGo.GetComponent<Canvas>();
        }
        if (canvas == null)
        {
            cGo = new GameObject("Canvas_NotasUI_Modal");
            canvas = cGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999; // Capa superior absoluta
            var scaler = cGo.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            cGo.AddComponent<GraphicRaycaster>();
            Object.DontDestroyOnLoad(cGo);
        }

        if (panelNota == null)
        {
            panelNota = new GameObject("PanelNota");
            panelNota.transform.SetParent(canvas.transform, false);
            Image bg = panelNota.AddComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.85f); // Fondo semitransparente oscuro
            RectTransform rt = panelNota.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            // Imagen de la nota centrada
            GameObject imgGo = new GameObject("ImagenNota");
            imgGo.transform.SetParent(panelNota.transform, false);
            imagenNota = imgGo.AddComponent<Image>();
            imagenNota.preserveAspect = true;
            imagenNota.color = Color.white;
            RectTransform rtImg = imgGo.GetComponent<RectTransform>();
            rtImg.anchorMin = new Vector2(0.5f, 0.5f);
            rtImg.anchorMax = new Vector2(0.5f, 0.5f);
            rtImg.sizeDelta = new Vector2(920f, 680f);
            rtImg.anchoredPosition = new Vector2(0f, 30f);

            // Texto inferior
            GameObject txtGo = new GameObject("TextoContinuar");
            txtGo.transform.SetParent(panelNota.transform, false);
            textoContinuar = txtGo.AddComponent<TextMeshProUGUI>();
            textoContinuar.font = RuntimeUI.FuenteTexto;
            textoContinuar.fontSize = 28f;
            textoContinuar.alignment = TextAlignmentOptions.Center;
            textoContinuar.color = Color.white;
            RectTransform rtTxt = txtGo.GetComponent<RectTransform>();
            rtTxt.anchorMin = new Vector2(0.5f, 0.5f);
            rtTxt.anchorMax = new Vector2(0.5f, 0.5f);
            rtTxt.sizeDelta = new Vector2(800f, 60f);
            rtTxt.anchoredPosition = new Vector2(0f, -380f);
        }
    }
}
