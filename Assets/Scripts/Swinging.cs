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
    private float maxSwingSpeed = 5f;

    [Header("Thrust")]
    public float sideThrust;
    public float upThrust;
    public bool isSwinging = false;

    [Header("Grapple Reel")]
    public float reelStrength = 6f;       // how hard to pull toward the point (m/s)

    public float reelRate = 10f;         // fraction of distance to reel in per second
    private bool yankRequested;           // set on key down, consumed in FixedUpdate
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playermove = GetComponent<PlayerMove>();
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    
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
        if (isSwinging && spaceheld)
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

        // Advanced rope mechanics
        if (!playermove.grounded)
        {
            Vector3 r = player.position - swingPoint;
            Vector3 rHoriz = Vector3.ProjectOnPlane(r, Vector3.up);
            float rLen = rHoriz.magnitude;
            if (wHeld)
            {
                ReelUp();
                
            }
            
            if (sHeld)
            {
                ReelDown();
            }
            

            if (rLen > 0.001f)
            {
                Vector3 tangentCCW = Vector3.Cross(Vector3.up, rHoriz).normalized;
                Vector3 tangentCW = -tangentCCW;
                if (aHeld && rb.linearVelocity.magnitude < maxSwingSpeed)
                {
                    ReelRight();
                    
                }
                
                if (dHeld && rb.linearVelocity.magnitude < maxSwingSpeed)
                {
                    ReelLeft();
                    
                }
                
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
        isSwinging = false;
    }

    void DrawRope()
    {
        if (!joint) return;

        currentGrapplePosition = Vector3.Lerp(currentGrapplePosition, swingPoint, Time.deltaTime * 8f);

        lr.SetPosition(0, gunTip.position);
        lr.SetPosition(1, swingPoint);
    }
}
