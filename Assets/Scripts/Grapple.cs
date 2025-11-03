using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GrappleBoost : MonoBehaviour
{
    private Rigidbody rb;

    [Header("Input")]
    public KeyCode grappleKey = KeyCode.Mouse1;

    [Header("References")]
    public Transform gunTip;
    public Transform cam;
    public LineRenderer lr;
    public LayerMask grappleable;

    [Header("Grapple Settings")]
    public float maxGrappleDistance = 80f;
    public float grappleBoostStrength = 5f;
    public float ropePullDuration = 0.2f;
    public float grappleAccelTimeCap = 1f;

    [Header("Cooldown")]
    public float grappleCooldown = 0.1f;
    private float cooldownTimer;

    private Vector3 grapplePoint;
    private Vector3 currentGrapplePosition;
    private bool isGrappling;
    private float grappleTimer;
    private bool grappleDinged;

    private Rigidbody connectedBody; // NEW: optional rigidbody being grappled

    public AudioSource audioSource; // Audio source for playing sounds
    public AudioClip grappleSound; // Sound to play when grappling
    public AudioClip grappleReadySound; // Sound to play when grappling
    public AudioClip grappleOffSound; // Sound to play when grappling

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        cooldownTimer = 0f;
        grappleDinged = true;
    }

    void Update()
    {
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;
        
        if (cooldownTimer <= 0f && grappleDinged == false)
        {
            PlayGrappleDingSound();
        }

        if (Input.GetKeyDown(grappleKey) && cooldownTimer <= 0f)
        {
            TryStartGrapple();
        }

        if (Input.GetKeyUp(grappleKey) && isGrappling)
        {
            StopGrapple();
        }

        if (isGrappling)
        {
            DrawRope();
        }
    }

    void FixedUpdate()
    {
        if (!isGrappling) return;

        grappleTimer += Time.fixedDeltaTime;
        if (grappleTimer > grappleAccelTimeCap ) { grappleTimer = grappleAccelTimeCap; }


        // Pull player toward grapple point
            Vector3 dirToPoint;
            if (connectedBody != null)
            {
                // Pull toward moving object
                dirToPoint = (connectedBody.position - transform.position).normalized;
            }
            else 
            {
                dirToPoint = (grapplePoint - transform.position).normalized;
            }
            
            float playerForce = (0.5f + grappleTimer/4) * grappleBoostStrength;
            rb.AddForce(dirToPoint * playerForce, ForceMode.VelocityChange);

            // Pull the object if it has a rigidbody
            if (connectedBody != null && !connectedBody.isKinematic)
            {
                Vector3 dirToPlayer = (transform.position - connectedBody.position).normalized;
                connectedBody.AddForce(dirToPlayer * grappleBoostStrength * 0.5f, ForceMode.VelocityChange);
            }
        //stop grapple when player is too close to grapple point
          if (Vector3.Distance(grapplePoint, gunTip.position) < 3f)
          {
            StopGrapple();
          }
        }

    void TryStartGrapple()
    {
        RaycastHit hit;
        if (Physics.Raycast(cam.position, cam.forward, out hit, maxGrappleDistance, grappleable))
        {
            // Play grapple sound
            PlayGrappleReelSound();

            grapplePoint = hit.point;
            connectedBody = hit.rigidbody; // NEW: save rigidbody if any
            currentGrapplePosition = gunTip.position;
            isGrappling = true;
            grappleTimer = 0f;

            lr.positionCount = 2;

            // Start cooldown
        }
    }

    void StopGrapple()
    {
        isGrappling = false;
        connectedBody = null; // NEW
        lr.positionCount = 0;

        cooldownTimer = grappleCooldown;
        grappleDinged = false;

        audioSource.PlayOneShot(grappleOffSound, 0.3f);
    }

    void DrawRope()
    {
        if (!isGrappling) return;

        // Directly update rope line (no smoothing)
        Vector3 targetPoint = connectedBody != null ? connectedBody.position : grapplePoint;
        lr.SetPosition(0, gunTip.position);
        lr.SetPosition(1, targetPoint);
    }

    void PlayGrappleReelSound()
    {
        audioSource.PlayOneShot(grappleSound);
    }

    void PlayGrappleDingSound()
    {
        audioSource.PlayOneShot(grappleReadySound, 0.3f);
        grappleDinged = true;
    }
}
