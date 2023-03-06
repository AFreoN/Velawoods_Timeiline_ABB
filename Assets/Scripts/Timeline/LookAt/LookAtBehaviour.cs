using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using CustomExtensions;

namespace CustomTracks
{
    [System.Serializable]
    public class LookAtBehaviour : PlayableBehaviour
    {
        FaceLookAt flt = null;      //FaceLookAt this track is binded to
        public Transform target = null;     //Target transform to look at
        public LookType lookType;

        bool isPlayed = false;

        public void setProperties(FaceLookAt _flt, Transform _target, LookType _lookType)
        {
            flt = _flt;
            target = _target;
            lookType = _lookType;
        }

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            if (Application.isPlaying && flt != null && !isPlayed)
            {
                flt.OnClipStart(this);
                isPlayed = true;
            }
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            if (Application.isPlaying == false || flt == null || !isPlayed) return;
            if (playable.isPlayableCompleted(info))
            {
                flt.OnClipEnd(this);
                isPlayed = false;
            }
        }
    }
}
