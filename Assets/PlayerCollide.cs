using UnityEngine;

public class PlayerCollide : MonoBehaviour
{
    public Color newColor = Color.blue;   // The color you want it to change to
    private Renderer rend;
    private Color originalColor;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rend = GetComponent<Renderer>();
        if (rend != null)
        {
            originalColor = rend.material.color;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Checkpoint reached.");
            // Add logic for what happens when the player enters the death zone
            rend.material.color = newColor;

        }
    }
}
