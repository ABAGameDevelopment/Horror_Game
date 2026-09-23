using UnityEngine;
using UnityEngine.InputSystem;

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

    private CharacterController controller;

    private Vector2 moveInput;
    private Vector2 lookInput;

    private float verticalVelocity;
    private float cameraPitch;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        HandleMovement();
        HandleLook();
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnLook(InputValue value)
    {
        lookInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed && controller.isGrounded)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    private void HandleMovement()
    {
        if (controller.isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f;
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

        if (Gamepad.current != null && Gamepad.current.rightStick.ReadValue().magnitude > 0.1f)
        {
            cameraX = lookInput.x * controllerSensitivity * Time.deltaTime;
            cameraY = lookInput.y * controllerSensitivity * Time.deltaTime;
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