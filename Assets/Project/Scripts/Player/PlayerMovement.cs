using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    private CharacterController controller;
    
    // Animation
    private Animator animator;
    private float airTime;
    
    [Header("Movement")]
    [SerializeField] private float baseSpeed = 5f;
    [SerializeField] private float sprintSpeed = 10f;
    [SerializeField] private float smoothInputSpeed = 0.2f;
    [SerializeField] private float smoothSprintSpeed = 0.2f;
    private InputAction moveAction;
    private InputAction sprintAction;
    private Vector2 moveDirection;
    private Vector2 currentDirection;
    private Vector2 smoothInputVelocity;
    private float currentSpeed;
    private float smoothSpeedVelocity;
    
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

        if (playerCamera == null) Debug.LogError("Player camera not found");
        
        // Bind all action inputs
        moveAction = InputSystem.actions.FindAction("Move");
        sprintAction = InputSystem.actions.FindAction("Sprint");
        jumpAction = InputSystem.actions.FindAction("Jump");
        lookAction = InputSystem.actions.FindAction("Look");
        
        // Make sure the player does not speed up from 0
        currentSpeed = baseSpeed;
        
        // Lock cursor to middle of the screen and make it not visible
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // Get the Animator for 3D model
        animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        // Movement
        moveDirection = moveAction.ReadValue<Vector2>();
        currentDirection = Vector2.SmoothDamp(currentDirection, moveDirection, ref smoothInputVelocity, smoothInputSpeed);
        Vector3 move = transform.right * currentDirection.x + transform.forward * currentDirection.y;
        
        // Sprinting
        float targetSpeed = sprintAction.IsPressed() ? sprintSpeed : baseSpeed;
        currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref smoothSpeedVelocity, smoothSprintSpeed);
        
        // Gravity and jump
        if (controller.isGrounded)
        {
            if (verticalVelocity < 0f) verticalVelocity = -2f;

            if (jumpAction.WasPressedThisFrame())
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
                animator.SetTrigger("Jump");
            }
            
        }
        verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity = move * currentSpeed;
        velocity.y = verticalVelocity;

        controller.Move(velocity * Time.deltaTime);
        
        // Pass the Speed your moving at to the animator
        Vector3 horizontal = controller.velocity;
        horizontal.y = 0f;
        animator.SetFloat("Speed", horizontal.magnitude, 0.1f, Time.deltaTime);
        
        // Set animator Bool for Jumping animation
        animator.SetBool("IsGrounded", controller.isGrounded);

        // Mouse look
        Vector2 lookDelta = lookAction.ReadValue<Vector2>();

        xRotation -= lookDelta.y * mouseSensitivity;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        
        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * (lookDelta.x * mouseSensitivity));
    }
}
