using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace CustomTracks
{
    [Serializable]
    public class WarpClip : PlayableAsset, ITimelineClipAsset
    {
        public ExposedReference<Transform> objectToWarpTo;
        public WarpBehaviour behaviour = new WarpBehaviour();

        public ClipCaps clipCaps
        {
            get { return ClipCaps.ClipIn; }
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<WarpBehaviour>.Create(graph, behaviour);
            WarpBehaviour clone = playable.GetBehaviour();
            clone.objectToWarpTo = objectToWarpTo.Resolve(graph.GetResolver());

            #if CLIENT_BUILD
            if (TimelineController.instance)
            {
                objectToWarpTo.exposedName = System.Guid.NewGuid().ToString();
                TimelineController.instance.getPlayableDirector().SetReferenceValue(objectToWarpTo.exposedName, clone.objectToWarpTo);
            }
            #endif
            return playable;
        }
    }
}
