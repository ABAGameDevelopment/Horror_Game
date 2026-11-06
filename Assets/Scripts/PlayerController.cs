
using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 4.5f;
    [SerializeField] private float runSpeed = 8.0f;
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float jumpHeight = 1.2f;

    [Header("Look")]
    [SerializeField] private Transform playerCamera;
    [SerializeField] private float mouseSensitivity = 0.2f;
    [SerializeField] private float controllerSensitivity = 100f;
    [SerializeField] private float maxLookAngle = 80f;

    [Header("Linterna")]
    [SerializeField] private Light luzLinterna;
    [SerializeField] private bool linternaEncendida = true;
    [SerializeField] private AudioSource audioLinterna;
    [SerializeField] private AudioClip sonidoLinterna;

    [Header("Intro")]
    [SerializeField] private IntroNivel introNivel;

    public event Action LinternaEncendidaPorPrimeraVez;

    public bool EstaCorriendo { get; private set; }
    public bool EstaMoviendose { get; private set; }

    private bool EnIntro
    {
        get
        {
            // La cinematica de IntroNivel solo pertenece al Nivel 1 (Universidad)
            string escena = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (escena != "University" && escena != "Level1")
                return false;

            return introNivel != null &&
                   introNivel.isActiveAndEnabled &&
                   !introNivel.IntroduccionTerminada;
        }
    }

    private bool yaSeNotificoLinternaEncendida;

    private CharacterController controller;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private float verticalVelocity;
    private float cameraPitch;

    [Header("Salto")]
    [SerializeField] private int maxJumps = 2;
    private int jumpsUsed = 0;
    private int ultimoFrameLinterna = -1;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Sincronizar el estado inicial de la linterna.
        if (luzLinterna != null)
        {
            luzLinterna.enabled = linternaEncendida;
        }
    }

    private void Start()
    {
        if (speed < 1f) speed = 4.5f;
        if (runSpeed < 5f) runSpeed = 8.5f;
        AsegurarLinterna();
        PasosJugador pasos = GetComponent<PasosJugador>();
        if (pasos == null) gameObject.AddComponent<PasosJugador>();
    }

    public void AsegurarLinterna()
    {
        if (luzLinterna == null)
        {
            luzLinterna = GetComponentInChildren<Light>();
        }

        if (luzLinterna == null)
        {
            Transform camTransform = playerCamera != null ? playerCamera.transform : (Camera.main != null ? Camera.main.transform : transform.Find("Camera"));
            if (camTransform == null) camTransform = transform;

            GameObject lampObj = new GameObject("Lampara");
            lampObj.transform.SetParent(camTransform, false);
            lampObj.transform.localPosition = new Vector3(0.2f, -0.15f, 0.35f);
            lampObj.transform.localRotation = Quaternion.identity;

            Light spot = lampObj.AddComponent<Light>();
            spot.type = LightType.Spot;
            spot.range = 35f;
            spot.spotAngle = 65f;
            spot.innerSpotAngle = 45f;
            spot.intensity = 2.6f;
            spot.color = new Color(1f, 0.96f, 0.88f);
            spot.shadows = LightShadows.Soft;

            luzLinterna = spot;
        }

        if (audioLinterna == null)
        {
            audioLinterna = GetComponent<AudioSource>();
            if (audioLinterna == null) audioLinterna = gameObject.AddComponent<AudioSource>();
        }

        if (sonidoLinterna == null)
        {
            sonidoLinterna = System.Linq.Enumerable.FirstOrDefault(Resources.FindObjectsOfTypeAll<AudioClip>(), c => c != null && c.name.ToLower().Contains("lampara"));
#if UNITY_EDITOR
            if (sonidoLinterna == null)
                sonidoLinterna = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Lampara.mp3");
#endif
        }

        if (luzLinterna != null)
        {
            luzLinterna.enabled = linternaEncendida;
        }
    }


    private void Update()
    {
        // Tecla F directa para linterna (funciona siempre al presionar F, incluso en Nivel 1)
        if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.fKey.wasPressedThisFrame)
        {
            if (NotasUI.Instance == null || !NotasUI.Instance.MostrandoNota)
            {
                ToggleLinterna();
            }
        }

        if (EnIntro || (NotasUI.Instance != null && NotasUI.Instance.MostrandoNota))
        {
            moveInput = Vector2.zero;
            lookInput = Vector2.zero;

            AplicarGravedadSinMovimiento();
            return;
        }

        HandleMovement();
        HandleLook();
    }

    // Movimiento
    public void OnMove(InputValue value)
    {
        if (EnIntro || (NotasUI.Instance != null && NotasUI.Instance.MostrandoNota))
        {
            moveInput = Vector2.zero;
            return;
        }

        moveInput = value.Get<Vector2>();
    }

    // Cámara
    public void OnLook(InputValue value)
    {
        if (EnIntro || (NotasUI.Instance != null && NotasUI.Instance.MostrandoNota))
        {
            lookInput = Vector2.zero;
            return;
        }

        lookInput = value.Get<Vector2>();
    }

    // Salto
    public void OnJump(InputValue value)
    {
        if (EnIntro || (NotasUI.Instance != null && NotasUI.Instance.MostrandoNota)) return;
        if (CarEscapeController.Instance != null && CarEscapeController.Instance.JugadorEscondido) return;
        if (!value.isPressed) return;

        if (controller.isGrounded || jumpsUsed < maxJumps)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpsUsed++;
        }
    }

    // Acción de linterna: F o botón del control Xbox.
    public void OnLight(InputValue value)
    {
        if (NotasUI.Instance != null && NotasUI.Instance.MostrandoNota) return;
        if (!value.isPressed) return;

        ToggleLinterna();
    }

    // Encender o apagar linterna.
    public void ToggleLinterna()
    {
        // Debounce de frame exacto: evita que OnLight y Update se anulen entre si en el mismo frame
        if (Time.frameCount == ultimoFrameLinterna)
            return;
        ultimoFrameLinterna = Time.frameCount;

        if (luzLinterna == null)
        {
            AsegurarLinterna();
        PasosJugador pasos = GetComponent<PasosJugador>();
        if (pasos == null) gameObject.AddComponent<PasosJugador>();
        }

        if (luzLinterna == null)
        {
            Debug.LogWarning("No se ha podido inicializar la luz de la linterna en PlayerController.");
            return;
        }

        linternaEncendida = !linternaEncendida;
        luzLinterna.enabled = linternaEncendida;

        if (audioLinterna != null && sonidoLinterna != null)
            audioLinterna.PlayOneShot(sonidoLinterna);

        Debug.Log(linternaEncendida
            ? "Linterna encendida."
            : "Linterna apagada.");

        // Notificar solamente la primera vez que se enciende.
        if (linternaEncendida && !yaSeNotificoLinternaEncendida)
        {
            yaSeNotificoLinternaEncendida = true;
            LinternaEncendidaPorPrimeraVez?.Invoke();
        }
    }

    public void ApagarLinterna()
    {
        if (luzLinterna != null && linternaEncendida)
        {
            linternaEncendida = false;
            luzLinterna.enabled = false;
        }
    }

    // Movimiento del jugador.
    private void HandleMovement()
    {
        if (controller == null)
        {
            controller = GetComponent<CharacterController>();
            if (controller == null) return;
        }
        if (CarEscapeController.Instance != null && CarEscapeController.Instance.JugadorEscondido) return;

        if (controller.isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f;
            jumpsUsed = 0;
        }

        Vector2 effectiveMove = moveInput;

        // Respaldo para teclado.
        if (effectiveMove.sqrMagnitude < 0.01f && Keyboard.current != null)
        {
            float kx = 0f;
            float ky = 0f;

            if (Keyboard.current.wKey.isPressed ||
                Keyboard.current.upArrowKey.isPressed)
                ky += 1f;

            if (Keyboard.current.sKey.isPressed ||
                Keyboard.current.downArrowKey.isPressed)
                ky -= 1f;

            if (Keyboard.current.aKey.isPressed ||
                Keyboard.current.leftArrowKey.isPressed)
                kx -= 1f;

            if (Keyboard.current.dKey.isPressed ||
                Keyboard.current.rightArrowKey.isPressed)
                kx += 1f;

            effectiveMove = new Vector2(kx, ky).normalized;
        }

        // Respaldo para salto con espacio.
        if (Keyboard.current != null &&
            Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (controller.isGrounded || jumpsUsed < maxJumps)
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
                jumpsUsed++;
            }
        }

                float baseSpeed = speed > 1f ? speed : 4.5f;
        float sprintSpeed = runSpeed > baseSpeed ? runSpeed : 8.5f;

        bool corriendo = false;
        if (Keyboard.current != null && (Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed))
        {
            corriendo = true;
        }
        if (Gamepad.current != null && (Gamepad.current.leftStickButton.isPressed || Gamepad.current.leftShoulder.isPressed))
        {
            corriendo = true;
        }

        bool tieneInput = effectiveMove.sqrMagnitude > 0.01f;
        EstaMoviendose = tieneInput;
        EstaCorriendo = corriendo && tieneInput;

        float currentSpeed = EstaCorriendo ? sprintSpeed : baseSpeed;

        Vector3 movement =
            transform.right * effectiveMove.x +
            transform.forward * effectiveMove.y;

        movement *= currentSpeed;

        verticalVelocity += gravity * Time.deltaTime;
        movement.y = verticalVelocity;

        controller.Move(movement * Time.deltaTime);
    }

    // Rotación de cámara.
    private void HandleLook()
    {
        Vector2 effectiveLook = lookInput;

        if (effectiveLook.sqrMagnitude < 0.001f &&
            Mouse.current != null)
        {
            effectiveLook = Mouse.current.delta.ReadValue();
        }

        float cameraX;
        float cameraY;

        if (Gamepad.current != null &&
            Gamepad.current.rightStick.ReadValue().magnitude > 0.1f)
        {
            cameraX = effectiveLook.x *
                      controllerSensitivity *
                      Time.deltaTime;

            cameraY = effectiveLook.y *
                      controllerSensitivity *
                      Time.deltaTime;
        }
        else
        {
            cameraX = effectiveLook.x * mouseSensitivity;
            cameraY = effectiveLook.y * mouseSensitivity;
        }

        transform.Rotate(Vector3.up * cameraX);

        if (playerCamera != null)
        {
            cameraPitch -= cameraY;

            float minPitch = -maxLookAngle;
            float maxPitch = maxLookAngle;

            if (CarEscapeController.Instance != null && CarEscapeController.Instance.JugadorEscondido)
            {
                minPitch = -22f;
                maxPitch = 22f;
            }

            cameraPitch = Mathf.Clamp(
                cameraPitch,
                minPitch,
                maxPitch
            );

            playerCamera.localRotation =
                Quaternion.Euler(cameraPitch, 0f, 0f);
        }
    }

    private void AplicarGravedadSinMovimiento()
    {
        if (controller == null) return;

        if (controller.isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f;
            jumpsUsed = 0;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        controller.Move(Vector3.up * verticalVelocity * Time.deltaTime);
    }
}
