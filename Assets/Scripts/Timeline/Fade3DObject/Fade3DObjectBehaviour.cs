using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System.Collections.Generic;
using CustomExtensions;

[Serializable]
public class Fade3DObjectBehaviour : PlayableBehaviour
{
    [HideInInspector] public List<GameObject> fadeObjects = new List<GameObject>();

    public void addFadeObject(GameObject g)
    {
        if (g == null || fadeObjects.Contains(g)) return;

        fadeObjects.Add(g);
    }

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying) return;
#endif

        if (fadeObjects == null || fadeObjects.Count == 0) return;

        foreach (GameObject g in fadeObjects)
        {
            g.SetActive(true);
        }
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying) return;
#endif

        if (fadeObjects == null || fadeObjects.Count == 0) return;

        if(playable.isPlayableCompleted(info))
        {
            foreach (GameObject g in fadeObjects)
                g.SetActive(false);
        }
    }

    public override void OnGraphStop(Playable playable)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying) return;
#endif

        if (fadeObjects == null) return;

        foreach (GameObject g in fadeObjects)
            g.SetActive(false);

        fadeObjects.Clear();
    }
}
