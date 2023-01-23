using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackColor(0.9245283f, 0.6098064f, 0.4055713f)]
[TrackClipType(typeof(CreateMiniGameClip))]
[TrackBindingType(typeof(CreateMiniGame))]
public class CreateMiniGameTrack : TrackAsset
{
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        CreateMiniGame cmg = (CreateMiniGame)go.GetComponent<PlayableDirector>().GetGenericBinding(this);

        //Get all clips in this track and set it's clips miniGame value to the one attached in this track
        foreach (var c in GetClips())
        {
            ((CreateMiniGameClip)(c.asset)).behaviour.miniGame = cmg;
        }

        return ScriptPlayable<CreateMiniGameBehaviour>.Create (graph, inputCount);
    }
}
