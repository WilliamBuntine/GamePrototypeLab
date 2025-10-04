using UnityEngine;

public class Interact : MonoBehaviour
{
    [Header("Settings")]
    public KeyCode interactKey = KeyCode.E;
    public float interactDistance = 5f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip successSound;
    public AudioClip failSound;

    [Header("References")]
    public Camera playerCamera;

    void Update()
    {
        if (Input.GetKeyDown(interactKey))
        {
            TryInteract();
        }
    }

    void TryInteract()
    {
        // Default to main camera if not assigned
        if (playerCamera == null)
            playerCamera = Camera.main;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance))
        {
            // Check if the hit object has a CourseStart component
            CourseStart courseStart = hit.collider.GetComponent<CourseStart>();

            if (courseStart != null)
            {
                PlaySound(successSound);
                Debug.Log($"Interacted successfully with: {hit.collider.name}");
                courseStart.StartCourse();
            }
            else
            {
                PlaySound(failSound);
                Debug.Log($"Object hit has no CourseStart component: {hit.collider.name}");
            }
        }
        else
        {
            PlaySound(failSound);
            Debug.Log("No object hit by interact raycast");
        }
    }

    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}
