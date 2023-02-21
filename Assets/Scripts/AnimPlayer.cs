using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using System.Collections.Generic;
using System.Collections;

public class AnimPlayer : MonoBehaviour
{
    Animator anim = null;
    bool initialized = false;

    RuntimeAnimatorController defaultController = null;

    [SerializeField] TimelineData timelineData = null;  //Scriptable object to get current playable director, timeline asset and reset runtime animation controller
    RuntimeAnimatorController resetController = null;
    PlayableDirector director;
    TimelineAsset asset;

    [SerializeField] string trackName = "";     //name of the parent of this animation track in timeline

    [SerializeField] int[] layers = new int[1] { 1 };

    [Header("Custom Animation Controller")][Tooltip("Is specific animation controller required for this character?")]
    [SerializeField] bool useCustomController = false;  //Sets different runtime controller instead of using from timelineData
    [DrawIf("useCustomController", true, ComparisonType.Equals, DisablingType.DontDraw)]
    [SerializeField] RuntimeAnimatorController customResetController = null;    //Custom runtime animation controller for this object

    [Header("Debugging")]
    [SerializeField] bool debugTracks = false;  //Throws all the timeline clips in the debug on Start call

    [SerializeField]
    List<TimelineAnimationClips> timelineAnimClips = new List<TimelineAnimationClips>();    //List of animation clips with its start and end time in timeline

    private void Start()
    {
        Initialize();
    }

    void Initialize()
    {
        anim = GetComponent<Animator>();
        defaultController = anim.runtimeAnimatorController;

        director = timelineData.playableDirector;
        asset = timelineData.timelineAsset;
        resetController = timelineData.resetController;

        if (asset != null)
        {
            getAnimationBindings();
        }

        initialized = true;
    }

    bool isLoopingRequired(TimelineClip clip)
    {
        AnimationPlayableAsset apa = (AnimationPlayableAsset)clip.asset;
        bool loop = false;
        if (apa.loop == AnimationPlayableAsset.LoopMode.On)
            loop = true;
        else if (apa.loop == AnimationPlayableAsset.LoopMode.UseSourceAsset)
        {
            if (apa.clip.isLooping)
                loop = true;
        }
        return loop;
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
                if(item.name.Contains("Animation Track") && item.muted == false)
                {
                    foreach (var v in item.GetClips())
                    {
                        bool loop = isLoopingRequired(v);                       
                        timelineAnimClips.Add(new TimelineAnimationClips(v.animationClip, (float)v.start, (float)v.end, 0, loop));
                    }

                    var childTracks = item.GetChildTracks();
                    if(childTracks != null)
                    {
                        int trackIndex = 0;
                        foreach (var c in childTracks)
                        {
                            trackIndex += 1;
                            foreach (var o in c.GetClips())
                            {
                                timelineAnimClips.Add(new TimelineAnimationClips(o.animationClip, (float)o.start, (float)o.end, trackIndex, isLoopingRequired(o)));
                            }
                        }
                    }
                }
                
                if(debugTracks)
                {
                    Debug.Log("Track : " + item.name);
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

        int L = timelineAnimClips.Count;
        if(debugTracks && L > 1)
        {
            Debug.Log(trackName + " Clip : " + timelineAnimClips[L-1].clip.name + ", Start : " + timelineAnimClips[L - 1].startTime + ", end : " + timelineAnimClips[L - 1].endTime);
        }
    }

    public TimelineAnimationClips getCurrentTimelineClip(float time, int id = 0)
    {
        foreach(TimelineAnimationClips c in timelineAnimClips)
        {
            if (c.isInTimeframe(time, id))
            {
                //Debug.Log("Return start time : " + c.startTime);
                //Debug.Log("Return end time : " + c.endTime);
                return c;
            }
        }
        return null;
    }

    [System.Serializable]
    public class TimelineAnimationClips
    {
        public AnimationClip clip;
        public float startTime;
        public float endTime;
        public float trackID;
        public bool loop;

        public TimelineAnimationClips(AnimationClip _clip, float _s, float _e, int _TrackID, bool _Loop)
        {
            clip = _clip; startTime = _s; endTime = _e; trackID = _TrackID; loop = _Loop;
        }

        public bool isInTimeframe(double t)
        {
            return (t >= startTime && t <= endTime);
        }

        public bool isInTimeframe(double t, int id = 0)
        {
            return (t >= startTime && t <= endTime) && trackID == id;
        }
    }

    /// <summary>
    /// Manually play animation by taking PlayableDirector time as a reference
    /// </summary>
    public void playAnimation()
    {
        if (!initialized)
            Initialize();

        if (useCustomController)
            anim.runtimeAnimatorController = customResetController;
        else
            anim.runtimeAnimatorController = resetController;

        for (int i = 0; i < layers.Length; i++)
        {
            var ctc = getCurrentTimelineClip((float)director.time, i);
            if (ctc == null)
            {
                if (debugTracks)
                    Debug.Log($"No animations tracks to play at this { (float)director.time} : " + trackName);
                continue;
            }
            string cName = ctc.clip.name;
            float normalizedTime = (float)(director.time - ctc.startTime);
            normalizedTime = normalizedTime % ctc.clip.averageDuration;
            normalizedTime /= ctc.clip.averageDuration;

            //if(trackName == "Mark")
            //{
            //    Debug.Log("Mark Clip : " + cName + ", director time : " + director.time);
            //}

            if(ctc.loop == false && (director.time - ctc.startTime) >= ctc.clip.length)
            {
                normalizedTime = 1;
            }


            anim.StartPlayback();
            anim.speed = 1;
            anim.Play(cName, layers[i], normalizedTime);

            if (debugTracks)
            {
                Debug.Log("Current playing animation : " + cName);
            }
            //anim.CrossFadeInFixedTime(cName, .5f);
        }

    }

    public void PlayNonTimelineAnimation()
    {
        if (!initialized)
            Initialize();

        for (int i = 0; i < layers.Length; i++)
        {
            var ctc = getCurrentTimelineClip((float)director.time, i);
            if (ctc == null)
            {
                if (debugTracks)
                    Debug.Log($"No animations tracks to play at this { (float)director.time} : " + trackName);
                continue;
            }
            string cName = ctc.clip.name;
            float normalizedTime = (float)(director.time - ctc.startTime);
            normalizedTime = normalizedTime % ctc.clip.averageDuration;
            normalizedTime /= ctc.clip.averageDuration;

            //if(trackName == "Mark")
            //{
            //    Debug.Log("Mark Clip : " + cName + ", director time : " + director.time);
            //}

            if (ctc.loop == false && (director.time - ctc.startTime) >= ctc.clip.length)
            {
                normalizedTime = 1;
            }


            anim.StartPlayback();
            anim.speed = 1;
            anim.Play(cName, layers[i], normalizedTime);

            if (debugTracks)
            {
                Debug.Log("Current playing animation : " + cName);
            }
            //anim.CrossFadeInFixedTime(cName, .5f);
        }
    }

    /// <summary>
    /// Resets the RuntimeAnimationController in this object
    /// </summary>
    public void resetAnimation()
    {
        anim.runtimeAnimatorController = defaultController;
    }
}
