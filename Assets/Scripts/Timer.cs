using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class Timer : MonoBehaviour
{
    public float time = 0f;
    public bool isRunning = false;
    public Course course;
    public Scoreboard scoreboard;
    private TMP_Text timerText;

    void Start()
    {
        timerText = GetComponent<TMP_Text>();
    }

    void Update()
    {
        if (isRunning)
        {
            time += Time.deltaTime;
            UpdateTimerUI();
        }
    }

    void UpdateTimerUI()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(time / 60);
            int seconds = Mathf.FloorToInt(time % 60);
            int milliseconds = Mathf.FloorToInt((time * 1000) % 1000);
            timerText.text = $"{minutes:00}:{seconds:00}:{milliseconds:000}";
        }
    }

    public void StartTimer()
    {
        time = 0f;
        isRunning = true;
    }

    public void StopTimer()
    {
        isRunning = false;
    }

    public void ResetTimer()
    {
        time = 0f;
        UpdateTimerUI();
    }

    public void SendTime(float time, Course course)
    {
        scoreboard.PostTime(time, course);
    }
}
