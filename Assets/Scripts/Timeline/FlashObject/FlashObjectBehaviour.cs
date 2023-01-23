using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using cakeslice;
using CustomExtensions;

[Serializable]
public class FlashObjectBehaviour : PlayableBehaviour
{
    [HideInInspector] public GameObject flashObject;
    bool isTriggered = false;

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying) return;
#endif

        if (flashObject == null || isTriggered) return;

        Outline o = flashObject.GetComponent<Outline>();

        if (o == null)
        {
            isTriggered = true;

            flashObject.AddComponent<Outline>();    //Adds highlight to the object
            flashObject.AddComponent<ObjectClick>();        //Check click action
            TimelineController.instance.PauseTimeline();
        }
        else
            Debug.Log("Flash object already contains Outline script");
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying) return;
#endif

        if (playable.isPlayableCompleted(info))
            isTriggered = false;
    }
}
