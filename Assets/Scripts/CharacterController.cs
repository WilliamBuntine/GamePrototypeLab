using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class FPSControllerWithSwinging : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 7f;
    public float sprintSpeed = 12f;
    public float jumpForce = 7f;
    public float groundDrag = 6f;
    public float airMultiplier = 0.5f;
    public float groundCheckDistance = 0.5f;
    public LayerMask groundMask;

    [Header("Look Settings")]
    public float mouseSensitivity = 100f;
    public Transform playerCamera;

    private Rigidbody rb;
    private float xRotation = 0f;
    private bool grounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true; // stop tipping over

        // Make sure player has a collider for ground check
        if (!GetComponent<CapsuleCollider>())
        {
            Debug.LogWarning("Player should have a CapsuleCollider for proper grounding.");
        }

        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        HandleLook();
        GroundCheck();

        // Apply drag when grounded for snappy stops
        rb.linearDamping = grounded ? groundDrag : 0f;

        if (Input.GetButtonDown("Jump") && grounded)
        {
            Jump();
        }
    }

    void FixedUpdate()
    {
        HandleMovement();
        ClampSpeed();
    }

    void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // rotate player left/right
        transform.Rotate(Vector3.up * mouseX);

        // rotate camera up/down
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    void HandleMovement()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 moveDir = (transform.right * moveX + transform.forward * moveZ).normalized;
        float targetSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed;

        // Add force relative to grounded/airborne
        if (grounded)
            rb.AddForce(moveDir * targetSpeed * 10f, ForceMode.Force);
        else
            rb.AddForce(moveDir * targetSpeed * 10f * airMultiplier, ForceMode.Force);
    }

    void Jump()
    {
        // reset vertical velocity for consistent jump
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    void GroundCheck()
    {
        grounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundMask);
    }

    void ClampSpeed()
    {
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        float maxSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed;

        if (flatVel.magnitude > maxSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * maxSpeed;
            rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
        }
    }
}
