using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;

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

        if(ogCharacter != null)
        {
            ogCharacter.OnClipStart(this);
        }
    }
}
