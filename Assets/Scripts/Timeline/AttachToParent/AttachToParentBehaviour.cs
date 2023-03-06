using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using CustomExtensions;

namespace CustomTracks
{
    [Serializable]
    public class AttachToParentBehaviour : PlayableBehaviour
    {
        [HideInInspector] public Transform targetTransform;     //Transform that's binded in this track
        [HideInInspector] public Transform parentObject;    //Parent object to attach to
        public bool attachToParent = true;
        public bool WorldPositionStays = true;

        public bool warpPosition = false;   //If true, change position of the target transform to it's parent position
        public bool warpRotation = false;   //If true, change rotation of the target transform to it's parent rotation

        #region For resetting
        Transform previousParent;
        Vector3 oldPosition;
        #endregion

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            if (!Application.isPlaying) return;
            if (parentObject == null || targetTransform == null)
            {
                return;
            }

            if (attachToParent)
                targetTransform.SetParent(parentObject, WorldPositionStays);

            if (warpPosition)
                targetTransform.position = parentObject.position;

            if (warpRotation)
                targetTransform.rotation = parentObject.rotation;
        }

        //public override void OnBehaviourPause(Playable playable, FrameData info)
        //{
        //    if (!Application.isPlaying) return;

        //}

        //public override void OnGraphStart(Playable playable)
        //{
        //    if (targetTransform == null) return;
        //    previousParent = targetTransform.parent;
        //    oldPosition = targetTransform.position;
        //}

        //public override void OnGraphStop(Playable playable)
        //{
        //    if (targetTransform == null) return;
        //    targetTransform.SetParent(previousParent);
        //    targetTransform.position = oldPosition;
        //}
    }
}
