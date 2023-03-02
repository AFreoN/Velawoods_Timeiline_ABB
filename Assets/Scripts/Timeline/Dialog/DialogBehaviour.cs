using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using CustomExtensions;

[System.Serializable]
public class DialogBehaviour : PlayableBehaviour
{
    public DialogEventManager dialogManager;
    public GameObject character;
    public string animationClipName;
    public AudioClip audioClip;
    public string subtitle;
    public bool isLearner;
    public bool isTutorial;

    public void setProperties(DialogEventManager _dialogManager, GameObject _character, string _animationClipName, AudioClip _audioClip, string _subtitle,bool _isLearner, bool _isTutorial)
    {
        dialogManager = _dialogManager;
        character = _character;
        animationClipName = _animationClipName;
        audioClip = _audioClip;
        subtitle = _subtitle;
        isLearner = _isLearner;
        isTutorial = _isTutorial;
    }

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying) return;
#endif

        if (dialogManager != null)
            dialogManager.OnClipStart(this);    //Show subtitle, it's audio and facial animation on start of this clip
    }


    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying) return;
#endif

        if (playable.isPlayableCompleted(info))
            dialogManager.OnClipEnd(this);      //Disable subtitle on end of this clip
    }
}
