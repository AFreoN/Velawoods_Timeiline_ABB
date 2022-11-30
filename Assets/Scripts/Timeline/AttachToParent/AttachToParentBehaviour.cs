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

    public override void OnPlayableCreate (Playable playable)
    {
        
    }

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
}
