using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using System;
using CustomExtensions;

namespace CustomTracks
{
    [System.Serializable]
    public class ActionBehaviour : PlayableBehaviour
    {
        public Action action;
        bool isTriggered = false;

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif

            if (action == null || isTriggered) return;

            action?.Invoke();
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif

            if (playable.isPlayableCompleted(info))
                isTriggered = false;
        }
    }
}
