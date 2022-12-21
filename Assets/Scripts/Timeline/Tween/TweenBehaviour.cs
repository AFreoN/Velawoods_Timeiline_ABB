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

    bool positionInitialized = false;
    public bool useCurveRotation = false;
    public TranslateType translateType = TranslateType.FromPreviousClip;

    public Vector3 startPosition, startRotation;
    public Vector3 endPosition, endRotation;

    public BezierType curveType = BezierType.Linear;
    public Vector3 point1, point2;

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
        if (!t || !director) return;

        double playTime = director.time;
        if (playTime <= startTime || playTime > endTime)
            return;

        double i = Extensions.InverseLerp(startTime, endTime, playTime);

        if(translateType == TranslateType.HoldNewPosition)
        {
            t.position = startPosition;
            t.rotation = Quaternion.Euler(startRotation);
            return;
        }

        if(translateType == TranslateType.FromNewPosition && !positionInitialized)
        {
            positionInitialized = true;
            t.forward = (endPosition - startPosition).setY(0);
        }
        move((float)i);
        if(useCurveRotation == false)
            t.rotation = Quaternion.Slerp(Quaternion.Euler(startRotation), Quaternion.Euler(endRotation), (float)i);
    }

    void move(float i)
    {
        float lerpSpeed = 0.02f;

        switch (curveType)
        {
            case BezierType.Linear:
                t.position = Vector3.Lerp(startPosition, endPosition, i);
                if (useCurveRotation)
                {
                    Vector3 final = (endPosition - startPosition).setY(0);
                    t.forward = Vector3.Lerp(t.forward, final, lerpSpeed);
                }
                break;

            case BezierType.Quadratic:
                t.position = Bezier.getQuadraticPoint(startPosition, endPosition, point1, i);
                if (useCurveRotation)
                {
                    Vector3 final = Bezier.getQuadraticTangent(startPosition, endPosition, point1, i).setY(0);
                    t.forward = Vector3.Lerp(t.forward, final, lerpSpeed);
                }
                break;

            case BezierType.Cubic:
                t.position = Bezier.getCubicPoint(startPosition, endPosition, point1, point2, i);
                if (useCurveRotation)
                {
                    Vector3 final = Bezier.getCubicTangent(startPosition, endPosition, point1, point2, i);
                    t.forward = Vector3.Lerp(t.forward, final, lerpSpeed);
                }
                break;
        }
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying) return;
#endif

        if (playable.isPlayableCompleted(info) && tb != null)
        {
            tb.OnClipEnd(this);
            positionInitialized = false;
        }
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

    public enum BezierType
    {
        Linear,
        Quadratic,
        Cubic
    }
}
