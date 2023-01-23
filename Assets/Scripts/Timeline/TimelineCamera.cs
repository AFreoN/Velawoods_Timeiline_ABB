using UnityEngine;

public class TimelineCamera : MonoBehaviour
{
    [SerializeField] bool holdPosition;
    [SerializeField] bool holdParent = false;
    [SerializeField] Space space = Space.World;

    bool timelinePaused = false;

    Transform prevParent = null;
    Vector3 currentPosition;
    Quaternion currentRotation;

    //Subscribe and unsubscribe timeline state change events
    private void OnEnable()
    {
        TimelineController.onTimelineStateChange += setOldPosition;
    }

    private void OnDisable()
    {
        TimelineController.onTimelineStateChange -= setOldPosition;
    }

    /// <summary>
    /// If timeline paused, hold the camera position and rotation instead of resetting to default values
    /// </summary>
    /// <param name="isPaused">Is Timleline Paused</param>
    public void setOldPosition(bool isPaused)
    {
        timelinePaused = isPaused;
        if (isPaused)
        {
            if(space == Space.World)
            {
                currentPosition = transform.position;
                currentRotation = transform.rotation;
            }
            else
            {
                currentPosition = transform.localPosition;
                currentRotation = transform.localRotation;
            }

            if (holdParent)
                prevParent = transform.parent;
        }
    }

    private void LateUpdate()
    {
        if(timelinePaused && holdPosition)
        {
            if (holdParent)
                transform.SetParent(prevParent);

            if(space == Space.World)
            {
                transform.position = currentPosition;
                transform.rotation = currentRotation;
            }
            else
            {
                transform.localPosition = currentPosition;
                transform.localRotation = currentRotation;
            }
        }
    }
}
