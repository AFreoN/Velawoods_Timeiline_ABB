using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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

        //pd.RebuildGraph();
        //pd.Play();
#if CLIENT_BUILD
        PauseTimeline();
#else
        PlayTimeline();
#endif

#if UNITY_EDITOR
        //Show current playing timeline on start
        StartCoroutine(ShowTimelineWindow());
#endif
    }

#if UNITY_EDITOR
    IEnumerator ShowTimelineWindow()
    {
        yield return new WaitForSeconds(0.5f);
        Transform old = UnityEditor.Selection.activeTransform;
        UnityEditor.Timeline.TimelineEditorWindow window = UnityEditor.EditorWindow.GetWindow<UnityEditor.Timeline.TimelineEditorWindow>();
        window.Show();
        UnityEditor.Selection.activeTransform = transform;

        yield return new WaitForSeconds(0.5f);
        window.locked = true;
        UnityEditor.Selection.activeTransform = old;
    }

    private void OnApplicationQuit()
    {
        UnityEditor.EditorWindow.GetWindow<UnityEditor.Timeline.TimelineEditorWindow>().locked = false;
    }
#endif

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
        CoreEventSystem.Instance.SendEvent("USSequencerPause");
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
        CoreEventSystem.Instance.SendEvent("USSequencerResume");
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

        CoreEventSystem.Instance.SendEvent(CoreEventTypes.ACTIVITY_SKIP_FINISHED);
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
                List<TimelineClip> clips = new List<TimelineClip>();
                TimelineClip currentClip = null;

                foreach (var c in v.GetClips())
                    clips.Add(c);

                clips.Sort((x, y) => y.start.CompareTo(x.start));

                foreach(var c in clips)
                {
                    if (c.start < _runningTime)
                    {
                        currentClip = c;
                        //Debug.Log("Current clip time : " + c.start + ", Run time : " + _runningTime);
                        //return (float)currentClip.start;
                        break;
                    }
                }

                if (currentClip != null)
                {
                    int index = clips.IndexOf(currentClip);
                    if (index + 1 < clips.Count)
                        result = (float)clips[index + 1].start;

                    return result;
                }
                foreach (var c in clips)
                {
                    if (c == currentClip)
                        continue;

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
                List<TimelineClip> clips = new List<TimelineClip>();
                TimelineClip currentClip = null;

                foreach (var c in v.GetClips())
                    clips.Add(c);

                clips.Sort((x, y) => x.start.CompareTo(y.start));

                foreach(var c in clips)
                {
                    if(c.start > _runningTime)
                    {
                        result = (float)c.start;
                        return result;
                    }
                }

                /*foreach (var c in v.GetClips())
                {
                    if (_runningTime > c.end)
                    {
                        float currentDiff = (float)c.start - _runningTime;
                        if (currentDiff < diff)
                        {
                            diff = currentDiff;
                            result = (float)c.start;
                            currentClip = c;
                        }
                    }
                }

                if(currentClip != null)
                {
                    int index = clips.IndexOf(currentClip);
                    if (index + 1 < clips.Count)
                    {
                        result = (float)clips[index + 1].start;
                        Debug.Log("Skipping activity to : " + result + ", Run Time : " + _runningTime);
                        return result;
                    }
                }

                foreach (var c in v.GetClips())
                {
                    if (c == currentClip)
                        continue;

                    if (c.start > _runningTime)
                    {
                        float currentDiff = (float)c.start - _runningTime;
                        if (currentDiff < diff)
                        {
                            diff = currentDiff;
                            result = (float)c.start;
                        }
                    }
                }*/
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
                }
            }
        }

        return result;
    }

    [ContextMenu("Debug Activity Tracks")]
    void DebugTracks()
    {
        var v = getAllClipsFromTrack<CustomTracks.ActivityEventClip>(TRACK_ACTIVITY);
        Debug.Log("Total clip count : " + v.Count);
    }
}
