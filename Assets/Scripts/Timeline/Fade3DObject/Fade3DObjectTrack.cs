using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackColor(0.3859858f, 0.2031417f, 0.7830189f)]
[TrackClipType(typeof(Fade3DObjectClip))]
public class Fade3DObjectTrack : TrackAsset
{
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        foreach(var c in GetClips())
        {
            ((Fade3DObjectClip)(c.asset)).behaviour.startTime = c.start;
            ((Fade3DObjectClip)(c.asset)).behaviour.endTime = c.end;
        }
        return ScriptPlayable<Fade3DObjectBehaviour>.Create (graph, inputCount);
    }
}
