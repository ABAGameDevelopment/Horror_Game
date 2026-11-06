
using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float jumpHeight = 1.2f;

    [Header("Sprint")]
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float sprintDuration = 4f;
    [SerializeField] private float sprintRegeneration = 5f;
    [SerializeField] private float sprintRecoveryDelay = 5f;

    private float recoveryTimer;
    private bool sprintBloqueado;

    private bool sprintInputHeld;
    private bool sprinting;
    private float sprintEnergy = 1f;

    public float SprintEnergy => sprintEnergy;
    public bool IsSprinting => sprinting;
    public bool EstaMoviendose { get; private set; }
    public bool EstaCorriendo { get; private set; }

    public event Action<float> SprintEnergyChanged;

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

    private void Update()
    {
        if (EnIntro || (NotasUI.Instance != null && NotasUI.Instance.MostrandoNota))
        {
            moveInput = Vector2.zero;
            lookInput = Vector2.zero;
            sprinting = false;

            ActualizarEnergiaSprint();
            AplicarGravedadSinMovimiento();
            return;
        }

        HandleMovement();
        HandleLook();

        ActualizarEnergiaSprint();
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

    // Sprint: Shift izquierdo o LS del control Xbox.
    public void OnRun(InputValue value)
    {
        sprintInputHeld = value.isPressed;
    }

    // Acción de linterna: F o botón del control Xbox.
    public void OnLight(InputValue value)
    {
        if (EnIntro || (NotasUI.Instance != null && NotasUI.Instance.MostrandoNota)) return;
        if (!value.isPressed) return;

        ToggleLinterna();
    }

    // Encender o apagar linterna.
    private void ToggleLinterna()
    {
        if (luzLinterna == null)
        {
            Debug.LogWarning("No se ha asignado la luz de la linterna en PlayerController.");
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

        if (CarEscapeController.Instance != null &&
    CarEscapeController.Instance.JugadorEscondido)
        {
            sprinting = false;
            return;
        }
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

        Vector3 movement =
    transform.right * effectiveMove.x +
    transform.forward * effectiveMove.y;

        bool tieneMovimiento = effectiveMove.sqrMagnitude > 0.01f;

        EstaMoviendose = tieneMovimiento;

        sprinting =
            sprintInputHeld &&
            tieneMovimiento &&
            sprintEnergy > 0.001f &&
            !sprintBloqueado;

        EstaCorriendo = sprinting;

        // Aplicar la velocidad correspondiente.
        float velocidadActual = sprinting ? sprintSpeed : speed;

        movement *= velocidadActual;

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

    private void ActualizarEnergiaSprint()
    {
        // Si estamos corriendo, consumir energía.
        if (sprinting)
        {
            sprintEnergy -= Time.deltaTime / sprintDuration;

            // Reiniciar el tiempo de recuperación.
            recoveryTimer = 0f;

            // Si se agota la energía, bloquear el sprint.
            if (sprintEnergy <= 0f)
            {
                sprintEnergy = 0f;

                sprinting = false;
                sprintInputHeld = false;

                sprintBloqueado = true;
            }
        }
        else
        {
            // Si todavía no tenemos energía completa.
            if (sprintEnergy < 1f)
            {
                recoveryTimer += Time.deltaTime;

                // Esperar antes de comenzar la regeneración.
                if (recoveryTimer >= sprintRecoveryDelay)
                {
                    sprintEnergy += Time.deltaTime / sprintRegeneration;
                }
            }
            else
            {
                recoveryTimer = 0f;
                sprintBloqueado = false;
            }
        }

        sprintEnergy = Mathf.Clamp01(sprintEnergy);

        // Actualizar la barra de energía.
        SprintEnergyChanged?.Invoke(sprintEnergy);
    }

    private void OnDisable()
    {
        sprintInputHeld = false;
        sprinting = false;
    }
}
