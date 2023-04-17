using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEditor;

namespace CustomTracks
{
    [Serializable]
    [TrackClipType(typeof(DialogueBehaviour))]
    public class DialogueClip : PlayableAsset, ITimelineClipAsset
    {
        public ExposedReference<GameObject> characterExposed;

        public DialogueBehaviour behaviour = new DialogueBehaviour();

        [HideInInspector] public float startTime, endTime;

        [HideInInspector]
        public ExposedReference<GameObject> holderExposed;

        public static GameObject holderInstance;

        public ClipCaps clipCaps
        {
            get { return ClipCaps.ClipIn; }
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<DialogueBehaviour>.Create(graph, behaviour);
            DialogueBehaviour clone = playable.GetBehaviour();

            GameObject holder = holderExposed.Resolve(graph.GetResolver());
#if CLIENT_BUILD
            holder = null;
#endif
            if (holder == null)
            {
                GameObject g = GameObject.Find(DialogueTrack.HOLDER_NAME);
                if (!g)
                    g = new GameObject(DialogueTrack.HOLDER_NAME);

                g.isStatic = true;
                g.hideFlags = HideFlags.HideAndDontSave;

                if (g == null)
                {
                    Debug.LogError("No gameobject with name : " + DialogueTrack.HOLDER_NAME + " found!");
                }
                else
                {
                    holder = new GameObject();
                    holder.isStatic = true;
                    holder.hideFlags = HideFlags.HideAndDontSave;
                    holder.transform.SetParent(g.transform);

                    PlayableDirector pd = owner.GetComponent<PlayableDirector>();
                    owner.GetComponent<PlayableDirector>().ClearReferenceValue(holderExposed.exposedName);
                    holderExposed.exposedName = System.Guid.NewGuid().ToString();
                    owner.GetComponent<PlayableDirector>().SetReferenceValue(holderExposed.exposedName, holder);

#if CLIENT_BUILD
                    GameObject loadedCharacter = characterExposed.Resolve(graph.GetResolver());
                    if(loadedCharacter == null && clone.eventData.dialogueData.character != null)
                    {
                        loadedCharacter = clone.eventData.dialogueData.character;
                        //pd.ClearReferenceValue(characterExposed.exposedName);
                        characterExposed.exposedName = System.Guid.NewGuid().ToString();
                        pd.SetReferenceValue(characterExposed.exposedName, loadedCharacter);
                    }
#endif

                    clone.dialogueEvent = holder.AddComponent<DialogueEvent>();
                    //clone.eventData = new DialogueEventData();
                    clone.dialogueEvent.Data = clone.eventData;
                    clone.dialogueEvent.startTime = startTime;
                    clone.dialogueEvent.endTime = endTime;
                    clone.startTime = startTime;
                    clone.endTime = endTime;
                    clone.dialogueEvent.Data.dialogueData.character = characterExposed.Resolve(graph.GetResolver());
#if CLIENT_BUILD
                    clone.dialogueEvent.Data.dialogueData.character = loadedCharacter;
#endif

                    holder.name = "DD";
                    if (clone.dialogueEvent.Data.dialogueData.dialogueText.Count > 0)
                        holder.name = clone.dialogueEvent.Data.dialogueData.dialogueText[0].text;
                }
            }

            return playable;
        }
    }
}
