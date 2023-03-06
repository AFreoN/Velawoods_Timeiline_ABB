using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using CustomExtensions;

namespace CustomTracks
{
    [Serializable]
    public class CreateMiniGameBehaviour : PlayableBehaviour
    {
        bool isPlayed = false;

        [HideInInspector] public CreateMiniGame miniGame;
        public string id;
        [Multiline]
        public string title;
        public bool pauseOnFire = true;

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif
            if (miniGame == null || isPlayed) return;

            miniGame.OnClipStart(this); //Fire minigame on start of this clip

            isPlayed = true;
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif

            if (playable.isPlayableCompleted(info))
                isPlayed = false;
        }
    }
}
