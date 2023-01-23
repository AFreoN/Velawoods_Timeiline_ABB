using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackColor(0.855f, 0.8623f, 0.87f)]
[TrackClipType(typeof(ParticleEmitterClip))]
[TrackBindingType(typeof(ParticleSystem))]
public class ParticleEmitterTrack : TrackAsset
{
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        ParticleSystem ps = (ParticleSystem)go.GetComponent<PlayableDirector>().GetGenericBinding(this);

        //Get all clips in this track and set it's clips particleSystem value to the one attached in this track
        foreach (var c in GetClips())
        {
            ((ParticleEmitterClip)(c.asset)).behaviour.particleSystem = ps;
        }

        return ScriptPlayable<ParticleEmitterBehaviour>.Create (graph, inputCount);
    }
}
