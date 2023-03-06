using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace CustomTracks
{
    [Serializable]
    public class FreeSpinCameraBehaviour : PlayableBehaviour
    {
        [HideInInspector] public CameraApartmentController controller = null;

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif

            if (controller == null) return;

            controller.FreeSpinActive(null);
            //TimelineController.instance.PauseTimeline();
        }
    }
}
