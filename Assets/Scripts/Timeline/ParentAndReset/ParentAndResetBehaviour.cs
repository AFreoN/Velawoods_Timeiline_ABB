using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using CustomExtensions;

namespace CustomTracks
{
    [Serializable]
    public class ParentAndResetBehaviour : PlayableBehaviour
    {
        [HideInInspector] public Transform target;
        [HideInInspector] public Transform parent;
        public bool resetOnClipEnd = true;
        public Vector3 Rotation;
        public Vector3 Offset;

        Transform previousParent = null;
        Vector2 previousPosition, previousRotation;

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif

            if (parent == null || target == null)
                return;

            previousParent = target.parent;
            previousPosition = target.localPosition;
            previousRotation = target.localRotation.eulerAngles;

            target.parent = parent;
            target.localPosition = Offset;
            target.localRotation = Quaternion.Euler(Rotation);
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif

            if (resetOnClipEnd == false || parent == null || target == null)
                return;

            if (playable.isPlayableCompleted(info) && previousParent != null)
            {
                target.SetParent(previousParent);
                target.localPosition = previousPosition;
                target.localRotation = Quaternion.Euler(previousRotation);

                previousParent = null;
            }
        }
    }
}
