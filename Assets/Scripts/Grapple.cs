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
    public float maxGrappleDistance = 40f;
    public float grappleBoostStrength = 30f;
    public float ropePullDuration = 0.2f;

    [Header("Cooldown")]
    public float grappleCooldown = 2f;
    private float cooldownTimer;

    private Vector3 grapplePoint;
    private Vector3 currentGrapplePosition;
    private bool isGrappling;
    private float grappleTimer;

    private Rigidbody connectedBody; // NEW: optional rigidbody being grappled

    public AudioSource audioSource; // Audio source for playing sounds
    public AudioClip grappleSound; // Sound to play when grappling

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        cooldownTimer = 0f;
    }

    void Update()
    {
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;

        if (Input.GetKeyDown(grappleKey) && cooldownTimer <= 0f)
        {
            TryStartGrapple();
        }

        if (Input.GetKeyUp(grappleKey))
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

        // Pull player toward grapple point
        if (grappleTimer < ropePullDuration)
        {
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

            rb.AddForce(dirToPoint * grappleBoostStrength, ForceMode.VelocityChange);

            // Pull the object if it has a rigidbody
            if (connectedBody != null && !connectedBody.isKinematic)
            {
                Vector3 dirToPlayer = (transform.position - connectedBody.position).normalized;
                connectedBody.AddForce(dirToPlayer * grappleBoostStrength * 0.5f, ForceMode.VelocityChange);
            }
        }
        else
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
            cooldownTimer = grappleCooldown;
        }
    }

    void StopGrapple()
    {
        isGrappling = false;
        connectedBody = null; // NEW
        lr.positionCount = 0;
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
        audioSource.clip = grappleSound;
        audioSource.Play();
    }
}
