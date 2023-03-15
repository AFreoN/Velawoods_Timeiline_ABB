using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace CustomTracks
{
    [TrackColor(0.5235849f, 0.9585208f, 1f)]
    [TrackClipType(typeof(SkyboxClip))]
    public class SkyboxTrack : TrackAsset
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            return ScriptPlayable<SkyboxBehaviour>.Create(graph, inputCount);
        }
    }
}
