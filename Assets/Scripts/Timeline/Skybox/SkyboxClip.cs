using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace CustomTracks
{
    [Serializable]
    public class SkyboxClip : PlayableAsset, ITimelineClipAsset
    {
        public SkyboxBehaviour behaviour = new SkyboxBehaviour();

        public ClipCaps clipCaps
        {
            get { return ClipCaps.ClipIn; }
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<SkyboxBehaviour>.Create(graph, behaviour);
            SkyboxBehaviour clone = playable.GetBehaviour();
            return playable;
        }
    }
}
