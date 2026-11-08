using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Joystick virtual táctil ("la rueda") para control analógico en dispositivos móviles y editor.
/// Soporta arrastre suave con ratón o dedo, radio límite y cálculo de vector de movimiento 2D.
/// </summary>
public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("Referencias de UI")]
    [SerializeField] private RectTransform baseRect;
    [SerializeField] private RectTransform handleRect;

    [Header("Configuración")]
    [SerializeField] private float handleLimit = 80f;
    [SerializeField] private float deadZone = 0.05f;

    private Canvas parentCanvas;
    private Camera uiCamera;
    private Vector2 inputVector = Vector2.zero;
    private Vector2 defaultHandlePosition = Vector2.zero;
    private bool isPointerDown = false;

    public Vector2 InputDirection => inputVector;
    public bool IsPressed => isPointerDown;

    public RectTransform BaseRect
    {
        get => baseRect;
        set => baseRect = value;
    }

    public RectTransform HandleRect
    {
        get => handleRect;
        set
        {
            handleRect = value;
            if (handleRect != null)
                defaultHandlePosition = handleRect.anchoredPosition;
        }
    }

    public float HandleLimit
    {
        get => handleLimit;
        set => handleLimit = value;
    }

    private void Awake()
    {
        if (baseRect == null)
            baseRect = GetComponent<RectTransform>();

        if (handleRect == null && transform.childCount > 0)
            handleRect = transform.GetChild(0).GetComponent<RectTransform>();

        ActualizarCamaraCanvas();

        if (handleRect != null)
            defaultHandlePosition = handleRect.anchoredPosition;
    }

    private void Start()
    {
        if (baseRect == null)
            baseRect = GetComponent<RectTransform>();

        if (handleRect == null && transform.childCount > 0)
            handleRect = transform.GetChild(0).GetComponent<RectTransform>();

        ActualizarCamaraCanvas();

        if (handleRect != null)
            defaultHandlePosition = handleRect.anchoredPosition;
    }

    private void ActualizarCamaraCanvas()
    {
        if (parentCanvas == null)
            parentCanvas = GetComponentInParent<Canvas>();

        if (parentCanvas != null && parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            uiCamera = parentCanvas.worldCamera;
        else
            uiCamera = null;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPointerDown = true;
        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (baseRect == null || handleRect == null)
            return;

        ActualizarCamaraCanvas();

        Vector2 position;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            baseRect,
            eventData.position,
            uiCamera,
            out position))
        {
            // Limitar la posición del pomo al radio del joystick
            Vector2 clampedPosition = Vector2.ClampMagnitude(position, handleLimit);
            handleRect.anchoredPosition = clampedPosition;

            // Calcular vector de entrada normalizado [0..1]
            Vector2 rawInput = clampedPosition / handleLimit;
            if (rawInput.magnitude < deadZone)
            {
                inputVector = Vector2.zero;
            }
            else
            {
                inputVector = rawInput;
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        ResetInput();
    }

    public void ResetInput()
    {
        isPointerDown = false;
        inputVector = Vector2.zero;

        if (handleRect != null)
            handleRect.anchoredPosition = defaultHandlePosition;
    }

    private void OnDisable()
    {
        ResetInput();
    }
}
