using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
[TrackClipType(typeof(AttachToParentBehaviour))]
public class AttachToParentClip : PlayableAsset, ITimelineClipAsset
{
    public ExposedReference<Transform> parentObject;    //parent object to attach to

    [Space(10)]
    public AttachToParentBehaviour template = new AttachToParentBehaviour ();

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
