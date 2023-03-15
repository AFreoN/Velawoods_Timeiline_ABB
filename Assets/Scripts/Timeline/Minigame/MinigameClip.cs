using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace CustomTracks
{
    [Serializable]
    public class MinigameClip : PlayableAsset, ITimelineClipAsset
    {
        public MinigameBehaviour behaviour = new MinigameBehaviour();

        public ClipCaps clipCaps
        {
            get { return ClipCaps.ClipIn; }
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<MinigameBehaviour>.Create(graph, behaviour);
            MinigameBehaviour clone = playable.GetBehaviour();
            return playable;
        }
    }
}
