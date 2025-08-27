using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class PlayerMove : MonoBehaviour
{
    [Header("Movement Settings")]
    public bool grappling = false;
    public float walkSpeed = 7f;
    public float sprintSpeed = 12f;
    public float jumpForce = 7f;
    public float groundFriction = 12f;
    public float airControl = 0.5f;
    public float groundCheckDistance = 0.5f;
    public LayerMask groundMask;

    [Header("Slide Settings")]

    [Header("Wall Jump Settings")]
    public WallDetector wallDetector;
    public float wallPushAwayForce = 5f;
    public float wallPushUpForce = 3f;

    [Header("Look Settings")]
    public float mouseSensitivity = 100f;
    public Transform playerCamera;

    private Rigidbody rb;
    private float xRotation = 0f;
    public bool grounded {get; private set;}
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
                Jump(Vector3.up);
            }
            else if (wallDetector != null && wallDetector.nearWall)
            {
                WallJump();
            }
        }
    }

    void FixedUpdate()
    {
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

        if (inputDir.sqrMagnitude > 0.01f)
        {
            if (grounded || (grappling && wallDetector != null && wallDetector.nearWall))
            {
                Vector3 desiredVel = inputDir * targetSpeed;
                Vector3 forceDir = (desiredVel - new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z)) * 10f;
                rb.AddForce(forceDir, ForceMode.Force);
            }
            else
            {
                Vector3 horizontalVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
                Vector3 airForce = inputDir * targetSpeed * airControl;
                rb.AddForce(airForce, ForceMode.Acceleration);
            }
        }
        else if (grounded)
        {
            // friction only on ground
            Vector3 horizontalVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            Vector3 frictionForce = -horizontalVel * groundFriction;
            rb.AddForce(frictionForce, ForceMode.Acceleration);
        }

        //ClampSpeed();
    }


    void Jump(Vector3 direction)
    {
        // reset vertical velocity
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(direction * jumpForce, ForceMode.Impulse);
    }

    void WallJump()
    {
        if (wallDetector == null || wallDetector.wallNormal == Vector3.zero) return;

        Vector3 jumpDir = wallDetector.wallNormal * wallPushAwayForce + Vector3.up * wallPushUpForce;
        Jump(jumpDir);
    }

    void GroundCheck()
    {
        grounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundMask);
    }

    void ClampSpeed()
    {
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        float maxSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed;

        if (grounded || (grappling && wallDetector != null && wallDetector.nearWall))
        {
            if (flatVel.magnitude > maxSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * maxSpeed;
                rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
            }
        }
    }

}
