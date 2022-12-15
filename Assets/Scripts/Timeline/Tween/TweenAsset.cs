using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using UnityEditor;
using UnityEditor.Timeline;

[TrackClipType(typeof(TweenBehaviour))]
public class TweenAsset : PlayableAsset, ITimelineClipAsset, ITimelineGizmoDrawable
{
    [HideInInspector] public Transform targetTransform;
    [HideInInspector] public TweenTrack track;
    [HideInInspector] public TimelineAsset asset;
    public TweenBehaviour behaviour = new TweenBehaviour();


    public ClipCaps clipCaps => ClipCaps.ClipIn;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<TweenBehaviour>.Create(graph, behaviour);
        return playable;
    }

    public void OnDrawGizmoSelected()
    {
        return;
        Vector3 sPos = behaviour.startPosition;
        Vector3 ePos = behaviour.endPosition;

        float radius = .1f;
        Color startColor = Color.blue;
        Color endColor = Color.red;

        if(behaviour.translateType == TweenBehaviour.TranslateType.FromNewPosition)
        {
            Gizmos.color = startColor;
            Gizmos.DrawSphere(sPos, radius);
            DrawArrow.ForGizmo(sPos, Quaternion.Euler(behaviour.startRotation) * Vector3.forward * .5f , startColor, 2.5f, .15f, 30);

            Gizmos.color = endColor;
            Gizmos.DrawSphere(ePos, radius);
            DrawArrow.ForGizmo(ePos, Quaternion.Euler(behaviour.endRotation) * Vector3.forward * .5f, endColor, 2.5f, .15f, 30);
        }
        else if(behaviour.translateType == TweenBehaviour.TranslateType.HoldNewPosition)
        {
            Gizmos.color = startColor;
            Gizmos.DrawSphere(sPos, radius);

            DrawArrow.ForGizmo(sPos, Quaternion.Euler(behaviour.startRotation) * Vector3.forward * .5f, startColor, 2.5f, .15f, 30);
        }
        else if(behaviour.translateType == TweenBehaviour.TranslateType.FromPreviousClip)
        {
            Gizmos.color = endColor;
            Gizmos.DrawSphere(ePos, radius);

            DrawArrow.ForGizmo(ePos, Quaternion.Euler(behaviour.endRotation) * Vector3.forward * .5f, endColor, 2.5f, .15f, 30);
        }

        //Handles.PositionHandle(sPos, Quaternion.Euler( behaviour.startRotation));
        //Handles.DrawAAPolyLine(new Vector3[] { behaviour.startPosition, behaviour.endPosition });

        if(Vector3.Distance(sPos, ePos) >= .1f)
            DrawArrow.ForGizmoMiddle(behaviour.startPosition, behaviour.endPosition - behaviour.startPosition, Color.white);
    }

    public bool OnSceneSelected(bool showHandles)
    {
        bool requireRepaint = false;

        Vector3 sPos = behaviour.startPosition;
        Vector3 ePos = behaviour.endPosition;
        Vector3 sEuler = behaviour.startRotation, eEuler = behaviour.endRotation;

        float radius = .1f;
        Color startColor = Color.blue;
        Color endColor = Color.red;

        Vector3 scale = Vector3.zero;

        Quaternion sRot = Quaternion.Euler(sEuler), eRot = Quaternion.Euler(eEuler);

        switch (behaviour.curveType)
        {
            case TweenBehaviour.BezierType.Quadratic:
                requireRepaint = showQuadraticHandles();
                break;

            case TweenBehaviour.BezierType.Cubic:
                requireRepaint = showCubicHandles();
                break;
        }

        if (showHandles)
        {
            switch (behaviour.translateType)
            {
                case TweenBehaviour.TranslateType.FromNewPosition:
                    //Drawing rotation arrows
                    DrawArrow.ForGizmo(sPos, Quaternion.Euler(sEuler) * Vector3.forward * .5f, startColor, 2.5f, .15f, 30);
                    DrawArrow.ForGizmo(ePos, Quaternion.Euler(eEuler) * Vector3.forward * .5f, endColor, 2.5f, .15f, 30);

                    Vector3 p1 = sPos, p2 = ePos;
                    Quaternion r1 = sRot, r2 = eRot;

                    //Drawing transform handles to move and rotate the points
                    Handles.TransformHandle(ref p1, ref r1);
                    Handles.TransformHandle(ref p2, ref r2);

                    //If position or rotation changed, record it in undo
                    if (p1 != sPos || r1 != sRot || p2 != ePos || r2 != eRot)
                    {
                        //Undo.RecordObject(this, "Tween asset start position changed");
                        UndoExtensions.RegisterPlayableAsset(this, "Tween asset start (or) end position changed");
                        behaviour.startPosition = p1;
                        behaviour.startRotation = r1.eulerAngles;

                        behaviour.endPosition = p2;
                        behaviour.endRotation = r2.eulerAngles;
                        requireRepaint = true;
                    }
                    break;

                case TweenBehaviour.TranslateType.HoldNewPosition:
                    DrawArrow.ForGizmo(sPos, Quaternion.Euler(behaviour.startRotation) * Vector3.forward * .5f, startColor, 2.5f, .15f, 30);

                    Vector3 p = sPos;
                    Quaternion r = sRot;
                    Handles.TransformHandle(ref p, ref r);

                    if (p != sPos || r != sRot)
                    {
                        UndoExtensions.RegisterPlayableAsset(this, "Tween asset start position changed");
                        behaviour.startPosition = p;
                        behaviour.startRotation = r.eulerAngles;
                        requireRepaint = true;
                    }
                    break;

                case TweenBehaviour.TranslateType.FromPreviousClip:
                    DrawArrow.ForGizmo(ePos, Quaternion.Euler(behaviour.endRotation) * Vector3.forward * .5f, endColor, 2.5f, .15f, 30);

                    p = ePos;
                    r = eRot;
                    Handles.TransformHandle(ref p, ref r);

                    if (p != ePos || r != eRot)
                    {
                        UndoExtensions.RegisterPlayableAsset(this, "Tween asset end position changed");
                        behaviour.endPosition = p;
                        behaviour.endRotation = r.eulerAngles;
                        requireRepaint = true;
                    }
                    break;
            }
        }

        //Handles.PositionHandle(sPos, Quaternion.Euler( behaviour.startRotation));
        //Handles.DrawAAPolyLine(new Vector3[] { behaviour.startPosition, behaviour.endPosition });

        switch (behaviour.curveType)
        {
            case TweenBehaviour.BezierType.Linear:
                if (Vector3.Distance(sPos, ePos) >= .1f && behaviour.translateType != TweenBehaviour.TranslateType.HoldNewPosition)
                    DrawArrow.ForGizmoMiddle(behaviour.startPosition, behaviour.endPosition - behaviour.startPosition, Color.white);
                break;

            case TweenBehaviour.BezierType.Quadratic:
                if (Vector3.Distance(sPos, ePos) >= .1f && behaviour.translateType != TweenBehaviour.TranslateType.HoldNewPosition)
                    DrawArrow.ForQuadraticBezierMiddle(behaviour.startPosition, behaviour.endPosition, behaviour.point1, Color.white);
                    //DrawArrow.DrawCubicBezier(behaviour.startPosition, behaviour.endPosition, behaviour.point1, Color.white, 5);
                break;

            case TweenBehaviour.BezierType.Cubic:
                if (Vector3.Distance(sPos, ePos) >= .1f && behaviour.translateType != TweenBehaviour.TranslateType.HoldNewPosition)
                    DrawArrow.ForCubicBezierMiddle(behaviour.startPosition, behaviour.endPosition, behaviour.point1, behaviour.point2, Color.white);
                break;
        }

        return requireRepaint;
    }

    bool showQuadraticHandles()
    {
        Vector3 p1 = behaviour.point1;

        Handles.color = Color.yellow;
        p1 = Handles.FreeMoveHandle(p1, Quaternion.identity, .25f, Vector3.zero, Handles.SphereHandleCap);

        if (behaviour.point1 != p1)
        {
            UndoExtensions.RegisterPlayableAsset(this, "Tween asset point 1 changed");
            behaviour.point1 = p1;
            return true;
        }
        return false;
    }

    bool showCubicHandles()
    {
        bool r1 = showQuadraticHandles();
        bool r2 = false;

        Vector3 p2 = behaviour.point2;
        Handles.color = Color.green;

        p2 = Handles.FreeMoveHandle(p2, Quaternion.identity, .25f, Vector3.zero, Handles.SphereHandleCap);

        if(behaviour.point2 != p2)
        {
            UndoExtensions.RegisterPlayableAsset(this, "Tween asset point 2 changed");
            behaviour.point2 = p2;
            r2 = true;
        }

        return r1 || r2;
    }
}
