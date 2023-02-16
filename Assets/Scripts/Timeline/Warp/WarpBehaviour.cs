using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class WarpBehaviour : PlayableBehaviour
{
    [HideInInspector] public Transform target;
    [HideInInspector] public Transform objectToWarpTo;
    public bool useObjectRotation;

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying) return;
#endif

        if (target == null || objectToWarpTo == null)
            return;

        target.position = objectToWarpTo.position;

        if (useObjectRotation)
            target.rotation = objectToWarpTo.rotation;
    }
}
