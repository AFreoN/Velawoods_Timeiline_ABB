using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UI;

namespace CustomTracks
{
    [TrackColor(0f, 0f, 0f)]
    [TrackClipType(typeof(FadeScreenClip))]
    public class FadeScreenTrack : TrackAsset
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            PlayableDirector pd = go.GetComponent<PlayableDirector>();

            //Get all clips in this track
            foreach (TimelineClip clip in this.GetClips())
            {
                double startTime = clip.start;
                double endTime = clip.end;

                //Set start time, end time and playable director for timely interpolation
                FadeScreenClip ta = (FadeScreenClip)clip.asset;
                ta.behaviour.startTime = startTime;
                ta.behaviour.endTime = endTime;
                ta.behaviour.director = pd;
            }

            return ScriptPlayable<FadeScreenBehaviour>.Create(graph, inputCount);
        }
    }
}
