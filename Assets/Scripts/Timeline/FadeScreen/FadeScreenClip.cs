using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UI;

[Serializable]
[TrackClipType(typeof(FadeScreenBehaviour))]
public class FadeScreenClip : PlayableAsset, ITimelineClipAsset
{
    public FadeScreenBehaviour behaviour = new FadeScreenBehaviour ();

    public ClipCaps clipCaps
    {
        get { return ClipCaps.ClipIn; }
    }

    public override Playable CreatePlayable (PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<FadeScreenBehaviour>.Create (graph, behaviour);
        return playable;
    }
}
