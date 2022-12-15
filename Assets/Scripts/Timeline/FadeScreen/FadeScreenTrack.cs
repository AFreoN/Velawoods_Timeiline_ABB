using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UI;

[TrackColor(0f, 0f, 0f)]
[TrackClipType(typeof(FadeScreenClip))]
public class FadeScreenTrack : TrackAsset
{
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        PlayableDirector pd = go.GetComponent<PlayableDirector>();
        foreach (TimelineClip clip in this.GetClips())
        {
            double startTime = clip.start;
            double endTime = clip.end;

            FadeScreenClip ta = (FadeScreenClip)clip.asset;
            ta.behaviour.startTime = startTime;
            ta.behaviour.endTime = endTime;
            ta.behaviour.director = pd;
            //Debug.Log("Pd : " + pd.name);
        }

        return ScriptPlayable<FadeScreenBehaviour>.Create (graph, inputCount);
    }
}
