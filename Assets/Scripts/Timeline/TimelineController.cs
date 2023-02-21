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
    public double currentPlayableTime => getPlayableDirector().time;

    [SerializeField] TimelineData timelineData = null;

    private void Awake()
    {
        instance = this;
        playableDirector = GetComponent<PlayableDirector>();
        isPlaying = true;

        if(timelineData != null)
            timelineData.setData(getPlayableDirector, playableDirector.playableAsset);

        playableDirector.RebuildGraph();
        playableDirector.Play();
    }

    /// <summary>
    /// Call when timeline needs to be paused
    /// </summary>
    public void PauseTimeline()
    {
        onTimelineStateChange?.Invoke(true);

        playableDirector.playableGraph.GetRootPlayable(0).SetSpeed(0);
        //playableDirector.Pause();

        isPlaying = false;
    }

    /// <summary>
    /// Call this to resume timeline playback
    /// </summary>
    public void PlayTimeline()
    {
        onTimelineStateChange?.Invoke(false);

        playableDirector.playableGraph.GetRootPlayable(0).SetSpeed(1);
        //playableDirector.Play();

        isPlaying = true;
    }

    public void SkipTimeline(double _time, bool _play = true)
    {
        //onTimelineStateChange?.Invoke(true);
        PlayableInstance.Skip((float)_time);
        playableDirector.time = _time;

        if (_play && isPlaying == false)
            StartCoroutine(toggleTimeline(false));
        else if (isPlaying)
            StartCoroutine(toggleTimeline(true));
    }

    IEnumerator toggleTimeline(bool pause)
    {
        yield return new WaitForEndOfFrame();
        if (pause) PauseTimeline();
        else PlayTimeline();
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

        //if(play)
        //    playableDirector.time += Time.deltaTime;

        //if(play)
        //    playableDirector.Evaluate();
    }
}
