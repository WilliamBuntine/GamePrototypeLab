using UnityEngine;
[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]

public class Swinging : MonoBehaviour
{

    private PlayerMove playermove;
    private Rigidbody rb;
    [Header("Input")]
    public KeyCode swingKey = KeyCode.Mouse0;

    [Header("References")]
    public PlayerMove playerController;
    public LineRenderer lr;
    public Transform gunTip, cam, player;
    public LayerMask Grappleable;
    private Vector3 currentGrapplePosition;

    //Advanced rope mechanics
    private float shortestDistance;
    private float minLeeway = 0.2f;
    private float leewayFraction = 0.05f;
    private float adaptiveLeeway;



    [Header("Swinging")]
    public float jointSpring = 2f;
    public float jointDamper = 0.5f;
    public float jointMassScale = 1f;
    private float maxSwingDistance = 25f;
    private Vector3 swingPoint;
    private SpringJoint joint;
    public float maxSpeed;
    public float reelStrength, reelRate;

    [Header("Thrust")]
    public float sideThrust;
    public float upThrust;
    public bool isSwinging = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playermove = GetComponent<PlayerMove>();
        rb = GetComponent<Rigidbody>();
    }

    float GetTangentialSpeed()
    {
        Vector3 ropeDir = GetRopeDir();
        Vector3 v = rb.linearVelocity; // if on Unity 2022/2023 use rb.velocity instead
        Vector3 tangential = Vector3.ProjectOnPlane(v, ropeDir);
        return tangential.magnitude;
    }

    // Update is called once per frame

    bool wHeld, aHeld, sHeld, dHeld, spaceheld;
    void Update()
    {
        if (Input.GetKeyDown(swingKey))
        {
            StartSwing();
            playerController.grappling = true;
        }
        wHeld = Input.GetKey(KeyCode.W);
        aHeld = Input.GetKey(KeyCode.A);
        sHeld = Input.GetKey(KeyCode.S);
        dHeld = Input.GetKey(KeyCode.D);
        spaceheld = Input.GetKey(KeyCode.Space);

        if (Input.GetKeyUp(swingKey))
        {
            StopSwing();
            playerController.grappling = false;
        }

    }

    void LateUpdate()
    {
        DrawRope();
    }
    void FixedUpdate()
    {
        if (spaceheld && isSwinging)
        {
            GrappleReel();
        }

        if (joint == null) return;

        float currentDist = Vector3.Distance(player.position, swingPoint);

        // Ratchet: only ever decrease
        if (currentDist < shortestDistance)
            shortestDistance = currentDist;

        // Adaptive leeway: scales with how close you've reeled in
        float adaptiveLeeway = Mathf.Max(minLeeway, shortestDistance * leewayFraction);

        // Hard minimum for maxDistance so it's always >= minDistance
        float hardMin = joint.minDistance + 0.01f;

        // New cap: closest-ever + adaptive slack
        float targetMax = Mathf.Max(shortestDistance + adaptiveLeeway, hardMin);

        joint.maxDistance = targetMax;
        Vector3 vAll = rb.linearVelocity;
        Vector3 vHoriz = new Vector3(vAll.x, 0f, vAll.z);



        // Advanced rope mechanics
        if (!playermove.grounded)
        {
            if (wHeld)
            {
                ReelUp();

            }
            if (aHeld && vHoriz.magnitude < maxSpeed)
            {
                ReelLeft();
            }
            else if (aHeld && vHoriz.magnitude > maxSpeed)
            {
                ReelLeftSpeed();
            }

            if (sHeld)
            {
                ReelDown();
            }
            if (dHeld && vHoriz.magnitude < maxSpeed)
            {
                ReelRight();
            }
            else if (dHeld && vHoriz.magnitude > maxSpeed)
            {
                ReelRightSpeed();
            }
        }

    }

    void StartSwing()
    {
        RaycastHit hit;
        if (Physics.Raycast(cam.position, cam.forward, out hit, maxSwingDistance, Grappleable))
        {
            swingPoint = hit.point;
            joint = player.gameObject.AddComponent<SpringJoint>();
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = swingPoint;

            float distanceFromPoint = Vector3.Distance(player.position, swingPoint);
            shortestDistance = distanceFromPoint;
            joint.maxDistance = shortestDistance; ;
            joint.minDistance = shortestDistance * 0.25f;

            joint.spring = 80f;
            joint.damper = 25f;
            joint.massScale = 1f;

            lr.positionCount = 2;
            currentGrapplePosition = gunTip.position;
            isSwinging = true;
        }
    }

    void StopSwing()
    {
        lr.positionCount = 0;
        Destroy(joint);
        isSwinging = false;
    }

    void DrawRope()
    {
        if (!joint) return;

        currentGrapplePosition = Vector3.Lerp(currentGrapplePosition, swingPoint, Time.deltaTime * 8f);

        lr.SetPosition(0, gunTip.position);
        lr.SetPosition(1, swingPoint);
    }

    void ReelUp()
    {
        rb.AddForce(player.up * sideThrust, ForceMode.Acceleration);
        isSwinging = true;
    }

    void ReelDown()
    {
        rb.AddForce(-player.up * sideThrust, ForceMode.Acceleration);
        isSwinging = true;
    }

    Vector3 GetRopeDir()
    {
        // From player to grapple point
        return (swingPoint - player.position).normalized;
    }

    Vector3 GetSideDir(bool right)
    {
        Vector3 ropeDir = GetRopeDir();

        // Use camera's right as your intended control direction,
        // BUT remove any component along the rope so it's purely tangential.
        Vector3 intended = right ? cam.right : -cam.right;

        Vector3 sideDir = Vector3.ProjectOnPlane(intended, ropeDir);

        // Safety: if ropeDir is almost parallel to intended, sideDir can be near zero.
        if (sideDir.sqrMagnitude < 0.0001f)
            return Vector3.zero;

        return sideDir.normalized;
    }

    void ReelLeft()
    {
        Vector3 dir = GetSideDir(right: false);
        if (dir != Vector3.zero)
            rb.AddForce(dir * sideThrust, ForceMode.Acceleration);
    }

    void ReelLeftSpeed()
    {
        Vector3 dir = GetSideDir(right: false);
        if (dir != Vector3.zero)
            rb.AddForce(dir * (0.3f * sideThrust), ForceMode.Acceleration);
    }

    void ReelRight()
    {
        Vector3 dir = GetSideDir(right: true);
        if (dir != Vector3.zero)
            rb.AddForce(dir * sideThrust, ForceMode.Acceleration);
    }

    void ReelRightSpeed()
    {
        Vector3 dir = GetSideDir(right: true);
        if (dir != Vector3.zero)
            rb.AddForce(dir * (0.3f * sideThrust), ForceMode.Acceleration);
    }
    
     void GrappleReel()
    {
        float g = Mathf.Abs(Physics.gravity.y);
        float hardMin = joint.minDistance + 0.01f;
        Vector3 toAnchor = (swingPoint - player.position).normalized;
        rb.AddForce(Vector3.up * (g * (1f - 0.5f)), ForceMode.Acceleration); // counteract gravity partially

        rb.AddForce(toAnchor * reelStrength, ForceMode.Acceleration);
        joint.maxDistance = Mathf.Max(hardMin, joint.maxDistance - reelRate * Time.fixedDeltaTime);

    }
}
