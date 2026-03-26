using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 9f;
    public float jumpHeight = 1.2f;
    public float gravity = -20f;

    [Header("Mouse Look")]
    public float mouseSensitivity = 0.3f;
    public float maxLookAngle = 85f;

    private CharacterController controller;
    private Transform cameraTransform;
    private Vector3 velocity;
    private float xRotation;
    private bool isGrounded;

    // Input references
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction jumpAction;
    private InputAction sprintAction;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        cameraTransform = Camera.main.transform;

        // Build input actions inline so no InputActionAsset is needed
        moveAction = new InputAction("Move", InputActionType.Value);
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");

        lookAction = new InputAction("Look", InputActionType.Value, "<Mouse>/delta");

        jumpAction = new InputAction("Jump", InputActionType.Button, "<Keyboard>/space");

        sprintAction = new InputAction("Sprint", InputActionType.Button, "<Keyboard>/leftShift");
    }

    void OnEnable()
    {
        moveAction.Enable();
        lookAction.Enable();
        jumpAction.Enable();
        sprintAction.Enable();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnDisable()
    {
        moveAction.Disable();
        lookAction.Disable();
        jumpAction.Disable();
        sprintAction.Disable();
    }

    void Update()
    {
        HandleGroundCheck();
        HandleMovement();
        HandleMouseLook();
        HandleJump();
        ApplyGravity();
    }

    void HandleGroundCheck()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0f)
            velocity.y = -2f;
    }

    void HandleMovement()
    {
        Vector2 input = moveAction.ReadValue<Vector2>();
        Vector3 move = transform.right * input.x + transform.forward * input.y;

        float speed = sprintAction.IsPressed() ? sprintSpeed : walkSpeed;
        controller.Move(move * speed * Time.deltaTime);
    }

    void HandleMouseLook()
    {
        Vector2 delta = lookAction.ReadValue<Vector2>();

        float mouseX = delta.x * mouseSensitivity;
        float mouseY = delta.y * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);

        // Rotate player on Y axis (yaw)
        transform.Rotate(Vector3.up * mouseX);

        // Apply pitch to camera, keeping it aligned with player's yaw
        cameraTransform.rotation = transform.rotation * Quaternion.Euler(xRotation, 0f, 0f);

        // Keep camera at player position (in case camera isn't a child)
        cameraTransform.position = transform.position + Vector3.up * 0.6f;
    }

    void HandleJump()
    {
        if (jumpAction.WasPressedThisFrame() && isGrounded)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
    }

    void ApplyGravity()
    {
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
