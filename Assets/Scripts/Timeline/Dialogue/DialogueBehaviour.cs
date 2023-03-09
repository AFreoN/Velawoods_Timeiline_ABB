using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using CustomExtensions;

namespace CustomTracks
{
    [Serializable]
    public class DialogueBehaviour : PlayableBehaviour
    {
        public DialogueEventData eventData;
        [HideInInspector] public DialogueEvent dialogueEvent;

        //public override void OnPlayableCreate(Playable playable)
        //{
        //    GameObject g = GameObject.Find(DialogueTrack.HOLDER_NAME);
        //    if (g == null)
        //    {
        //        Debug.LogError("No gameobject with name : " + DialogueTrack.HOLDER_NAME + " found!");
        //        return;
        //    }
        //    GameObject holder = new GameObject();
        //    holder.name = "Dialogue " + UnityEngine.Random.Range(01, 4);//holder.GetInstanceID().ToString();
        //    holder.transform.SetParent(g.transform);

        //    dialogueEvent = holder.AddComponent<DialogueEvent>();
        //    eventData = new DialogueEventData();
        //    dialogueEvent.Data = eventData;
        //}

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif

            if (dialogueEvent == null) return;

            dialogueEvent.OnClipStart(this);
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif

            if (dialogueEvent == null) return;

            if (playable.isPlayableCompleted(info))
                dialogueEvent.OnClipEnd(this);
        }

        public override void OnPlayableDestroy(Playable playable)
        {
            //Debug.Log("On playable destroy");
            if(dialogueEvent != null)
            {
                UnityEngine.Object.DestroyImmediate(dialogueEvent.gameObject);
            }
        }
    }
}
