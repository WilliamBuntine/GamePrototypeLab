using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class PlayerMove : MonoBehaviour
{
    private Swinging swinging;
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
    public float slideFrictionAdjustment = 0.2f;
    public float slideDuration = 1f;
    public float slideHeight = 0.5f;       // how short the collider gets while sliding
    public KeyCode slideKey = KeyCode.LeftControl;
    private bool isSliding = false;
    private float slideTimer = 0f;
    private float slideRefresh = 0f;
    private float slideCooldown = 2f;


    [Header("Wall Jump Settings")]
    public WallDetector wallDetector;
    public float wallPushAwayForce = 5f;
    public float wallPushUpForce = 3f;

    public float walljumpmultiplier;

    public float wallJumpCooldown;
    private float wallJumpTimer = 0f;
    private bool hasWallJumped;

    [Header("Look Settings")]
    public float mouseSensitivity = 100f;
    public Transform playerCamera;
    public float cameraSlideHeightAdjust = -0.5f;

    private Rigidbody rb;
    private CapsuleCollider capsule;
    private float xRotation = 0f;
    public bool grounded { get; private set; }
    private Vector3 inputDir;

    // store original collider + camera info
    private float originalColliderHeight;
    private Vector3 originalColliderCenter;
    private Vector3 originalCameraLocalPos;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();
        rb.freezeRotation = true;

        originalColliderHeight = capsule.height;
        originalColliderCenter = capsule.center;
        originalCameraLocalPos = playerCamera.localPosition;

        Cursor.lockState = CursorLockMode.Locked;
        swinging = GetComponent<Swinging>();

    }

    bool spaceHeld;
    void Update()
    {
        spaceHeld = Input.GetKey(KeyCode.Space);

        HandleLook();
        GroundCheck();

        slideRefresh -= Time.deltaTime;

        if(hasWallJumped)
        {
            wallJumpTimer -= Time.deltaTime;
            if(wallJumpTimer <= 0f)
            {
                print("reset wall jump cooldown");
                hasWallJumped = false;
                wallJumpTimer = wallJumpCooldown;
            }
        }


        // start slide
        if (Input.GetKeyDown(slideKey) && !isSliding && slideRefresh <= 0f)
        {
            StartSlide();
        }

        // end slide (timer or key up)
        if (isSliding)
        {
            slideTimer -= Time.deltaTime;
            if (slideTimer <= 0f || Input.GetKeyUp(slideKey))
            {
                StopSlide();
            }
        }

        // Collect input here (for FixedUpdate use)
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");
        inputDir = (transform.right * moveX + transform.forward * moveZ).normalized;

        if (Input.GetButtonDown("Jump"))
        {
            if (grounded && !swinging.isSwinging)
            {
                Jump(Vector3.up);
            }
            else if (wallDetector != null && wallDetector.nearWall && !hasWallJumped && !grappling)
            {
                WallJump();

            }
            
        }
    }

    void FixedUpdate()
    {
        if (!isSliding) // disable control while sliding
        {
            HandleMovement();
        }
        else
        {
            HandleSlideMovement();
        }
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
    }

    void HandleSlideMovement()
    {
        // low friction → keeps momentum
        Vector3 horizontalVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        Vector3 slideFriction = -horizontalVel * groundFriction * slideFrictionAdjustment;
        rb.AddForce(slideFriction, ForceMode.Acceleration);
    }

    void StartSlide()
    {
        isSliding = true;
        slideTimer = slideDuration;
        slideRefresh = slideCooldown;

        // shrink collider
        capsule.height = slideHeight;
        capsule.center = new Vector3(originalColliderCenter.x, slideHeight / 2f, originalColliderCenter.z);

        // lower camera
        playerCamera.localPosition += new Vector3(0f, cameraSlideHeightAdjust, 0f);

        // small forward impulse
        rb.AddForce(transform.forward * 2f, ForceMode.Impulse);
    }

    void StopSlide()
    {
        isSliding = false;

        // restore collider
        capsule.height = originalColliderHeight;
        capsule.center = originalColliderCenter;

        // restore camera
        playerCamera.localPosition = originalCameraLocalPos;
    }

    void Jump(Vector3 direction)
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(direction * jumpForce, ForceMode.Impulse);

        if (isSliding) StopSlide(); // jump cancels slide
    }

    void WallJump()
    {
        hasWallJumped = true;
        wallJumpTimer = wallJumpCooldown;
        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        float speed = horizontalVelocity.magnitude;
        if (wallDetector == null || wallDetector.wallNormal == Vector3.zero) return;
        if (speed <= 5f)
        {
            Vector3 jumpDir = wallDetector.wallNormal * wallPushAwayForce + Vector3.up * wallPushUpForce;
            Jump(jumpDir);
        }
        else if (speed > 5f)
        {
            Vector3 jumpDir = wallDetector.wallNormal * (speed * walljumpmultiplier) + Vector3.up * wallPushUpForce;
            Jump(jumpDir);
        }
    }

    void GroundCheck()
    {
        grounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundMask);
    }
}
