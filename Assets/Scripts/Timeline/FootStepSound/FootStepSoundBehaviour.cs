using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace CustomTracks
{
    [Serializable]
    public class FootStepSoundBehaviour : PlayableBehaviour
    {
        [HideInInspector] public FaceAnim faceAnim = null;

        public AudioClip Clip;

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif

            if (faceAnim == null)
            {
                return;
            }

            //Set footStepSound in FaceAnim on start of this clip
            faceAnim.footStepSound = Clip;
        }
    }
}
