using UnityEngine;
using TMPro;


public class Course : MonoBehaviour
{
    public AudioSource audioSource; // Audio source for playing sounds
    public AudioClip CourseComplete; // Sound to play when at high speed

    public string courseName;
    public Checkpoint[] pointList;
    public CourseUI ui;
    public Timer timer;
    public TextMeshProUGUI timerUI;
    public Scoreboard scoreboard;
    public GameObject startCheckpoint;
    public GameObject respawnPoint;

    private int checkpointsReached = 0;
    private bool courseComplete = false;

    public void CreateList()
    {
        if (ui != null)
        {
            ui.gameObject.SetActive(true);
            ui.activeCourse = this;
            ui.GenerateUI(pointList.Length);
        }

        foreach (Checkpoint point in pointList)
        {
            if (point != null)
                point.parentCourse = this;
            point.Refresh();
        }

        checkpointsReached = 0;
        courseComplete = false;
        timerUI.gameObject.SetActive(true);
        timer?.ResetTimer();
        timer?.StartTimer();
    }


    public void UpdateList()
    {
        if (courseComplete) return;

        checkpointsReached++;
        ui?.FillNextCheckpoint();

        if (checkpointsReached >= pointList.Length)
        {
            CompleteCourse();
        }
    }

    void CompleteCourse()
    {
        courseComplete = true;
        
        timer?.StopTimer();
        CourseStart.activeCourse = null;
        
                Debug.Log("course Finished.");

        float finalTime = timer != null ? timer.time : 0f;

        if (startCheckpoint != null)
        {
            startCheckpoint.SetActive(true);
        }

        foreach (Checkpoint point in pointList)
        {
            point.gameObject.SetActive(false);
        }

        scoreboard.UpdateScore(courseName, finalTime);

        if (ui != null)
        {
            ui.activeCourse = null;
            ui.gameObject.SetActive(false);
            timerUI.gameObject.SetActive(false);
        }

        Debug.Log($"Course '{courseName}' complete! Final time: {finalTime:F2}s");
    }

    public void CancelCourse()
    {
        courseComplete = true;

        timer?.StopTimer();
        CourseStart.activeCourse = null;

        Debug.Log("Eminem Cancelled.");

        if (startCheckpoint != null)
        {
            startCheckpoint.SetActive(true);
        }

        foreach (Checkpoint point in pointList)
        {
            point.gameObject.SetActive(false);
        }

        if (ui != null)
        {
            ui.activeCourse = null;
            ui.gameObject.SetActive(false);
            timerUI.gameObject.SetActive(false);
        }

        Debug.Log($"Course '{courseName}' Cancelled!");
    }
    
    void Completesound()
    {
        audioSource.PlayOneShot(CourseComplete, 1.0f);
    }
}
