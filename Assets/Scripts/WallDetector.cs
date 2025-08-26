using UnityEngine;

public class WallDetector : MonoBehaviour
{
    [HideInInspector] public bool nearWall = false;
    [HideInInspector] public Vector3 wallNormal;

    private void OnTriggerStay(Collider other)
    {
        // Check only for walls tagged "Jumpable"
        if (other.CompareTag("Jumpable"))
        {
            nearWall = true;

            // Calculate normal away from wall
            Vector3 closestPoint = other.ClosestPoint(transform.position);
            wallNormal = (transform.position - closestPoint).normalized;
        }
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
