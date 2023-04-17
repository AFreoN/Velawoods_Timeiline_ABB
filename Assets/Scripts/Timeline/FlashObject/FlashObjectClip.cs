using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace CustomTracks
{
    [Serializable]
    public class FlashObjectClip : PlayableAsset, ITimelineClipAsset
    {
        public FlashObjectBehaviour behaviour = new FlashObjectBehaviour();
        public ExposedReference<GameObject> flashObject;

        public ClipCaps clipCaps
        {
            get { return ClipCaps.ClipIn; }
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<FlashObjectBehaviour>.Create(graph, behaviour);
            FlashObjectBehaviour clone = playable.GetBehaviour();
            clone.flashObject = flashObject.Resolve(graph.GetResolver());

#if CLIENT_BUILD
            if (TimelineController.instance)
            {
                flashObject.exposedName = System.Guid.NewGuid().ToString();
                TimelineController.instance.getPlayableDirector().SetReferenceValue(flashObject.exposedName, clone.flashObject);
            }
#endif
            return playable;
        }
    }
}
