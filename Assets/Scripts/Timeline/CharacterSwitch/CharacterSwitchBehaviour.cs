using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;

namespace CustomTracks
{
    [System.Serializable]
    public class CharacterSwitchBehaviour : PlayableBehaviour
    {
        CharacterSwitch ogCharacter = null;

        public void setProperties(CharacterSwitch _og)
        {
            ogCharacter = _og;
        }


        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            if (Application.isPlaying == false || ogCharacter == null) return;

            if (ogCharacter != null)
            {
                ogCharacter.OnClipStart(this);
            }
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
#if UNITY_EDITOR
            if (Application.isPlaying == false) return;
#endif

            if (!ogCharacter) return;

            ogCharacter.OnClipEnd(this);
        }
    }
}
