using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
[TrackClipType(typeof(AttachToParentBehaviour))]
public class AttachToParentClip : PlayableAsset, ITimelineClipAsset
{
    public AttachToParentBehaviour template = new AttachToParentBehaviour ();
    public ExposedReference<Transform> parentObject;

    public ClipCaps clipCaps
    {
        get { return ClipCaps.ClipIn; }
    }

    public override Playable CreatePlayable (PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<AttachToParentBehaviour>.Create (graph, template);
        AttachToParentBehaviour clone = playable.GetBehaviour ();
        clone.parentObject = parentObject.Resolve (graph.GetResolver ());
        return playable;
    }
}
