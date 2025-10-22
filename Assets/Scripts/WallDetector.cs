using UnityEngine;

public class WallDetector : MonoBehaviour
{
    [HideInInspector] public bool nearWall = false;
    [HideInInspector] public Vector3 wallNormal;
    Rigidbody rb;
    PlayerMove playerMove;
    public SphereCollider sphere;
    public float baseRadius = 0.5f;
    public float maxRadius = 0.8f;
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Jumpable"))
        {
            nearWall = true;

            // Direction from player to collider center
            Vector3 dir = (transform.position - other.ClosestPointOnBounds(transform.position)).normalized;
            wallNormal = dir;
        }
    }
    void Start()
    {
        rb = GetComponentInParent<Rigidbody>();
        playerMove = GetComponentInParent<PlayerMove>();

    }

    void Update()
    {
        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        float speed = horizontalVelocity.magnitude;
        if (speed > 7f)
        {
            sphere.radius = maxRadius; // because radius * 2 ≈ diameter in world scale

        } else sphere.radius = baseRadius;

    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Jumpable"))
        {
            nearWall = false;
            wallNormal = Vector3.zero;
        }
    }


}
