using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine;

namespace CustomTracks
{
    [TrackClipType(typeof(ActionClip))]
    public class ActionTrack : TrackAsset
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            //return base.CreateTrackMixer(graph, go, inputCount);
            return ScriptPlayable<ActionBehaviour>.Create(graph, inputCount);
        }
    }
}
