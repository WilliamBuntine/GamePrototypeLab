using UnityEngine;

public class WallDetector : MonoBehaviour
{
    [HideInInspector] public bool nearWall = false;
    [HideInInspector] public Vector3 wallNormal;

    private Rigidbody rb;
    private PlayerMove playerMove;

    public SphereCollider sphere;
    public float baseRadius = 0.5f;
    public float maxRadius = 0.8f;
    public float raycastDistance = 2f; // how far to probe for walls

    void Start()
    {
        rb = GetComponentInParent<Rigidbody>();
        playerMove = GetComponentInParent<PlayerMove>();
    }

    void Update()
    {
        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        float speed = horizontalVelocity.magnitude;
        sphere.radius = speed > 7f ? maxRadius : baseRadius;
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Jumpable"))
            return;

        nearWall = true;

        // Try to raycast toward the collider from the player's position.
        Vector3 direction = (other.transform.position - transform.position).normalized;

        // Perform a raycast to find the surface normal
        if (Physics.Raycast(transform.position, direction, out RaycastHit hit, raycastDistance))
        {
            if (hit.collider == other)
            {
                wallNormal = hit.normal;
                return;
            }
        }

        // Fallback: SphereCast in case the wall is close but ray misses
        if (Physics.SphereCast(transform.position, 0.1f, direction, out hit, raycastDistance))
        {
            if (hit.collider == other)
            {
                wallNormal = hit.normal;
                return;
            }
        }

        // If both fail, clear the wallNormal but keep nearWall = true
        wallNormal = Vector3.zero;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Jumpable"))
            return;

        nearWall = false;
        wallNormal = Vector3.zero;
    }
}
