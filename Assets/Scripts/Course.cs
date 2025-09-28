using UnityEngine;

public class Course : MonoBehaviour
{

    public Checkpoint[] pointList;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       foreach (Checkpoint point in pointList)
        {
            if (point != null)
                point.parentCourse = this;
        } 
    }


    public void CheckpointReached(Checkpoint point)
    {

    }
}
