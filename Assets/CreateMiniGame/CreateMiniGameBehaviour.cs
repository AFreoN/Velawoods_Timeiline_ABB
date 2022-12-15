using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using CustomExtensions;

[Serializable]
public class CreateMiniGameBehaviour : PlayableBehaviour
{
    bool isPlayed = false;

    public string name;

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying) return;
#endif
        if (isPlayed) return;

        Debug.Log("Playable is : " + playable.GetPlayableType());

        isPlayed = true;
        Debug.Log("Fired MiniGame on : " + name);
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying) return;
#endif

        if (playable.isPlayableCompleted(info))
            isPlayed = false;
    }
}
