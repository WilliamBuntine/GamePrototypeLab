using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using TMPro;

[System.Serializable]
public class ScoreData
{
    public List<ScoreEntry> entries = new List<ScoreEntry>();
}

public class Scoreboard : MonoBehaviour
{
    private string filePath;
    public ScoreData data = new ScoreData();
    public AudioSource audioSource;
    public AudioClip CourseComplete;
    private TMP_Text scoreboardText;

    void Awake()
    {
        filePath = Path.Combine(Application.persistentDataPath, "scores.json");
        scoreboardText = GetComponent<TMP_Text>();
        LoadScores();
        UpdateDisplay();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            CompleteSound();
        }
    }

    public void UpdateScore(string courseName, float newTime)
    {
        var entry = data.entries.Find(e => e.courseName == courseName);

        if (entry != null)
        {
            if (newTime < entry.bestTime)
            {
                CompleteSound();
                entry.bestTime = newTime;
            }
        }
        else
        {
            data.entries.Add(new ScoreEntry { courseName = courseName, bestTime = newTime });
            CompleteSound();
        }

        SaveScores();
        UpdateDisplay();
    }

    private void SaveScores()
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(filePath, json);
    }

    private void LoadScores()
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            data = JsonUtility.FromJson<ScoreData>(json);
        }
    }

    private void UpdateDisplay()
    {
        if (scoreboardText == null) return;

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("<b>Course Best Times</b>\n");

        foreach (var entry in data.entries)
        {
            int minutes = Mathf.FloorToInt(entry.bestTime / 60);
            int seconds = Mathf.FloorToInt(entry.bestTime % 60);
            int milliseconds = Mathf.FloorToInt((entry.bestTime * 1000) % 1000);

            sb.AppendLine($"{entry.courseName} - {minutes:00}:{seconds:00}:{milliseconds:000}");
        }

        scoreboardText.text = sb.ToString();
    }

    void CompleteSound()
    {
        audioSource.PlayOneShot(CourseComplete, 0.5f);
    }
}
