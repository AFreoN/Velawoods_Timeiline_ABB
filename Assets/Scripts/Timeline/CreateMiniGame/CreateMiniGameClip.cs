using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
[TrackClipType(typeof(CreateMiniGameBehaviour))]
public class CreateMiniGameClip : PlayableAsset, ITimelineClipAsset
{
    public CreateMiniGameBehaviour behaviour = new CreateMiniGameBehaviour ();

    public ClipCaps clipCaps
    {
        get { return ClipCaps.None; }
    }

    public override Playable CreatePlayable (PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<CreateMiniGameBehaviour>.Create (graph, behaviour);
        CreateMiniGameBehaviour clone = playable.GetBehaviour ();
        return playable;
    }
}
