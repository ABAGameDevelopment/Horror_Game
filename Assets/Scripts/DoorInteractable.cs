using UnityEngine;
using UnityEngine.InputSystem;

public class DoorInteractable : MonoBehaviour
{
    [SerializeField] private float openAngle = 88f;
    [SerializeField] private float openSpeed = 160f;
    [SerializeField] private float autoOpenDistance = 1.95f;
    [SerializeField] private float interactDistance = 2.8f;
    [SerializeField] private bool invertDirection = false;

    private Vector3 hingeWorldPoint;
    private Vector3 closedCenterWorld;
    private float currentAngle = 0f;
    private bool isOpen = false;
    private bool initialized = false;
    private Transform playerTransform;

    public void Configure(float angle, bool invert, float triggerDist = 1.95f)
    {
        openAngle = angle;
        invertDirection = invert;
        autoOpenDistance = triggerDist;
    }

    private void Start()
    {
        InitializeHinge();
    }

    private void InitializeHinge()
    {
        if (initialized)
        {
            return;
        }

        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        if (renderers != null && renderers.Length > 0)
        {
            Bounds b = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                b.Encapsulate(renderers[i].bounds);
            }

            closedCenterWorld = b.center;
            // El borde de bisagra está en el extremo del eje más ancho (X o Z) del bounding box de la puerta
            if (b.size.x >= b.size.z)
            {
                float hingeX = invertDirection ? b.max.x : b.min.x;
                hingeWorldPoint = new Vector3(hingeX, b.center.y, b.center.z);
            }
            else
            {
                float hingeZ = invertDirection ? b.max.z : b.min.z;
                hingeWorldPoint = new Vector3(b.center.x, b.center.y, hingeZ);
            }
        }
        else
        {
            closedCenterWorld = transform.position;
            hingeWorldPoint = transform.position;
        }

        GameObject playerObj = GameObject.Find("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
        else if (Camera.main != null)
        {
            playerTransform = Camera.main.transform;
        }

        initialized = true;
    }

    private void Update()
    {
        if (!initialized)
        {
            InitializeHinge();
        }

        if (LevelStateManager.Instance != null && !LevelStateManager.Instance.JuegoActivo) return;

        if (playerTransform == null)
        {
            if (Camera.main != null)
            {
                playerTransform = Camera.main.transform;
            }
            else
            {
                return;
            }
        }

        Vector2 playerXZ = new Vector2(playerTransform.position.x, playerTransform.position.z);
        Vector2 doorXZ = new Vector2(closedCenterWorld.x, closedCenterWorld.z);
        float dist = Vector2.Distance(playerXZ, doorXZ);
        float yDiff = Mathf.Abs(playerTransform.position.y - closedCenterWorld.y);

        bool pressedInteract = false;
        if (Keyboard.current != null && (Keyboard.current.eKey.wasPressedThisFrame || Keyboard.current.fKey.wasPressedThisFrame))
        {
            pressedInteract = true;
        }
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            pressedInteract = true;
        }

        if (yDiff < 3.5f)
        {
            if (dist <= interactDistance && pressedInteract)
            {
                isOpen = !isOpen;
            }
            else if (dist <= autoOpenDistance)
            {
                isOpen = true;
            }
            else if (dist > interactDistance + 1.2f && isOpen)
            {
                isOpen = false;
            }
        }

        float target = isOpen ? (invertDirection ? -openAngle : openAngle) : 0f;
        if (Mathf.Abs(currentAngle - target) > 0.05f)
        {
            float nextAngle = Mathf.MoveTowards(currentAngle, target, openSpeed * Time.deltaTime);
            float delta = nextAngle - currentAngle;
            transform.RotateAround(hingeWorldPoint, Vector3.up, delta);
            currentAngle = nextAngle;
        }
    }
}
