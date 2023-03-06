using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace CustomTracks
{
    [TrackColor(0.990566f, 0.3085479f, 0.2009167f)]
    [TrackClipType(typeof(FlashObjectClip))]
    public class FlashObjectTrack : TrackAsset
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            return ScriptPlayable<FlashObjectBehaviour>.Create(graph, inputCount);
        }
    }
}
