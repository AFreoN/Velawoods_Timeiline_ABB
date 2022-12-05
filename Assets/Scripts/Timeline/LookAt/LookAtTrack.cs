using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;

[TrackClipType(typeof(LookAtAsset))]
[TrackBindingType(typeof(FaceLookAt))]
public class LookAtTrack : TrackAsset
{
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject gameObject, int inputCount)
    {
        FaceLookAt lookAt = (FaceLookAt)gameObject.GetComponent<PlayableDirector>().GetGenericBinding(this);

        foreach(var c in GetClips())
        {
            ((LookAtAsset)(c.asset)).faceLookAt = lookAt;
        }

        return ScriptPlayable<LookAtBehaviour>.Create(graph, inputCount);
    }
}
