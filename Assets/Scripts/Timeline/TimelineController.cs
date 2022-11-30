using UnityEngine;
using UnityEngine.Playables;
using System.Collections;
using System;

[RequireComponent(typeof(PlayableDirector))]
public class TimelineController : MonoBehaviour
{
    public static TimelineController instance { get; private set; }

    public delegate void OnTimelineStateChange(bool b); //b = isPaused
    public static event OnTimelineStateChange onTimelineStateChange;

    PlayableDirector playableDirector = null;

    [SerializeField] TimelineData timelineData = null;

    private void Awake()
    {
        instance = this;
        playableDirector = GetComponent<PlayableDirector>();

        if(timelineData != null)
            timelineData.setData(playableDirector);
    }

    /// <summary>
    /// Call when timeline needs to be paused
    /// </summary>
    public void PauseTimeline()
    {
        onTimelineStateChange?.Invoke(true);
        //StartCoroutine(toggleTimeline(true));
        playableDirector.Pause();
        Debug.Log("Timeline Paused");
    }

    /// <summary>
    /// Call this to resume timeline playback
    /// </summary>
    public void PlayTimeline()
    {
        onTimelineStateChange?.Invoke(false);
        //StartCoroutine(toggleTimeline(false));
        playableDirector.Play();
    }

    [ContextMenu("Skip duration")]
    public void Skip()
    {
        //onTimelineStateChange?.Invoke(true);
        playableDirector.time = 6.5f;
        PlayableInstance.Skip((float)playableDirector.time);
    }

    IEnumerator toggleTimeline(bool pause)
    {
        yield return new WaitForEndOfFrame();
        if (pause) playableDirector.Pause();
        else playableDirector.Play();
    }
}

//Every object in the scene that requires control from custom timeline track needs to derived from this class
public abstract class TimelineBehaviour : MonoBehaviour
{
    //[HideInInspector]
    public float startTime, endTime;

    public void setTimings(float _startTime, float _endTime)
    {
        startTime = _startTime;
        endTime = _endTime;
        PlayableInstance.AddPlayable(this);
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
}
