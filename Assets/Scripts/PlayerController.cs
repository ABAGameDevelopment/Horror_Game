
using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 5f;
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

    [Header("Intro")]
    [SerializeField] private IntroNivel introNivel;

    public event Action LinternaEncendidaPorPrimeraVez;

    private bool EnIntro =>
        introNivel != null &&
        introNivel.isActiveAndEnabled &&
        !introNivel.IntroduccionTerminada;

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
        if (EnIntro)
        {
            moveInput = Vector2.zero;
            lookInput = Vector2.zero;
            return;
        }

        HandleMovement();
        HandleLook();
    }

    // Movimiento
    public void OnMove(InputValue value)
    {
        if (EnIntro)
        {
            moveInput = Vector2.zero;
            return;
        }

        moveInput = value.Get<Vector2>();
    }

    // Cámara
    public void OnLook(InputValue value)
    {
        if (EnIntro)
        {
            lookInput = Vector2.zero;
            return;
        }

        lookInput = value.Get<Vector2>();
    }

    // Salto
    public void OnJump(InputValue value)
    {
        if (EnIntro) return;
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
        if (EnIntro) return;
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

    // Movimiento del jugador.
    private void HandleMovement()
    {
        if (controller == null) return;

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

        movement *= speed;

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

            cameraPitch = Mathf.Clamp(
                cameraPitch,
                -maxLookAngle,
                maxLookAngle
            );

            playerCamera.localRotation =
                Quaternion.Euler(cameraPitch, 0f, 0f);
        }
    }
}
