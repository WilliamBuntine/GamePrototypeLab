using UnityEngine;

public class CourseStart : MonoBehaviour
{
    [Header("References")]
    public Course course;
    public CourseUI ui;

    public Timer timer;

    private void Start()
    {
        SetCourseActive(false);
    }

    public void StartCourse()
    {
        if (course == null)
        {
            Debug.LogWarning("No Course assigned to CourseStart!");
            return;
        }

        course.ui = ui;

        SetCourseActive(true);
        course.CreateList();
        Debug.Log("I set course active....");

        timer.StartTimer();

        Debug.Log($"Course '{course.name}' started");
    }

    private void SetCourseActive(bool active)
    {
        course.gameObject.SetActive(active);

        foreach (Transform child in course.transform)
        {
            child.gameObject.SetActive(active);
        }
    }
}
