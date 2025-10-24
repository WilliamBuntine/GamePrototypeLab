using UnityEngine;

public class CourseStart : MonoBehaviour
{
    [Header("References")]
    public Course course;
    public CourseUI ui;
    public static CourseStart activeCourse;
    public PlayerMove player;

    private void Start()
    {

        SetCourseActive(false);
        activeCourse = null;
    }

    public void StartCourse()
    {
        if (activeCourse != null && activeCourse != course)
        {
            Debug.LogWarning($"Cannot start '{course.name}' — '{activeCourse.name}' is already active!");
            return;
        }

        player.activeCourse = course;

        if (course == null)
        {
            Debug.LogWarning("No Course assigned to CourseStart!");
            return;
        }

        course.ui = ui;
        ui.Start();

        SetCourseActive(true);
        course.CreateList();
        Debug.Log("I set course active....");

        Debug.Log($"Course '{course.name}' started");
    }

    public void SetCourseActive(bool active)
    {
        course.gameObject.SetActive(active);

        foreach (Transform child in course.transform)
        {
            child.gameObject.SetActive(active);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        { 
            StartCourse();
            gameObject.SetActive(false);
        }
    }



}
