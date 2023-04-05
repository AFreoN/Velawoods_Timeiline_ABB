using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(PlayableDirector))]
public class TimelineController : MonoBehaviour
{
    public static TimelineController instance { get; private set; }
    public static bool isPlaying { get; private set; }

    public const string TRACK_ACTIVITY = "Activity Event Track";
    public const string TRACK_MINIGAME = "Minigame Track";

    /// <summary>
    /// Delegate to handle timeline pause and play state change events
    /// </summary>
    /// <param name="b">Is Timeline Pauses</param>
    public delegate void OnTimelineStateChange(bool b); //b = isPaused
    public static event OnTimelineStateChange onTimelineStateChange;

    public static PlayableDirector playableDirector
    {
        get
        {
            if(pd == null)
            {
                pd = GameObject.FindObjectOfType<PlayableDirector>();
            }
            return pd;
        }
    }
    static PlayableDirector pd = null;

    public PlayableDirector getPlayableDirector() => playableDirector;
    public double currentPlayableTime => getPlayableDirector().time;

    [SerializeField] TimelineData timelineData = null;

    private void Awake()
    {
        instance = this;
        pd = GetComponent<PlayableDirector>();

        if(timelineData != null)
            timelineData.setData(getPlayableDirector, playableDirector.playableAsset);

        pd.RebuildGraph();
        pd.Play();
#if CLIENT_BUILD
        PauseTimeline();
#else
        PlayTimeline();
#endif
    }

    /// <summary>
    /// Call when timeline needs to be paused
    /// </summary>
    public void PauseTimeline()
    {
        onTimelineStateChange?.Invoke(true);

        if (playableDirector.playableGraph.IsValid())
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

        if(playableDirector.playableGraph.IsValid())
            playableDirector.playableGraph.GetRootPlayable(0).SetSpeed(1);
        //playableDirector.Play();

        isPlaying = true;
    }

    public void SkipTimeline(double _time, bool _play = true)
    {
        //onTimelineStateChange?.Invoke(true);
        PlayableInstance.Skip((float)_time);
        playableDirector.time = _time;

        if (_play)
            PlayTimeline();
        else
            PauseTimeline();

        //Debug.Log("Timeline skipped with time : " + _time + ", play : " + _play);
    }

    IEnumerator toggleTimeline(bool pause)
    {
        yield return new WaitForEndOfFrame();
        if (pause) PauseTimeline();
        else PlayTimeline();
    }

    public static float getPreviousActivityTime(float _time) => getPreviousTrackTime(TRACK_ACTIVITY, _time);
    public static float getNextActivityTime(float _time) => getNextTrackTime(TRACK_ACTIVITY, _time);
    public static float getPreviousMinigameTime(float _time) => getPreviousTrackTime(TRACK_MINIGAME, _time);
    public static float getNextMinigameTime(float _time) => getNextTrackTime(TRACK_MINIGAME, _time);

    static float getPreviousTrackTime(string _trackName, float _runningTime)
    {
        float result = 0, diff = float.MaxValue;

        TimelineAsset asset = playableDirector.playableAsset as TimelineAsset;

        foreach (var v in asset.GetOutputTracks())
        {
            if (v.name.Equals(_trackName))
            {
                foreach (var c in v.GetClips())
                {
                    if (c.start < _runningTime)
                    {
                        float currentDiff = _runningTime - (float)c.start;
                        if (currentDiff < diff)
                        {
                            diff = currentDiff;
                            result = (float)c.start;
                        }
                    }
                }
            }
        }

        return result;
    }

    static float getNextTrackTime(string _trackName, float _runningTime)
    {
        float result = -1, diff = float.MaxValue;

        TimelineAsset asset = playableDirector.playableAsset as TimelineAsset;

        foreach (var v in asset.GetOutputTracks())
        {
            if (v.name.Equals(_trackName))
            {
                foreach (var c in v.GetClips())
                {
                    if (c.start > _runningTime)
                    {
                        float currentDiff = (float)c.start - _runningTime;
                        if (currentDiff < diff)
                        {
                            diff = currentDiff;
                            result = (float)c.start;
                        }
                    }
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Get all the clips from the give track name
    /// </summary>
    /// <typeparam name="T">Playable asset this track is associated with</typeparam>
    /// <param name="_trackName">Name of the track in timeline asset</param>
    /// <returns></returns>
    public static List<(T, float, float)> getAllClipsFromTrack<T>(string _trackName) where T : PlayableAsset
    {
        List<(T, float, float)> result = new List<(T, float, float)>();

        TimelineAsset asset = playableDirector.playableAsset as TimelineAsset;

        foreach(var track in asset.GetOutputTracks())
        {
            if (track.name.Equals(_trackName))
            {
                foreach(var c in track.GetClips())
                {
                    var item = ((T)c.asset, (float)c.start, (float)c.end);
                    result.Add(item);
                    return result;
                }
            }
        }

        return null;
    }
}
