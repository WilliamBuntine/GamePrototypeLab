using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class PlayerMove : MonoBehaviour
{
    [Header("Movement Settings")]
    public bool grappling = false;
    public float walkSpeed = 7f;
    public float sprintSpeed = 12f;
    public float jumpForce = 7f;
    public float groundFriction = 12f;   // higher = stops faster on ground
    public float airControl = 0.5f;      // % of control in air
    public float groundCheckDistance = 0.5f;
    public LayerMask groundMask;

    [Header("Look Settings")]
    public float mouseSensitivity = 100f;
    public Transform playerCamera;

    private Rigidbody rb;
    private float xRotation = 0f;
    private bool grounded;
    private Vector3 inputDir;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        if (!GetComponent<CapsuleCollider>())
            Debug.LogWarning("Player should have a CapsuleCollider for proper grounding.");

        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        HandleLook();
        GroundCheck();

        // Collect input here (for FixedUpdate use)
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");
        inputDir = (transform.right * moveX + transform.forward * moveZ).normalized;

        if (Input.GetButtonDown("Jump"))
        {
            if (grounded)
            {
                Jump();
            }
        }
    }

    void FixedUpdate()
    {
        if (grounded)
            HandleMovement();
    }

    void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        transform.Rotate(Vector3.up * mouseX);

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    void HandleMovement()
    {
        float targetSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed;

        // If input pressed → accelerate
        if (inputDir.sqrMagnitude > 0.01f)
        {
            Vector3 forceDir = inputDir * targetSpeed * 10f;
            if (grounded)
                rb.AddForce(forceDir, ForceMode.Force);
            else
                rb.AddForce(forceDir * airControl, ForceMode.Force);
        }
        // If no input and grounded → apply friction to stop sliding
        else if (grounded)
        {
            Vector3 horizontalVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            Vector3 frictionForce = -horizontalVel * groundFriction;
            rb.AddForce(frictionForce, ForceMode.Acceleration);
        }

        ClampSpeed();
    }

    void Jump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z); // reset Y
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
