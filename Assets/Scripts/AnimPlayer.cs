using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using System.Collections.Generic;

public class AnimPlayer : MonoBehaviour
{
    Animator anim = null;

    RuntimeAnimatorController defaultController = null;

    [SerializeField] TimelineData timelineData = null;  //Scriptable object to get current playable director, timeline asset and reset runtime animation controller
    RuntimeAnimatorController resetController = null;
    PlayableDirector director;
    TimelineAsset asset;

    [SerializeField] string trackName = "";     //name of the parent of this animation track in timeline

    [Header("Custom Animation Controller")][Tooltip("Is specific animation controller required for this character?")]
    [SerializeField] bool useCustomController = false;  //Sets different runtime controller instead of using from timelineData
    [DrawIf("useCustomController", true, ComparisonType.Equals, DisablingType.DontDraw)]
    [SerializeField] RuntimeAnimatorController customResetController = null;    //Custom runtime animation controller for this object

    [Header("Debugging")]
    [SerializeField] bool debugTracks = false;  //Throws all the timeline clips in the debug on Start call

    List<TimelineAnimationClips> timelineAnimClips = new List<TimelineAnimationClips>();    //List of animation clips with its start and end time in timeline

    private void Start()
    {
        anim = GetComponent<Animator>();
        defaultController = anim.runtimeAnimatorController;

        director = timelineData.playableDirector;
        asset = timelineData.timelineAsset;
        resetController = timelineData.resetController;

        if(asset != null)
        {
            getAnimationBindings();
        }

        //anim.Play(clip[0].name);
    }

    /// <summary>
    /// Get all Animations by finding the track using ParentTrack name, and then store it in TimelineAnimationClips with its start and end time
    /// </summary>
    void getAnimationBindings()
    {
        IEnumerable<TrackAsset> allTracks = asset.GetOutputTracks();
        foreach(var item in allTracks)
        {
            if(item.parent.name == trackName)
            {
                if(item.name.Contains("Animation Track"))
                {
                    foreach (var v in item.GetClips())
                    {
                        timelineAnimClips.Add(new TimelineAnimationClips(v.animationClip, (float)v.start, (float)v.end));
                    }
                }
            }
        }

        for(int i = 0; i < timelineAnimClips.Count - 1; i++)
        {
            float currentEndTime = timelineAnimClips[i].endTime;
            float nextStartTime = timelineAnimClips[i + 1].startTime;
            if (nextStartTime < currentEndTime)
                timelineAnimClips[i].endTime = nextStartTime;

            if(debugTracks)
                Debug.Log(trackName + " Clip : " + timelineAnimClips[i].clip.name + ", Start : " + timelineAnimClips[i].startTime + ", end : " + timelineAnimClips[i].endTime);
        }

        if(debugTracks)
            Debug.Log("Clip at 10s : " + getCurrentTimelineClip(10f).clip.name);
    }

    TimelineAnimationClips getCurrentTimelineClip(float time)
    {
        foreach(TimelineAnimationClips c in timelineAnimClips)
        {
            if (c.isInTimeframe(time))
            {
                //Debug.Log("Return start time : " + c.startTime);
                //Debug.Log("Return end time : " + c.endTime);
                return c;
            }
        }
        return null;
    }

    protected class TimelineAnimationClips
    {
        public AnimationClip clip;
        public float startTime;
        public float endTime;

        public TimelineAnimationClips(AnimationClip _clip, float _s, float _e)
        {
            clip = _clip; startTime = _s; endTime = _e;
        }

        public bool isInTimeframe(double t)
        {
            return t >= startTime && t <= endTime;
        }
    }

    /// <summary>
    /// Manually play animation by taking PlayableDirector time as a reference
    /// </summary>
    public void playAnimation()
    {
        var ctc = getCurrentTimelineClip((float)director.time);
        if (ctc == null) return;
        string cName = ctc.clip.name;
        float normalizedTime = (float)(director.time - ctc.startTime);
        normalizedTime = normalizedTime % ctc.clip.averageDuration;
        normalizedTime /= ctc.clip.averageDuration;

        //if(trackName == "Mark")
        //{
        //    Debug.Log("Mark Clip : " + cName + ", director time : " + director.time);
        //}

        if (useCustomController)
            anim.runtimeAnimatorController = customResetController;
        else
            anim.runtimeAnimatorController = resetController;

        anim.StartPlayback();
        anim.speed = 1;
        anim.Play(cName, 1, normalizedTime);
        //anim.CrossFadeInFixedTime(cName, .5f);

        //Debug.Log("Animation playback started : " + cName);
    }

    /// <summary>
    /// Resets the RuntimeAnimationController in this object
    /// </summary>
    public void resetAnimation()
    {
        anim.runtimeAnimatorController = defaultController;
    }
}
