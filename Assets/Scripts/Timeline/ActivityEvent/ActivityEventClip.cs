using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace CustomTracks
{
    [Serializable]
    public class ActivityEventClip : PlayableAsset, ITimelineClipAsset
    {
        public ActivityEventBehaviour behaviour = new ActivityEventBehaviour();

        public ClipCaps clipCaps
        {
            get { return ClipCaps.ClipIn; }
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<ActivityEventBehaviour>.Create(graph, behaviour);
            ActivityEventBehaviour clone = playable.GetBehaviour();
            return playable;
        }
    }
}
