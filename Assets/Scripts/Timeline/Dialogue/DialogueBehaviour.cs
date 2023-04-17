using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using CustomExtensions;

namespace CustomTracks
{
    [Serializable]
    public class DialogueBehaviour : PlayableBehaviour, ITimelineBehaviour
    {
        public DialogueEventData eventData;
        [HideInInspector] public DialogueEvent dialogueEvent;

        bool isTriggered = false;

        public double startTime { get; set; }
        public double endTime { get; set; }

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif

            if (dialogueEvent == null || isTriggered) return;

            isTriggered = true;
            PlayableInstance.AddPlayable(this);
            dialogueEvent.OnClipStart(this);
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif

            if (dialogueEvent == null) return;

            if (playable.isPlayableCompleted(info))
            {
                isTriggered = false;
                PlayableInstance.RemovePlayable(this);
                dialogueEvent.OnClipEnd(this);
            }
        }

        public override void OnPlayableDestroy(Playable playable)
        {
            if(dialogueEvent != null)
            {
                UnityEngine.Object.DestroyImmediate(dialogueEvent.gameObject);  //Delete Holder DialogueEvent Object
            }
        }

        public void OnSkip()
        {
            isTriggered = false;
        }
    }
}
