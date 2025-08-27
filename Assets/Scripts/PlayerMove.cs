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
    public KeyCode slideKey = KeyCode.LeftControl;
    public float slideFriction = 0.5f;  // much lower than groundFriction
    public float slideDuration = 1.0f;
    private bool sliding = false;
    private float slideTimer;

    [Header("Wall Jump Settings")]
    public WallDetector wallDetector;
    public float wallPushAwayForce = 5f;
    public float wallPushUpForce = 3f;

    [Header("Look Settings")]
    public float mouseSensitivity = 100f;
    public Transform playerCamera;

    private Rigidbody rb;
    private float xRotation = 0f;
    public bool grounded { get; private set; }
    private Vector3 inputDir;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        HandleLook();
        GroundCheck();

        // Slide input
        if (Input.GetKeyDown(slideKey) && !sliding)
        {
            StartSlide();
        }

        if (sliding)
        {
            slideTimer -= Time.deltaTime;
            if (slideTimer <= 0 || rb.linearVelocity.magnitude < 1f)
            {
                StopSlide();
            }
        }

        // Collect movement input ONLY if not sliding
        if (!sliding)
        {
            float moveX = Input.GetAxisRaw("Horizontal");
            float moveZ = Input.GetAxisRaw("Vertical");
            inputDir = (transform.right * moveX + transform.forward * moveZ).normalized;
        }
        else
        {
            inputDir = Vector3.zero; // no player steering
        }

        // Jump input
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

        if (sliding)
        {
            // Apply reduced friction instead of movement
            Vector3 horizontalVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            Vector3 frictionForce = -horizontalVel * slideFriction;
            rb.AddForce(frictionForce, ForceMode.Acceleration);
            return;
        }

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
                Vector3 airForce = inputDir * targetSpeed * airControl;
                rb.AddForce(airForce, ForceMode.Acceleration);
            }
        }
        else if (grounded)
        {
            // normal ground friction
            Vector3 horizontalVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            Vector3 frictionForce = -horizontalVel * groundFriction;
            rb.AddForce(frictionForce, ForceMode.Acceleration);
        }
    }

    void Jump(Vector3 direction)
    {
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

    void StartSlide()
    {
        sliding = true;
        slideTimer = slideDuration;
    }

    void StopSlide()
    {
        sliding = false;
    }
}
