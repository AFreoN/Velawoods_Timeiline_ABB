using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace CustomTracks
{
    [Serializable]
    public class FreeSpinCameraClip : PlayableAsset, ITimelineClipAsset
    {
        [HideInInspector] public FreeSpinCameraBehaviour behaviour = new FreeSpinCameraBehaviour();

        public ClipCaps clipCaps
        {
            get { return ClipCaps.ClipIn; }
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<FreeSpinCameraBehaviour>.Create(graph, behaviour);
            FreeSpinCameraBehaviour clone = playable.GetBehaviour();
            return playable;
        }

        [Serializable]
        public class ExposedGameObject : ExposedReferenceHolder<GameObject>
        {
        }
    }
}
