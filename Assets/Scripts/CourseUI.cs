using UnityEngine;
using UnityEngine.UI;

public class CourseUI : MonoBehaviour
{
    public Course activeCourse;  
    public GameObject checkpointPrefab; 
    public Transform container;  

    private Image[] checkpointImages;
    private int filledCount = 0;

    public void Start()
    {
        if (activeCourse != null)
        {
            GenerateUI(activeCourse.pointList.Length);
        }
    }

    public void GenerateUI(int count)
    {
        // Clear existing children
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }

        checkpointImages = new Image[count];
        filledCount = 0;

        for (int i = 0; i < count; i++)
        {
            GameObject go = Instantiate(checkpointPrefab, container);
            checkpointImages[i] = go.GetComponent<Image>();
            checkpointImages[i].fillAmount = 0.5f;
        }
    }

    // Call this when any checkpoint is reached
    public void FillNextCheckpoint()
    {
        if (checkpointImages == null || filledCount >= checkpointImages.Length) return;

        checkpointImages[filledCount].fillAmount = 1f;
        filledCount++;
    }
}
