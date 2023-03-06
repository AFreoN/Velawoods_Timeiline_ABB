using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace CustomTracks
{
    [Serializable]
    public class WarpBehaviour : PlayableBehaviour
    {
        [HideInInspector] public Transform target;
        [HideInInspector] public Transform objectToWarpTo;
        public bool useObjectRotation;

        #region For resetting transform
        Vector3 resetPosition = Vector3.zero;
        Quaternion resetRotation = Quaternion.identity;
        #endregion

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            //#if UNITY_EDITOR
            //        if (!Application.isPlaying) return;
            //#endif

            if (target == null || objectToWarpTo == null)
                return;

            target.position = objectToWarpTo.position;

            if (useObjectRotation)
                target.rotation = objectToWarpTo.rotation;
        }

        public override void OnGraphStart(Playable playable)
        {
            if (target == null) return;
            resetPosition = target.position;
            resetRotation = target.rotation;
        }

        public override void OnGraphStop(Playable playable)
        {
            if (target == null) return;
            target.position = resetPosition;
            target.rotation = resetRotation;
        }
    }
}
