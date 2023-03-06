using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace CustomTracks
{
    [TrackColor(0.855f, 0.8623f, 0.87f)]
    [TrackClipType(typeof(AttachToParentClip))]
    [TrackBindingType(typeof(Transform))]
    public class AttachToParentTrack : TrackAsset
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            Transform t = (Transform)go.GetComponent<PlayableDirector>().GetGenericBinding(this);

            //Get all clips in this track and set it's clip target transform value to the one attached in this track
            foreach (var c in GetClips())
            {
                ((AttachToParentClip)(c.asset)).template.targetTransform = t;
            }

            return ScriptPlayable<AttachToParentBehaviour>.Create(graph, inputCount);
        }
    }
}
