using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class FootStepSoundClip : PlayableAsset, ITimelineClipAsset
{
    public FootStepSoundBehaviour behaviour = new FootStepSoundBehaviour ();

    public ClipCaps clipCaps
    {
        get { return ClipCaps.ClipIn; }
    }

    public override Playable CreatePlayable (PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<FootStepSoundBehaviour>.Create (graph, behaviour);
        FootStepSoundBehaviour clone = playable.GetBehaviour ();
        return playable;
    }
}
