using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;

namespace CustomTracks
{
    [TrackClipType(typeof(ActionBehaviour))]
    public class ActionClip : PlayableAsset
    {
        [HideInInspector]
        public ActionBehaviour behaviour = new ActionBehaviour();

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<ActionBehaviour>.Create(graph, behaviour);
            return playable;
        }
    }
}
