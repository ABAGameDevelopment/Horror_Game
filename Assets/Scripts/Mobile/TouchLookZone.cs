using UnityEngine;
using UnityEngine.EventSystems;


    /// <summary>
    /// Zona táctil en pantalla (generalmente mitad derecha) para rotar la cámara en primera persona.
    /// </summary>
    public class TouchLookZone : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [Header("Sensibilidad")]
        [SerializeField] private float sensibilidadX = 0.15f;
        [SerializeField] private float sensibilidadY = 0.15f;
        [SerializeField] private bool invertirY = false;

        private PlayerController playerController;
        private int pointerIdActivo = -1;
        private Vector2 lookDeltaAcumulado;

        public float SensibilidadX { get => sensibilidadX; set => sensibilidadX = value; }
        public float SensibilidadY { get => sensibilidadY; set => sensibilidadY = value; }
        public bool InvertirY { get => invertirY; set => invertirY = value; }

        public void ConfigurarJugador(PlayerController jugador)
        {
            playerController = jugador;
        }

        private void Awake()
        {
            if (playerController == null)
            {
                playerController = FindAnyObjectByType<PlayerController>();
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (pointerIdActivo == -1)
            {
                pointerIdActivo = eventData.pointerId;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.pointerId != pointerIdActivo)
                return;

            if (playerController == null)
            {
                playerController = FindAnyObjectByType<PlayerController>();
                if (playerController == null) return;
            }

            float deltaX = eventData.delta.x * sensibilidadX;
            float deltaY = (invertirY ? -eventData.delta.y : eventData.delta.y) * sensibilidadY;

            playerController.SetVirtualLookInput(new Vector2(deltaX, deltaY));
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.pointerId == pointerIdActivo)
            {
                pointerIdActivo = -1;
            }
        }

        public void ResetInput()
        {
            pointerIdActivo = -1;
        }

        private void OnDisable()
        {
            ResetInput();
        }
    }

