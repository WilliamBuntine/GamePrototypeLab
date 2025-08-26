using UnityEngine;

public class Swinging : MonoBehaviour
{

    [Header("Input")]
    public KeyCode swingKey = KeyCode.Mouse0;

    [Header("References")]
    public PlayerMove playerController;
    public LineRenderer lr;
    public Transform gunTip, cam, player;
    public LayerMask Grappleable;
    private Vector3 currentGrapplePosition;


    [Header("Swinging")]
    public float jointSpring = 2f;   
    public float jointDamper = 0.5f;  
    public float jointMassScale = 1f; 

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
        if (Input.GetKeyDown(swingKey))
        {
            StartSwing();
            playerController.grappling = true;
        } 
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

            joint.maxDistance = distanceFromPoint * 0.8f;
            joint.minDistance = distanceFromPoint * 0.25f;

            joint.spring = 4.5f;
            joint.damper = 7f;
            joint.massScale = 4.5f;

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
