using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using CustomExtensions;

[Serializable]
public class AttachToParentBehaviour : PlayableBehaviour
{
    [HideInInspector] public Transform targetTransform;
    [HideInInspector] public Transform parentObject;
    public bool WorldPositionStays = true;

    #region For resetting
    Transform previousParent;
    Vector3 oldPosition;
    #endregion

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        if (!Application.isPlaying) return;
        if (parentObject == null || targetTransform == null)
        {
            return;
        }

        targetTransform.SetParent(parentObject, WorldPositionStays);
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        if (!Application.isPlaying) return;

    }

    public override void OnGraphStart(Playable playable)
    {
        if (targetTransform == null) return;
        previousParent = targetTransform.parent;
        oldPosition = targetTransform.position;
    }

    public override void OnGraphStop(Playable playable)
    {
        if (targetTransform == null) return;
        targetTransform.SetParent(previousParent);
        targetTransform.position = oldPosition;
    }
}
