using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class PlayerMove : MonoBehaviour
{

    public Course activeCourse;
    public KeyCode resetKey = KeyCode.G;

    [Header("Movement Settings")]

    float airbourneTimer = 0f;
    public bool grappling = false;
    public float walkSpeed = 7f;
    float currentSpeed;
    public float sprintSpeed = 12f;

    bool isSprinting = false;
    public float jumpForce = 7f;
    public float groundFriction = 10f;
    public float airControl = 0.5f;
    public float maxAirSpeed;
    public float groundCheckDistance = 0.5f;
    public LayerMask groundMask;

    [Header("Slide Settings")]
    public float slideFrictionMult = 1f;
    public float slideDuration = 10f;
    public float slideHeight = 0.5f;       // how short the collider gets while sliding
    public KeyCode slideKey = KeyCode.LeftControl;
    private bool isSliding = false;


    [Header("Wall Jump Settings")]
    public WallDetector wallDetector;
    public float wallPushAwayForce = 5f;
    public float wallPushUpForce = 3f;
    public bool wallRunningEnabled;
    public float minAngle = 70f;
    public float maxAngle = 110f;

    [Header("Look Settings")]
    public float mouseSensitivity = 100f;
    public Transform playerCamera;
    public float cameraSlideHeightAdjust = -0.5f;

    [Header("Sound Settings")]
    public AudioSource audioSource; // Audio source for playing sounds
    public AudioClip speedSound; // Sound to play when at high speed
    public AudioClip Falling;

    public float baseInterval = 0.5f; // seconds between steps when walking
    float minInterval = 0.15f; // minimum interval between steps

    public AudioClip Jumping; // Sound to play when jumping

    public AudioClip Walking; // Sound to play when walking
    private float footstepInterval; // Interval between footstep sounds
    private float footstepTimer = 0f; // Timer to track footstep intervals
    private float speedSoundCooldown = 0f; // timer to track cooldown
    public float speedSoundInterval = 15f; // seconds between chances


    private Rigidbody rb;
    private CapsuleCollider capsule;
    private float xRotation = 0f;
    public bool grounded { get; private set; }
    private bool walkingSoundPlaying = false;
    private Vector3 inputDir;

    // store original collider + camera info
    private float originalColliderHeight;
    private Vector3 originalColliderCenter;
    private Vector3 originalCameraLocalPos;
    private Swinging swinging;



    void Start()
    {
        rb = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();
        rb.freezeRotation = true;

        originalColliderHeight = capsule.height;
        originalColliderCenter = capsule.center;
        originalCameraLocalPos = playerCamera.localPosition;

        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        speedSoundCooldown -= Time.deltaTime;
        
        


        SetStepInterval();

        footstepTimer += Time.deltaTime;
        if(CheckWalkingandGrounded())
        {
            Walksound();
        }
        HandleLook();
        GroundCheck();

        Vector3 horizontalVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        float SpeedHoriz = horizontalVel.magnitude;

        if (SpeedHoriz > 30f && speedSoundCooldown <= 0f)
        {
            TryPlaySpeedSound();
        }


        // start slide
        if ((Input.GetKeyDown(slideKey)))
        {
            StartSlide();
        }
        if (Input.GetKey(KeyCode.LeftShift)) { isSprinting = true; }
        // end slide (timer or key up)
        if (isSliding)
        {
            if (Input.GetKeyUp(slideKey))
            {
                StopSlide();
            }
        }

        // Collect input here (for FixedUpdate use)
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");
        
        inputDir = (transform.right * moveX + transform.forward * moveZ).normalized;
        currentSpeed = walkSpeed;
       

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

        if (Input.GetKeyDown(resetKey))
        {
            activeCourse.CancelCourse();

            transform.position = activeCourse.respawnPoint.transform.position;

            transform.rotation = activeCourse.respawnPoint.transform.rotation;

            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
                rb.linearVelocity = Vector3.zero;
        }

    }

    void FixedUpdate()
    {
        if (!grounded)
        {
            airbourneTimer += Time.deltaTime;
        }        
        if(!grounded && airbourneTimer > 2f)
        {
            FallingSound();
            if(grounded)
            {
                Landsound();
            }
        }

        if (isSliding)
        {
            slideFrictionMult += Time.fixedDeltaTime / slideDuration;
            slideFrictionMult = Mathf.Clamp01(slideFrictionMult);
        }
        else
        {
            slideFrictionMult = 1f; // normal movement when not sliding
        }
        
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
        float targetSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : currentSpeed;
        Vector3 horizontalVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        if (isSliding)
        {
            // Sliding movement — preserve existing momentum, just add a gentle input-based push
            if (inputDir.sqrMagnitude > 0.01f)
            {
                Vector3 slideForce = inputDir * targetSpeed * 0.8f; // smaller than normal movement
                rb.AddForce(slideForce, ForceMode.Force);
            }

            // Gradually restoring friction
            Vector3 frictionForce = -horizontalVel * (groundFriction * slideFrictionMult);
            rb.AddForce(frictionForce, ForceMode.Acceleration);
            return; // skip the rest while sliding
        }

        // Non-sliding movement
        if (inputDir.sqrMagnitude > 0.01f)
        {
            if (grounded || (grappling && wallDetector != null && wallDetector.nearWall && wallRunningEnabled))
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
            // friction only on ground when idle
            horizontalVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            Vector3 frictionForce = -horizontalVel * (groundFriction * slideFrictionMult);
            rb.AddForce(frictionForce, ForceMode.Acceleration);
        }
    }


    void StartSlide()
    {
        isSliding = true;
        slideFrictionMult = 0f;

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
        slideFrictionMult = 1f;


        // restore collider
        capsule.height = originalColliderHeight;
        capsule.center = originalColliderCenter;

        // restore camera
        playerCamera.localPosition = originalCameraLocalPos;
    }

    void Jump(Vector3 direction)
    {
        // Preserve current horizontal velocity
        Vector3 horizontalVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        // Set the Rigidbody velocity to current horizontal + jump vertical
        rb.linearVelocity = horizontalVel + direction * jumpForce;
        JumpSound();
        // Stop slide if jumping out of it
        // if (isSliding) StopSlide();
    }


    void WallJump()
    {
        if (wallDetector == null || wallDetector.wallNormal == Vector3.zero) return;

        // Angle between wall normal and vertical
        float wallAngle = Vector3.Angle(Vector3.up, wallDetector.wallNormal);
        print(wallAngle);

        if (wallAngle < minAngle || wallAngle > maxAngle) return;

        Vector3 jumpDir = wallDetector.wallNormal * wallPushAwayForce + Vector3.up * wallPushUpForce;
        Jump(jumpDir);
    }

    void GroundCheck()
    {
        grounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundMask);
    }

    void TryPlaySpeedSound()
    {
        audioSource.PlayOneShot(speedSound);
        speedSoundCooldown = speedSoundInterval;
        UnityEngine.Debug.Log("Speed sound played, cooldown started");
    }

    void JumpSound()
    {
        audioSource.PlayOneShot(Jumping, 0.25f);
    }
    void Landsound()
    {
        audioSource.pitch = 1.5f;

        audioSource.PlayOneShot(Jumping);
    }

    void Walksound()
    {
        if (footstepTimer >= footstepInterval)
        {
            audioSource.pitch = Random.Range(0.8f, 1.2f);
            audioSource.PlayOneShot(Walking, 0.5f);
            UnityEngine.Debug.Log("Playing walking sound");
            footstepTimer = 0f;

        }



        return;
    }

    bool CheckWalkingandGrounded()
    {
        Vector3 horizontalVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        float SpeedHoriz = horizontalVel.magnitude;

        if (SpeedHoriz > 1f && grounded && !isSliding)
        {
            walkingSoundPlaying = true;
        }
        else
        {
            walkingSoundPlaying = false;
            footstepTimer = 0f; // reset timer when not walking
        }


        return walkingSoundPlaying;
    }

    void SetStepInterval()
    {
        float horizontalSpeed = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z).magnitude;


        // Adjust interval based on speed (faster speed = smaller interval)
        // Clamp to avoid going too crazy fast or too slow
        if (isSprinting)
        {
            footstepInterval = baseInterval;
        }
        else if (!isSprinting)
        {
            footstepInterval = minInterval;
        }
    }
    
    void FallingSound()
    {
        audioSource.PlayOneShot(Falling);
    }

}

