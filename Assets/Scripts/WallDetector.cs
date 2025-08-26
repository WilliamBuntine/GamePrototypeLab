using UnityEngine;

public class WallDetector : MonoBehaviour
{
    [HideInInspector] public bool nearWall = false;
    [HideInInspector] public Vector3 wallNormal;

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


    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Jumpable"))
        {
            nearWall = false;
            wallNormal = Vector3.zero;
        }
    }
}
