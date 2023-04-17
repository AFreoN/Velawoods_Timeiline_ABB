using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace CustomTracks
{
    [Serializable]
    public class ParentAndResetClip : PlayableAsset, ITimelineClipAsset
    {
        public ExposedReference<Transform> parent;
        public ParentAndResetBehaviour behaviour = new ParentAndResetBehaviour();

        public ClipCaps clipCaps
        {
            get { return ClipCaps.ClipIn; }
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<ParentAndResetBehaviour>.Create(graph, behaviour);
            ParentAndResetBehaviour clone = playable.GetBehaviour();
            clone.parent = parent.Resolve(graph.GetResolver());

            #if CLIENT_BUILD
            if (TimelineController.instance)
            {
                parent.exposedName = System.Guid.NewGuid().ToString();
                TimelineController.instance.getPlayableDirector().SetReferenceValue(parent.exposedName, clone.parent);
            }
            #endif

            return playable;
        }
    }
}
