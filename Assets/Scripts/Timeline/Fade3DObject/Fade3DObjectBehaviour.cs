using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System.Collections.Generic;
using CustomExtensions;

namespace CustomTracks
{
    [System.Serializable]
    public class Fade3DObjectBehaviour : PlayableBehaviour, ITimelineBehaviour
    {
        [HideInInspector] public List<GameObject> fadeObjects = new List<GameObject>();

        public double startTime { get; set; }
        public double endTime { get; set; }

        public float fadeTime = 0.5f;
        bool isTriggered = false;


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

            if (isTriggered || fadeObjects == null || fadeObjects.Count == 0) return;

            foreach (GameObject g in fadeObjects)
            {
                //g.SetActive(true);

                FadeNoTouch fadeNoTouch = g.GetComponent<FadeNoTouch>();

                if(fadeNoTouch != null)
                {
                    Object.DestroyImmediate(fadeNoTouch);
                }

                fadeNoTouch = g.AddComponent<FadeNoTouch>();

                fadeNoTouch.FadeObjectIn(fadeTime);
            }

            PlayableInstance.AddPlayable(this);
            isTriggered = true;
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif

            if (fadeObjects == null || fadeObjects.Count == 0) return;

            if (playable.isPlayableCompleted(info))
            {
                foreach (GameObject g in fadeObjects)
                {
                    //g.SetActive(false);

                    FadeNoTouch fadeNoTouch = g.GetComponent<FadeNoTouch>();

                    if (fadeNoTouch != null)
                    {
                        Object.DestroyImmediate(fadeNoTouch);
                    }

                    fadeNoTouch = g.AddComponent<FadeNoTouch>();

                    fadeNoTouch.FadeObjectOut(fadeTime);
                }
                isTriggered = false;
            }

            PlayableInstance.RemovePlayable(this);
        }

        public override void OnGraphStop(Playable playable)
        {
            //if (fadeObjects == null) return;
            //fadeObjects.Clear();
        }

        public void OnSkip()
        {
            if (fadeObjects == null || fadeObjects.Count == 0) return;

            foreach (GameObject g in fadeObjects)
                g.SetActive(false);

            isTriggered = false;
        }
    }
}
