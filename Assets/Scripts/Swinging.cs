using UnityEngine;

public class Swinging : MonoBehaviour
{

    [Header("Input")]
    public KeyCode swingKey = KeyCode.Mouse0;

    [Header("References")]
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
    private float maxSwingDistance = 25f;
    private Vector3 swingPoint;

    private SpringJoint joint;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    

    void Update()
    {
        if (Input.GetKeyDown(swingKey)) StartSwing();
        if (Input.GetKeyUp(swingKey)) StopSwing();

    }

    void LateUpdate()
    {
        DrawRope();
    }
    void FixedUpdate()
    {
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
        }
    }

    void StopSwing()
    {
        lr.positionCount = 0;
        Destroy(joint);

    }

    void DrawRope()
    {
        if (!joint) return;

        currentGrapplePosition = Vector3.Lerp(currentGrapplePosition, swingPoint, Time.deltaTime * 8f);

        lr.SetPosition(0, gunTip.position);
        lr.SetPosition(1, swingPoint);
    }
}
