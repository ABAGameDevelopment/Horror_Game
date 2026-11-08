using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


    /// <summary>
    /// Cruceta direccional táctil con 4 flechas (Arriba, Abajo, Izquierda, Derecha).
    /// </summary>
    public class VirtualDPad : MonoBehaviour
    {
        [Header("Referencias de Botones de Flechas")]
        [SerializeField] private DPadButton flechaArriba;
        [SerializeField] private DPadButton flechaAbajo;
        [SerializeField] private DPadButton flechaIzquierda;
        [SerializeField] private DPadButton flechaDerecha;

        private Vector2 inputVector = Vector2.zero;

        public Vector2 InputDirection => inputVector;

        public DPadButton FlechaArriba { get => flechaArriba; set => flechaArriba = value; }
        public DPadButton FlechaAbajo { get => flechaAbajo; set => flechaAbajo = value; }
        public DPadButton FlechaIzquierda { get => flechaIzquierda; set => flechaIzquierda = value; }
        public DPadButton FlechaDerecha { get => flechaDerecha; set => flechaDerecha = value; }

        private void Update()
        {
            CalcularDireccion();
        }

        private void CalcularDireccion()
        {
            float x = 0f;
            float y = 0f;

            if (flechaArriba != null && flechaArriba.IsPressed)
                y += 1f;

            if (flechaAbajo != null && flechaAbajo.IsPressed)
                y -= 1f;

            if (flechaDerecha != null && flechaDerecha.IsPressed)
                x += 1f;

            if (flechaIzquierda != null && flechaIzquierda.IsPressed)
                x -= 1f;

            Vector2 raw = new Vector2(x, y);
            if (raw.sqrMagnitude > 1f)
            {
                inputVector = raw.normalized;
            }
            else
            {
                inputVector = raw;
            }
        }

        public void ResetInput()
        {
            if (flechaArriba != null) flechaArriba.ResetState();
            if (flechaAbajo != null) flechaAbajo.ResetState();
            if (flechaIzquierda != null) flechaIzquierda.ResetState();
            if (flechaDerecha != null) flechaDerecha.ResetState();
            inputVector = Vector2.zero;
        }

        private void OnDisable()
        {
            ResetInput();
        }
    }

    /// <summary>
    /// Botón individual de flecha para el D-Pad táctil.
    /// </summary>
    public class DPadButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IPointerEnterHandler
    {
        [SerializeField] private Image targetImage;
        [SerializeField] private Color normalColor = new Color(1f, 1f, 1f, 0.45f);
        [SerializeField] private Color pressedColor = new Color(0.2f, 0.8f, 1f, 0.95f);

        private bool isPressed = false;
        private bool isPointerInside = false;

        public bool IsPressed => isPressed;

        public Image TargetImage
        {
            get => targetImage;
            set => targetImage = value;
        }

        public Color NormalColor
        {
            get => normalColor;
            set => normalColor = value;
        }

        public Color PressedColor
        {
            get => pressedColor;
            set => pressedColor = value;
        }

        private void Awake()
        {
            if (targetImage == null)
                targetImage = GetComponent<Image>();

            ActualizarColor();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            isPressed = true;
            isPointerInside = true;
            ActualizarColor();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            isPressed = false;
            ActualizarColor();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isPointerInside = false;
            isPressed = false;
            ActualizarColor();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            // Si el dedo se arrastra desde otra flecha hacia esta manteniendo presionado
            if (eventData.dragging)
            {
                isPressed = true;
                isPointerInside = true;
                ActualizarColor();
            }
        }

        public void ResetState()
        {
            isPressed = false;
            isPointerInside = false;
            ActualizarColor();
        }

        private void ActualizarColor()
        {
            if (targetImage != null)
            {
                targetImage.color = isPressed ? pressedColor : normalColor;
            }
        }
    }

