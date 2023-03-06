using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace CustomTracks
{
    [TrackColor(0.855f, 0.8623f, 0.87f)]
    [TrackClipType(typeof(FreeSpinCameraClip))]
    [TrackBindingType(typeof(CameraApartmentController))]
    public class FreeSpinCameraTrack : TrackAsset
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            CameraApartmentController cac = (CameraApartmentController)go.GetComponent<PlayableDirector>().GetGenericBinding(this);

            foreach (var c in GetClips())
            {
                ((FreeSpinCameraClip)(c.asset)).behaviour.controller = cac;
            }

            return ScriptPlayable<FreeSpinCameraBehaviour>.Create(graph, inputCount);
        }
    }
}
