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

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        cooldownTimer = 0f;
    }

    void Update()
    {
        // Countdown cooldown
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

        if (grappleTimer < ropePullDuration)
        {
            // Apply force toward grapple point
            Vector3 dir = (grapplePoint - transform.position).normalized;
            rb.AddForce(dir * grappleBoostStrength, ForceMode.VelocityChange);
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
            grapplePoint = hit.point;
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
        lr.positionCount = 0;
    }

    void DrawRope()
    {
        currentGrapplePosition = Vector3.Lerp(currentGrapplePosition, grapplePoint, Time.deltaTime * 10f);
        lr.SetPosition(0, gunTip.position);
        lr.SetPosition(1, currentGrapplePosition);
    }
}
