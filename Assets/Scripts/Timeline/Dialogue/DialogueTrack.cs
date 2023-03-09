using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace CustomTracks
{
    [TrackColor(1f, 0.9823553f, 0.259434f)]
    [TrackClipType(typeof(DialogueClip))]
    public class DialogueTrack : TrackAsset
    {
        public const string HOLDER_NAME = "ConversationEvent_Holder";

        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            foreach (var c in GetClips())
            {
                ((DialogueClip)(c.asset)).startTime = (float)c.start;
                ((DialogueClip)(c.asset)).endTime = (float)c.duration + (float)c.start;
            }

            return ScriptPlayable<DialogueBehaviour>.Create(graph, inputCount);
        }
    }
}
