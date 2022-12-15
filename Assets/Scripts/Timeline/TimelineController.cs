using UnityEngine;
using UnityEngine.Playables;
using System.Collections;
using System;

[RequireComponent(typeof(PlayableDirector))]
public class TimelineController : MonoBehaviour
{
    public static TimelineController instance { get; private set; }
    bool isPlaying = true;

    /// <summary>
    /// Delegate to handle timeline pause and play state change events
    /// </summary>
    /// <param name="b">Is Timeline Pauses</param>
    public delegate void OnTimelineStateChange(bool b); //b = isPaused
    public static event OnTimelineStateChange onTimelineStateChange;

    PlayableDirector playableDirector = null;
    PlayableDirector getPlayableDirector() => playableDirector;

    [SerializeField] TimelineData timelineData = null;

    private void Awake()
    {
        instance = this;
        playableDirector = GetComponent<PlayableDirector>();
        isPlaying = true;

        if(timelineData != null)
            timelineData.setData(getPlayableDirector, playableDirector.playableAsset);
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
        isPlaying = false;
    }

    /// <summary>
    /// Call this to resume timeline playback
    /// </summary>
    public void PlayTimeline()
    {
        onTimelineStateChange?.Invoke(false);
        //StartCoroutine(toggleTimeline(false));
        playableDirector.Play();
        isPlaying = true;
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

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            if (isPlaying)
                PauseTimeline();
            else
                PlayTimeline();
        }    
    }
}
