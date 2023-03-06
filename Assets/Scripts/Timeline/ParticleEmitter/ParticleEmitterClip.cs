using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace CustomTracks
{
    [Serializable]
    public class ParticleEmitterClip : PlayableAsset, ITimelineClipAsset
    {
        public ParticleEmitterBehaviour behaviour = new ParticleEmitterBehaviour();

        public ClipCaps clipCaps
        {
            get { return ClipCaps.ClipIn; }
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<ParticleEmitterBehaviour>.Create(graph, behaviour);
            ParticleEmitterBehaviour clone = playable.GetBehaviour();
            return playable;
        }
    }
}
