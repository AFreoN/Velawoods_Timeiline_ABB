using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;

namespace CustomTracks
{
    [TrackClipType(typeof(LookAtBehaviour))]
#if UNITY_EDITOR
    [UnityEditor.CanEditMultipleObjects]
#endif
    public class LookAtAsset : PlayableAsset, ITimelineClipAsset
    {
        public ClipCaps clipCaps => ClipCaps.ClipIn;

        [HideInInspector] public FaceLookAt faceLookAt = null; //Set by LookAtTrack.cs while loading

        public LookType type = LookType.Face;

        public ExposedReference<Transform> target;


        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var behaviour = new LookAtBehaviour();
            Transform t = target.Resolve(graph.GetResolver());
            behaviour.setProperties(faceLookAt, t, type);
            var playable = ScriptPlayable<LookAtBehaviour>.Create(graph, behaviour);

#if CLIENT_BUILD
            if (TimelineController.instance)
            {
                target.exposedName = System.Guid.NewGuid().ToString();
                TimelineController.instance.getPlayableDirector().SetReferenceValue(target.exposedName, t);
            }
#endif
            return playable;
        }
    }

    public enum LookType
    {
        Face,
        Target,
        FreeLook
    }
}
