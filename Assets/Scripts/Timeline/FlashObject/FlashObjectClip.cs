using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class FlashObjectClip : PlayableAsset, ITimelineClipAsset
{
    public FlashObjectBehaviour behaviour = new FlashObjectBehaviour ();
    public ExposedReference<GameObject> flashObject;

    public ClipCaps clipCaps
    {
        get { return ClipCaps.ClipIn; }
    }

    public override Playable CreatePlayable (PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<FlashObjectBehaviour>.Create (graph, behaviour);
        FlashObjectBehaviour clone = playable.GetBehaviour ();
        clone.flashObject = flashObject.Resolve (graph.GetResolver ());
        return playable;
    }
}
