using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class WarpClip : PlayableAsset, ITimelineClipAsset
{
    public ExposedReference<Transform> objectToWarpTo;
    public WarpBehaviour behaviour = new WarpBehaviour ();

    public ClipCaps clipCaps
    {
        get { return ClipCaps.ClipIn; }
    }

    public override Playable CreatePlayable (PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<WarpBehaviour>.Create (graph, behaviour);
        WarpBehaviour clone = playable.GetBehaviour ();
        clone.objectToWarpTo = objectToWarpTo.Resolve (graph.GetResolver ());
        return playable;
    }
}
