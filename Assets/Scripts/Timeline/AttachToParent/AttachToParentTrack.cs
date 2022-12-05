using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackColor(0.855f, 0.8623f, 0.87f)]
[TrackClipType(typeof(AttachToParentClip))]
[TrackBindingType(typeof(Transform))]
public class AttachToParentTrack : TrackAsset
{
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        Transform t = (Transform)go.GetComponent<PlayableDirector>().GetGenericBinding(this);

        foreach (var c in GetClips())
        {
            ((AttachToParentClip)(c.asset)).template.targetTransform = t;
        }

        return ScriptPlayable<AttachToParentBehaviour>.Create (graph, inputCount);
    }
}
