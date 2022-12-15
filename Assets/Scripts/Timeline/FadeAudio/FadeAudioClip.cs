using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
[TrackClipType(typeof(FadeAudioBehaviour))]
public class FadeAudioClip : PlayableAsset, ITimelineClipAsset
{
    public FadeAudioBehaviour behaviour = new FadeAudioBehaviour ();

    public ClipCaps clipCaps
    {
        get { return ClipCaps.ClipIn; }
    }

    public override Playable CreatePlayable (PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<FadeAudioBehaviour>.Create (graph, behaviour);
        FadeAudioBehaviour clone = playable.GetBehaviour ();
        return playable;
    }
}
