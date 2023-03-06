using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UI;
using CustomExtensions;

namespace CustomTracks
{
    [Serializable]
    public class FadeScreenBehaviour : PlayableBehaviour
    {
        public AnimationCurve fadeCurve;
        public Color fadeColor;

        [HideInInspector]
        public double startTime, endTime;
        [HideInInspector]
        public PlayableDirector director;

        static GameObject fadeObject = null;
        static Image fadeImage = null;

        bool IsInTime => director.time >= startTime && director.time <= endTime;

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
#if UNITY_EDITOR
            if (Application.isPlaying == false) return;
#endif
            if (startTime == 0 && endTime == 0) return;

            if (fadeObject == null)
            {
                // Create the new game object that will be used to fade the screen
                fadeObject = new GameObject();
                fadeObject.name = "FadeObject"; // For easy identification

                GameObject canvas = GameObject.Find("OverlayCanvas");
                fadeObject.transform.SetParent(canvas.transform);

                // Add image and make it fill the screen
                fadeImage = fadeObject.AddComponent<Image>();
                fadeImage.rectTransform.anchorMin = new Vector2(0.0f, 0.0f);
                fadeImage.rectTransform.anchorMax = new Vector2(1.0f, 1.0f);

                // Add canvas group to block raycasts
                fadeObject.AddComponent<CanvasGroup>();

                fadeObject.transform.SetAsLastSibling();

                RectTransform rt = fadeObject.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(0, 0);
                rt.anchoredPosition = new Vector2(0, 0);
                rt.localScale = Vector3.one;
            }
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
#if UNITY_EDITOR
            if (Application.isPlaying == false) return;
#endif

            if (!director || !IsInTime) return;

            //Interpolate time values to evaluate animation curve,use it's value to set image alpha value
            float i = (float)Extensions.InverseLerp(startTime, endTime, director.time);
            Fade(fadeCurve.Evaluate(i));
        }

        void Fade(float alpha)
        {
            if (!fadeObject)
            {
                Debug.LogError("No Fade Object!");
                return;
            }

            alpha = Mathf.Min(Mathf.Max(0.0f, alpha), 1.0f);
            fadeColor.a = alpha;
            fadeImage.color = fadeColor;

            fadeImage.enabled = (alpha > 0.01f);
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
#if UNITY_EDITOR
            if (Application.isPlaying == false) return;
#endif

            if (playable.isPlayableCompleted(info) && fadeObject)
                UnityEngine.Object.DestroyImmediate(fadeObject);    //Destroy create image and it's gameobject on end of this clip
        }
    }
}
