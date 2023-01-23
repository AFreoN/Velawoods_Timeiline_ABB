using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class ParticleEmitterBehaviour : PlayableBehaviour
{
    [HideInInspector] public ParticleSystem particleSystem = null;
    public bool Play;

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying) return;
#endif

        if (particleSystem == null) return;

        if (Play)
        {
            particleSystem.Play();  //Play particle system on start of this clip, if play is true
        }
        else
        {
            particleSystem.Clear();
            particleSystem.Stop();      //Stop particle system on star of this clip, if play is false
        }
    }
}
