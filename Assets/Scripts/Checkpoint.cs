using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public Color completeColor = Color.blue;
    private Renderer rend;
    private Color originalColor = Color.red;

    public Course parentCourse;
    public bool isComplete = false;

    void Awake()
    {
        rend = GetComponent<Renderer>();
    }

    void Start()
    {
        if (rend != null)
            rend.material.color = originalColor;
    }

    public void Refresh()
    {
        if (rend != null)
        {
            rend.material.color = originalColor;
            isComplete = false;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isComplete)
        {
            Debug.Log("Checkpoint reached.");
            rend.material.color = completeColor;
            parentCourse?.UpdateList();
            isComplete = true;
        }
    }
}
