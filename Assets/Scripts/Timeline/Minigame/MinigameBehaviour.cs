using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using CoreSystem;
using System.Collections.Generic;
using CustomExtensions;

namespace CustomTracks
{
    [Serializable]
    public class MinigameBehaviour : PlayableBehaviour, ITimelineBehaviour
    {
        private bool _autoSizeAll = false;
        private List<string> _minigameToAutoSize = new List<string>()
    {
        "NC2", // New Concept 2
        "T4.1", // Dynamic Grouping
        "T1/PairAndMatchCupcakes", // Pair And Match
        "RT2/RT2Cupcakes", // Repeat Say
        "NC3", // New Concept 3
		"X3.4", // DYN
        "TB", // Text Builder
        "T4", // Arrange Conversation
		"X3.2", // Conrner Bubble - Did You Notice
		"ABCD", // Alphabet Book
		"T2.1",  // Fill In The Blanks - Drag
        "T6/T6_WorldMap", // Say
        "T1/PairAndMatchMap", // Pair And Match
        "T2.3", // Fill In The Blanks
        "T1/PairAndMatchGrid", // Pair And Match
        "NI2" // Name The Illustration Say
    };

        const float XMINDEFAULT = 0.1f;
        const float XMAXDEFAULT = 0.98f;
        const float YMINDEFAULT = 0.0f;
        const float YMAXDEFAULT = 0.9213021f;
        private Color DEFAULTCOLOR1 = new Color(0.0f, 0.0f, 0.0f, 1.0f);
        private Color DEFAULTCOLOR2 = new Color(0.058f, 0.62f, 0.058f, 1.0f);
        private Color DEFAULTCOLOR3 = new Color(0.871f, 0.0273f, 0.0273f, 1.0f);
        private Color DEFAULTCOLOR4 = new Color(1.0f, 1.0f, 1.0f, 1.0f);

        public bool _pauseOnFire = true;
        public GameObject[] _params;

        public bool UseDefaultBackground = false;
        public bool BackgroundPersistNext = false;
        public bool BackgroundPersistPrevious = false;
        public Color BackgroundColor = Color.white;
        public bool UseDefaultColors = true;
        public Color _color1;
        public Color _color2;
        public Color _color3;
        public Color _UIcolor1;
        public bool _forcedAnchorPositions = false;
        public bool _useDefaultForceDimensions = true;
        public float _xMin = Mathf.Clamp(XMINDEFAULT, 0.0f, 1.0f);
        public float _xMax = Mathf.Clamp(XMAXDEFAULT, 0.0f, 1.0f);
        public float _yMin = Mathf.Clamp(YMINDEFAULT, 0.0f, 1.0f);
        public float _yMax = Mathf.Clamp(YMAXDEFAULT, 0.0f, 1.0f);


        public bool _showGuideLines = false;

        public RectTransform minigameZone = null;
        [HideInInspector] public bool initialized = false;
        bool isTriggered = false;

        public double startTime { get ; set ; }
        public double endTime { get; set; }

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif

            if (!initialized || isTriggered) return;

            FireEvent();
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif

            if (!initialized) return;

            if (playable.isPlayableCompleted(info))
                isTriggered = false;
        }

        void FireEvent()
        {
            isTriggered = true;

            // Find minigame type and widget type
            string minigameType = MiniGameManager.Instance.GetMiniGameType();

            if ("X3.4" == minigameType)
            {
                return;
            }

            string minigameWidgetType = MiniGameManager.Instance.GetMiniGameWidgetType();

            // Build a type indentity string to use in check for auto expand
            string currentMinigame = minigameType;
            if (string.IsNullOrEmpty(minigameWidgetType) == false)
            {
                currentMinigame = string.Format("{0}/{1}", currentMinigame, minigameWidgetType);
            }

            MiniGameManager.Instance.hasMinigameZoneDefined = (minigameZone != null);

            if ((minigameZone == null) && (_autoSizeAll || _minigameToAutoSize.Contains(currentMinigame) || _minigameToAutoSize.Contains(minigameType)))
            {
                LayerSystem.Instance.SetDimensionsOnMinigameLayer(_xMin, _xMax, _yMin, _yMax, minigameZone);
                MiniGameManager.Instance.CreateAnimatedBackground(BackgroundPersistNext, BackgroundPersistPrevious, BackgroundColor);
                MiniGameManager.Instance.UsingAnimatedBackground = true;
            }
            else
            {
#if CLIENT_BUILD
            if (UseDefaultBackground == true)
            {
                LayerSystem.Instance.SetDimensionsOnMinigameLayer(_xMin, _xMax, _yMin, _yMax, minigameZone);
                MiniGameManager.Instance.CreateAnimatedBackground(BackgroundPersistNext, BackgroundPersistPrevious, BackgroundColor);
                MiniGameManager.Instance.UsingAnimatedBackground = true;
            }
            else
            {
                MiniGameManager.Instance.UsingAnimatedBackground = false;
            }
#endif
                if (_pauseOnFire)
                {
                    TimelineController.instance.PauseTimeline();
                }

                if (UseDefaultBackground != true)
                {
                    if ((_forcedAnchorPositions == true) || (minigameZone != null))
                    {
                        LayerSystem.Instance.SetDimensionsOnMinigameLayer(_xMin, _xMax, _yMin, _yMax, minigameZone);
                    }
                    else
                    {
                        LayerSystem.Instance.SetDimensionsOnMinigameLayer();
                    }
                }
            }

            MiniGameManager.Instance.Color1 = _color1;
            MiniGameManager.Instance.Color2 = _color2;
            MiniGameManager.Instance.Color3 = _color3;
            MiniGameManager.Instance.UIColor1 = _UIcolor1;
            MiniGameManager.Instance.TriggerMiniGame(_params, _pauseOnFire);
        }

        public void OnSkip()
        {
            isTriggered = false;
        }
    }
}