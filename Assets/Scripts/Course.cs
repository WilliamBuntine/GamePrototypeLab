using UnityEngine;

public class Course : MonoBehaviour
{
    public Checkpoint[] pointList;
    public CourseUI ui;

    void Start()
    {
        foreach (Checkpoint point in pointList)
        {
            if (point != null)
                point.parentCourse = this;
        }

        if (ui != null)
            ui.GenerateUI(pointList.Length);
    }

    public void UpdateList()
    {
        ui?.FillNextCheckpoint();
    }
}
