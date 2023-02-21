using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackColor(0.5f, 0.5f, 0.87f)]
[TrackClipType(typeof(TouchAndJumpClip))]
public class TouchAndJumpTrack : TrackAsset
{
    public bool pauseTimeline = true;

    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        foreach (var c in GetClips())
            ((TouchAndJumpClip)(c.asset)).behaviour.pause = pauseTimeline;

        return ScriptPlayable<TouchAndJumpBehaviour>.Create (graph, inputCount);
    }
}
