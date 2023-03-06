using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace CustomTracks
{
    [TrackColor(0.8679245f, 0.7786266f, 0.6427554f)]
    [TrackClipType(typeof(FootStepSoundClip))]
    [TrackBindingType(typeof(FaceAnim))]
    public class FootStepSoundTrack : TrackAsset
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            FaceAnim fa = (FaceAnim)go.GetComponent<PlayableDirector>().GetGenericBinding(this);

            //Get all clips in this track and set it's clips faceAnim value to the one attached in this track
            foreach (var c in GetClips())
            {
                ((FootStepSoundClip)(c.asset)).behaviour.faceAnim = fa;
            }

            return ScriptPlayable<FootStepSoundBehaviour>.Create(graph, inputCount);
        }
    }
}
