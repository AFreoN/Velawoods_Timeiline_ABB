using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackColor(0.855f, 0.8623f, 0.87f)]
[TrackClipType(typeof(FadeAudioClip))]
[TrackBindingType(typeof(AudioSource))]
public class FadeAudioTrack : TrackAsset
{
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        PlayableDirector pd = go.GetComponent<PlayableDirector>();
        AudioSource source = (AudioSource)pd.GetGenericBinding(this);

        //Get all clips in this track and set it's clips audioSource value to the one attached in this track
        foreach (var v in GetClips())
        {
            FadeAudioBehaviour behaviour = ((FadeAudioClip)(v.asset)).behaviour;
            behaviour.audioSource = source;

            //Set start time, end time and playable director for timely interpolation
            behaviour.startTime = v.start;
            behaviour.endTime = v.end;
            behaviour.director = pd;
        }

        return ScriptPlayable<FadeAudioBehaviour>.Create (graph, inputCount);
    }
}
