using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace CustomTracks
{
    [TrackColor(0.855f, 0.8623f, 0.87f)]
    [TrackClipType(typeof(ParentAndResetClip))]
    [TrackBindingType(typeof(Transform))]
    public class ParentAndResetTrack : TrackAsset
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            Transform t = (Transform)go.GetComponent<PlayableDirector>().GetGenericBinding(this);

            foreach (var c in GetClips())
            {
                ((ParentAndResetClip)(c.asset)).behaviour.target = t;
            }

            return ScriptPlayable<ParentAndResetBehaviour>.Create(graph, inputCount);
        }
    }
}
