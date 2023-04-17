using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

//Every object in the scene that requires control from custom timeline track needs to derived from this class
public abstract class TimelineBehaviour : MonoBehaviour
{
    [HideInInspector]
    public float startTime, endTime;

    public void setTimings(float _startTime, float _endTime)
    {
        startTime = _startTime;
        endTime = _endTime;
        //PlayableInstance.AddPlayable(this);
    }

    public virtual void OnClipStart(object o)
    {

    }

    public virtual void OnClipEnd(object o)
    {

    }

    public virtual void OnProcessFrame(object o)
    {

    }

    public virtual void OnSkip()
    {

    }

    public virtual void OnReset()
    {

    }
}

public interface ITimelineBehaviour
{
    public double startTime { get; set; }
    public double endTime { get; set; }

    public void OnSkip();

    public virtual void OnReset()
    {

    }
}

public class CustomPlayaleBehaviour : UnityEngine.Playables.PlayableBehaviour
{

    public virtual void OnReset()
    {

    }
}
