using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackColor(0.9245283f, 0.6098064f, 0.4055713f)]
[TrackClipType(typeof(CreateMiniGameClip))]
public class CreateMiniGameTrack : TrackAsset
{
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        return ScriptPlayable<CreateMiniGameBehaviour>.Create (graph, inputCount);
    }
}
