using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;

namespace CustomTracks
{
    [TrackColor(1, 1, 0)]
    [TrackClipType(typeof(DialogAsset))]
    [TrackBindingType(typeof(DialogEventManager))]
    public class DialogTrack : TrackAsset
    {

        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            DialogEventManager d = (DialogEventManager)go.GetComponent<PlayableDirector>().GetGenericBinding(this);

            //Get all clips in this track and set it's clips dialogManger value to the one attached in this track
            foreach (var c in GetClips())
            {
                ((DialogAsset)(c.asset)).dialogManager = d;
            }

            //return base.CreateTrackMixer(graph, go, inputCount);
            return ScriptPlayable<DialogBehaviour>.Create(graph, inputCount);
        }
    }
}
