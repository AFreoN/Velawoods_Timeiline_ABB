using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;

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
        behaviour.setProperties(faceLookAt, target.Resolve(graph.GetResolver()), type);
        var playable = ScriptPlayable<LookAtBehaviour>.Create(graph, behaviour);
        return playable;
    }
}

public enum LookType
{
    Face,
    Target,
    FreeLook
}
