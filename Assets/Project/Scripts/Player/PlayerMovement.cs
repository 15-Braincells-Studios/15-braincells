using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    private CharacterController controller;
    
    [Header("Movement")]
    [SerializeField] private float speed = 5f;
    private InputAction moveAction;
    private Vector2 moveDirection;
    
    [Header("Jumping")]
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -9.81f;
    private float verticalVelocity;
    private InputAction jumpAction;

    [Header("Looking")]
    [SerializeField] private Transform playerCamera;
    [SerializeField] private float mouseSensitivity = 0.1f;
    private InputAction lookAction;
    private float xRotation;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");
        lookAction = InputSystem.actions.FindAction("Look");
        
        // Lock cursor to middle of the screen and make it not visible
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {

        // Movement
        moveDirection = moveAction.ReadValue<Vector2>();
        Vector3 move = transform.right * moveDirection.x + transform.forward * moveDirection.y;

        // Gravity and jump
        if (controller.isGrounded)
        {
            if (verticalVelocity < 0f) verticalVelocity = -2f;

            if (jumpAction.WasPressedThisFrame())
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
        }
        verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity = move * speed;
        velocity.y = verticalVelocity;

        controller.Move(velocity * Time.deltaTime);

        // Mouse look
        Vector2 lookDelta = lookAction.ReadValue<Vector2>();

        xRotation -= lookDelta.y * mouseSensitivity;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        if (playerCamera != null)
        {
            playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }

        transform.Rotate(Vector3.up * (lookDelta.x * mouseSensitivity));
    }
}
