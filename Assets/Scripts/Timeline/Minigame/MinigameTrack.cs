using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace CustomTracks
{
    [TrackColor(0.6470588f, 0.4104982f, 0.145098f)]
    [TrackClipType(typeof(MinigameClip))]
    public class MinigameTrack : TrackAsset
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            foreach (var c in GetClips())
            {
                ((MinigameClip)(c.asset)).behaviour.initialized = true;
            }

            return ScriptPlayable<MinigameBehaviour>.Create(graph, inputCount);
        }
    }
}