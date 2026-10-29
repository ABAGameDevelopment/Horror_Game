
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

    private bool yaSeNotificoLinternaEncendida;

    private CharacterController controller;

    private Vector2 moveInput;
    private Vector2 lookInput;

    private float verticalVelocity;
    private float cameraPitch;

    [SerializeField] private int maxJumps = 2;
    private int jumpsUsed = 0;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        // Bloquear el control durante la introduccion.
        if (introNivel != null &&
            !introNivel.IntroduccionTerminada)
        {
            moveInput = Vector2.zero;
            lookInput = Vector2.zero;
            return;
        }

        HandleMovement();
        HandleLook();
    }

    public void OnMove(InputValue value)
    {
        if (introNivel != null &&
            !introNivel.IntroduccionTerminada)
        {
            moveInput = Vector2.zero;
            return;
        }

        moveInput = value.Get<Vector2>();
    }

    public void OnLook(InputValue value)
    {
        if (introNivel != null &&
            !introNivel.IntroduccionTerminada)
        {
            lookInput = Vector2.zero;
            return;
        }

        lookInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        if (introNivel != null &&
            !introNivel.IntroduccionTerminada)
        {
            return;
        }

        if (!value.isPressed)
            return;

        if (controller.isGrounded || jumpsUsed < maxJumps)
        {
            verticalVelocity =
                Mathf.Sqrt(jumpHeight * -2f * gravity);

            jumpsUsed++;
        }
    }

    public void OnLight(InputValue value)
    {
        if (introNivel != null &&
            !introNivel.IntroduccionTerminada)
            return;

        if (!value.isPressed)
            return;

        linternaEncendida = !linternaEncendida;

        if (luzLinterna != null)
            luzLinterna.enabled = linternaEncendida;

        // Avisar solamente la primera vez que se enciende.
        if (linternaEncendida && !yaSeNotificoLinternaEncendida)
        {
            yaSeNotificoLinternaEncendida = true;
            LinternaEncendidaPorPrimeraVez?.Invoke();
        }
    }

    private void HandleMovement()
    {
        if (controller.isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f;
            jumpsUsed = 0;
        }

        Vector3 movement =
            transform.right * moveInput.x +
            transform.forward * moveInput.y;

        movement *= speed;

        verticalVelocity += gravity * Time.deltaTime;
        movement.y = verticalVelocity;

        controller.Move(movement * Time.deltaTime);
    }

    private void HandleLook()
    {
        float cameraX;
        float cameraY;

        if (Gamepad.current != null &&
            Gamepad.current.rightStick.ReadValue().magnitude > 0.1f)
        {
            cameraX = lookInput.x *
                      controllerSensitivity *
                      Time.deltaTime;

            cameraY = lookInput.y *
                      controllerSensitivity *
                      Time.deltaTime;
        }
        else
        {
            cameraX = lookInput.x * mouseSensitivity;
            cameraY = lookInput.y * mouseSensitivity;
        }

        transform.Rotate(Vector3.up * cameraX);

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
