using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WorldSpaceCourseProgress : MonoBehaviour
{
    public Slider progressSlider;   // The world-space UI Slider
    public TMP_Text scoreboardText; // The text object containing the scoreboard
    public int totalCourses = 8;    // Total number of courses (for 100% progress)

    void Update()
    {
        if (scoreboardText == null || progressSlider == null)
            return;

        string text = scoreboardText.text;
        int completedCourses = CountHyphens(text);

        // Calculate normalized progress (0 to 1)
        float progress = Mathf.Clamp01((float)completedCourses / totalCourses);

        // Apply the progress to the slider
        progressSlider.value = progress;
    }

    int CountHyphens(string text)
    {
        int count = 0;
        foreach (char c in text)
        {
            if (c == '-') count++;
        }
        return count;
    }
}
