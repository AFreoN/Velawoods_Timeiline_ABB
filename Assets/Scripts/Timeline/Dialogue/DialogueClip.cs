using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace CustomTracks
{
    [Serializable]
    public class DialogueClip : PlayableAsset, ITimelineClipAsset
    {
        public ExposedReference<GameObject> characterExposed;

        public DialogueBehaviour behaviour = new DialogueBehaviour();

        [HideInInspector] public float startTime, endTime;

        [HideInInspector] public ExposedReference<GameObject> holderExposed;

        public ClipCaps clipCaps
        {
            get { return ClipCaps.ClipIn; }
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<DialogueBehaviour>.Create(graph, behaviour);
            DialogueBehaviour clone = playable.GetBehaviour();

            GameObject holder = holderExposed.Resolve(graph.GetResolver());
            if(holder == null)
            {
                GameObject g = GameObject.Find(DialogueTrack.HOLDER_NAME);
                if (g == null)
                {
                    Debug.LogError("No gameobject with name : " + DialogueTrack.HOLDER_NAME + " found!");
                }
                else
                {
                    holder = new GameObject();
                    holder.transform.SetParent(g.transform);


                    owner.GetComponent<PlayableDirector>().ClearReferenceValue(holderExposed.exposedName);
                    holderExposed.exposedName = UnityEditor.GUID.Generate().ToString();
                    owner.GetComponent<PlayableDirector>().SetReferenceValue(holderExposed.exposedName, holder);

                    clone.dialogueEvent = holder.AddComponent<DialogueEvent>();
                    //clone.eventData = new DialogueEventData();
                    clone.dialogueEvent.Data = clone.eventData;
                    clone.dialogueEvent.startTime = startTime;
                    clone.dialogueEvent.endTime = endTime;
                    clone.dialogueEvent.Data.dialogueData.character = characterExposed.Resolve(graph.GetResolver());

                    //string n = clone.dialogueEvent.Data.dialogueData.dialogueText.Count > 0 ? clone.dialogueEvent.Data.dialogueData.dialogueText[0].text : "No Dialogue";
                    holder.name = "DD"; //"Dialogue " + UnityEngine.Random.Range(01, 4);//holder.GetInstanceID().ToString();
                }
            }


            return playable;
        }
    }
}
