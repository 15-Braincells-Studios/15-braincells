using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public float mouseSensitivity = 0.1f;
    public Transform playerCamera;

    public float gravity = -9.81f;
    public float jumpHeight = 1.5f;

    private CharacterController controller;
    private float xRotation = 0f;
    private float verticalVelocity = 0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (Keyboard.current == null || Mouse.current == null)
            return;

        // WASD movement
        float x = 0f;
        float z = 0f;

        if (Keyboard.current.aKey.isPressed) x--;
        if (Keyboard.current.dKey.isPressed) x++;
        if (Keyboard.current.wKey.isPressed) z++;
        if (Keyboard.current.sKey.isPressed) z--;

        Vector3 move =
            (transform.right * x + transform.forward * z).normalized;

        // Gravity and jump
        if (controller.isGrounded)
        {
            if (verticalVelocity < 0f)
                verticalVelocity = -2f;

            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
        }

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity = move * speed;
        velocity.y = verticalVelocity;

        controller.Move(velocity * Time.deltaTime);

        // Mouse look
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        xRotation -= mouseDelta.y * mouseSensitivity;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        if (playerCamera != null)
        {
            playerCamera.localRotation =
                Quaternion.Euler(xRotation, 0f, 0f);
        }

        transform.Rotate(
            Vector3.up * mouseDelta.x * mouseSensitivity
        );
    }
}
