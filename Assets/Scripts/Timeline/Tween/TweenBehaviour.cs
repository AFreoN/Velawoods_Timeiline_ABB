using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using CustomExtensions;

[System.Serializable]
public class TweenBehaviour : PlayableBehaviour
{
    [HideInInspector] public PlayableDirector director;
    [HideInInspector] public Transform t;

    #region For resetting transform
    Vector3 resetPosition = Vector3.zero;
    Quaternion resetRotation = Quaternion.identity;
    #endregion

    public TranslateType translateType = TranslateType.FromPreviousClip;

    //[BoxGroup("Start")]
    //[HideIfGroup("Start/translateType", TranslateType.FromPreviousClip)]
    public Vector3 startPosition, startRotation;
    
    public Vector3 endPosition, endRotation;

    [HideInInspector] public double startTime;
    [HideInInspector] public double endTime;

    TimelineBehaviour tb;

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying || t == null) return;
#endif

        //tb = t.GetComponent<TimelineBehaviour>();
        //if (tb != null)
        //{
        //    tb.setTimings((float)startTime, (float)endTime);
        //    tb.OnClipStart(this);
        //}
        //else
        //    Debug.LogError("No timeline behaviour found on : " + t.name);
    }

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        //Transform t = playerData as Transform;
        //if (!firstFrameHappened)
        //{
        //    firstFrameHappened = true;
        //    referenceTransform = t;
        //    resetPosition = t.position;
        //    resetRotation = t.rotation;
        //}

        if (!t || !director) return;

        double playTime = director.time;
        if (playTime <= startTime || playTime > endTime)
            return;

        double i = Extensions.InverseLerp(startTime, endTime, playTime);

        t.position = Vector3.Lerp(startPosition, endPosition, (float)i);
        t.rotation = Quaternion.Slerp(Quaternion.Euler(startRotation), Quaternion.Euler(endRotation), (float)i);
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying) return;
#endif

        if (playable.isPlayableCompleted(info) && tb != null)
            tb.OnClipEnd(this);
    }

    public override void OnGraphStart(Playable playable)
    {
        if (t == null) return;
        resetPosition = t.position;
        resetRotation = t.rotation;
    }

    public override void OnGraphStop(Playable playable)
    {
        if (t == null) return;
        t.position = resetPosition;
        t.rotation = resetRotation;
    }

    //public override void OnPlayableDestroy(Playable playable)
    //{
    //    firstFrameHappened = false;

    //    t.position = resetPosition;
    //    t.rotation = resetRotation;
    //}

    public enum TranslateType
    {
        FromPreviousClip,
        FromNewPosition,
        HoldNewPosition
    }
}
