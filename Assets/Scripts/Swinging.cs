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

    [Header("Swinging")]
    public float jointSpring = 0f;
    public float jointDamper = 0f;
    public float jointMassScale = 1f;
    public float maxSwingDistance = 25f;
    private Vector3 swingPoint;
    private SpringJoint joint;
    private Rigidbody connectedBody; // NEW
    public float maxSpeed;
    public float reelStrength, reelRate;

    [Header("Thrust")]
    public float sideThrust;
    public float upThrust;
    public bool isSwinging = false;

    // Interpolation smoothing for rope visuals
    [Header("Rope Visuals")]
    public float ropeSmoothSpeed = 8f; // NEW

    void Start()
    {
        playermove = GetComponent<PlayerMove>();
        rb = GetComponent<Rigidbody>();
    }

    bool wHeld, aHeld, sHeld, dHeld, spaceheld;
    void Update()
    {
        if (Input.GetKeyDown(swingKey))
        {
            StartSwing();
            playerController.grappling = true;  //Assisting PlayerMove script to know when grappling
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

        if (currentDist < shortestDistance)
            shortestDistance = currentDist;

        float adaptiveLeeway = Mathf.Max(minLeeway, shortestDistance * leewayFraction);
        float hardMin = joint.minDistance + 0.01f;
        float targetMax = Mathf.Max(shortestDistance + adaptiveLeeway, hardMin);

        joint.maxDistance = targetMax;

        Vector3 vAll = rb.linearVelocity;
        Vector3 vHoriz = new Vector3(vAll.x, 0f, vAll.z);

        if (!playermove.grounded)
        {
            if (wHeld) ReelUp();
            if (aHeld && vHoriz.magnitude < maxSpeed) ReelLeft();
            else if (aHeld) ReelLeftSpeed();

            if (sHeld) ReelDown();
            if (dHeld && vHoriz.magnitude < maxSpeed) ReelRight();
            else if (dHeld) ReelRightSpeed();
        }

        // 🔹 NEW: Apply pull force to grappled rigidbody if it has one
        if (connectedBody != null)
        {
            Vector3 pullDir = (gunTip.position - connectedBody.position).normalized;
            connectedBody.AddForce(pullDir * reelStrength * 0.5f, ForceMode.Force);
        }
    }

    void StartSwing()
    {
        RaycastHit hit;
        if (Physics.Raycast(cam.position, cam.forward, out hit, maxSwingDistance, Grappleable))
        {
            swingPoint = hit.point;
            connectedBody = hit.rigidbody; // NEW (save rigidbody if any)

            joint = player.gameObject.AddComponent<SpringJoint>();
            joint.autoConfigureConnectedAnchor = false;

            if (connectedBody != null) // attach to moving object
            {
                joint.connectedBody = connectedBody; // NEW
                joint.connectedAnchor = hit.transform.InverseTransformPoint(hit.point); // keep local offset
            }
            else
            {
                joint.connectedAnchor = swingPoint;
            }

            float distanceFromPoint = Vector3.Distance(player.position, swingPoint);
            shortestDistance = distanceFromPoint;
            joint.maxDistance = shortestDistance;
            joint.minDistance = shortestDistance * 0.25f;

            joint.spring = 80f;
            joint.damper = 25f;
            joint.massScale = 1f;

            lr.positionCount = 2;
            currentGrapplePosition = gunTip.position;
            isSwinging = true;
        }
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

    void ReelUp()
    {
        

        rb.AddForce(player.up * sideThrust, ForceMode.Acceleration);
        isSwinging = true;
    }

    void ReelDown()
    {
        rb.AddForce(-player.up * sideThrust, ForceMode.Acceleration);
    }

    void ReelLeft()
    {
        Vector3 r = player.position - swingPoint;
        Vector3 rHoriz = Vector3.ProjectOnPlane(r, Vector3.up);
        if (rHoriz.sqrMagnitude < 1e-6f) return;

        Vector3 tangentCW = -Vector3.Cross(Vector3.up, rHoriz).normalized;
        rb.AddForce(tangentCW * sideThrust, ForceMode.Acceleration);
    }

    void ReelRight()
    {
        Vector3 r = player.position - swingPoint;
        Vector3 rHoriz = Vector3.ProjectOnPlane(r, Vector3.up);
        if (rHoriz.sqrMagnitude < 1e-6f) return;

        Vector3 tangentCCW = Vector3.Cross(Vector3.up, rHoriz).normalized;
        rb.AddForce(tangentCCW * sideThrust, ForceMode.Acceleration);
    }

    void StopSwing()
    {
        lr.positionCount = 0;
        Destroy(joint);
        connectedBody = null; // NEW
        isSwinging = false;
    }

    void DrawRope()
    {
        if (!joint) return;

        // Find current rope end
        Vector3 ropeEnd = connectedBody
            ? connectedBody.transform.TransformPoint(joint.connectedAnchor) // if swinging from rigidbody
            : swingPoint; // if swinging from static point

        // Instantly set rope positions (no smoothing)
        lr.SetPosition(0, gunTip.position);
        lr.SetPosition(1, ropeEnd);
    }




    void ReelUp() => rb.AddForce(player.up * upThrust, ForceMode.Acceleration);
    void ReelDown() => rb.AddForce(-player.up * (0.5f * upThrust), ForceMode.Acceleration);
    void ReelLeft() => rb.AddForce(-player.right * sideThrust, ForceMode.Acceleration);
    void ReelLeftSpeed() => rb.AddForce(-player.right * (0.3f * sideThrust), ForceMode.Acceleration);
    void ReelRight() => rb.AddForce(player.right * sideThrust, ForceMode.Acceleration);
    void ReelRightSpeed() => rb.AddForce(player.right * (0.3f * sideThrust), ForceMode.Acceleration);

    void GrappleReel()
    {
        float g = Mathf.Abs(Physics.gravity.y);
        float hardMin = joint.minDistance + 0.01f;
        Vector3 toAnchor = (swingPoint - player.position).normalized;
        rb.AddForce(Vector3.up * (g * (1f - 0.5f)), ForceMode.Acceleration);
        rb.AddForce(toAnchor * reelStrength, ForceMode.Acceleration);
        joint.maxDistance = Mathf.Max(hardMin, joint.maxDistance - reelRate * Time.fixedDeltaTime);
    }
}
