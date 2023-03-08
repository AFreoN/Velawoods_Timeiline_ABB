using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CoreLib;

public class CreateMiniGame : TimelineBehaviour
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
	private Color DEFAULTCOLOR1 = new Color(0.0f,0.0f,0.0f,1.0f);
	private Color DEFAULTCOLOR2 = new Color(0.058f, 0.62f, 0.058f, 1.0f);
	private Color DEFAULTCOLOR3 = new Color(0.871f, 0.0273f, 0.0273f, 1.0f);
	private Color DEFAULTCOLOR4 = new Color (1.0f, 1.0f, 1.0f, 1.0f);
   
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

    public override void OnClipStart(object o)
    {
		FireEvent();
    }

    public void FireEvent()
    {
        // Find minigame type and widget type
        string minigameType = MiniGameManager.Instance.GetMiniGameType();

        if("X3.4" == minigameType)
        {
            return;
        }

        string minigameWidgetType = MiniGameManager.Instance.GetMiniGameWidgetType();

        // Build a type indentity string to use in check for auto expand
        string currentMinigame = minigameType;
        if(string.IsNullOrEmpty(minigameWidgetType) == false)
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

    public override void OnSkip()
    {
		Skip();
    }

    public void Skip ()
	{

	}

	public void Reset ()
	{
		
	}


	 //resets forced dimensions to their default values
	[ExecuteInEditMode]
	void Update(){

			if (!(_forcedAnchorPositions)) {
				_xMin = XMINDEFAULT;
				_xMax = XMAXDEFAULT;
				_yMin = YMINDEFAULT;
				_yMax = YMAXDEFAULT;
			}

		if ((_xMin > _xMax) || (_yMin > _yMax)) {
			Debug.LogError("Minimum anchor must be larger than maximum anchor, otherwise no content will show");
		}

		if (_xMin < 0) {
			_xMin = 0;
		}
		if (_xMin > 1) {
			_xMin = 1;
		}
		if (_xMax < 0) {
			_xMax = 0;
		}
		if (_xMax > 1) {
			_xMax = 1;
		}
		if (_yMin < 0) {
			_yMin = 0;
		}
		if (_yMin > 1) {
			_yMin = 1;
		}
		if (_yMax < 0) {
			_yMax = 0;
		}
		if (_yMax > 1) {
			_yMax = 1;
		}

		if (_useDefaultForceDimensions) {
			_xMin = XMINDEFAULT;
			_xMax = XMAXDEFAULT;
			_yMin = YMINDEFAULT;
			_yMax = YMAXDEFAULT;

		}

		if (UseDefaultColors) {
			_color1 = DEFAULTCOLOR1;
			_color2 = DEFAULTCOLOR2;
			_color3 = DEFAULTCOLOR3;
			_UIcolor1 = DEFAULTCOLOR4;
			
		}


		if (_showGuideLines) {

			GameObject _canvas = GameObject.Find ("Frame_Canvas 1");
			if (_canvas != null) {
				RectTransform _rect = _canvas.GetComponent<RectTransform> ();
				float _width = _rect.rect.width * _rect.transform.localScale.x;
				float _height = _rect.rect.height * _rect.transform.localScale.y;
				float _tempYMax;
				float _tempYmin;
				float _tempXMax;
				float _tempXMin;
				Color _tempColor = Color.green;
				if ((_xMin > _xMax) || (_yMin > _yMax)) {
					_tempColor = Color.red;
				}
				if (_forcedAnchorPositions) {
					_tempYMax = _yMax;
					_tempYmin = _yMin;
					_tempXMax = _xMax;
					_tempXMin = _xMin;
				} else {
					_tempYMax = 1.0f;
					_tempYmin = 0.0f;
					_tempXMax = 1.0f;
					_tempXMin = 0.0f;
				}
				Debug.DrawLine (new Vector3 (_width * _tempXMin, _height * _tempYMax, 0.0f), new Vector3 (_width * _tempXMin, _height * _tempYmin, 0), _tempColor);
				Debug.DrawLine (new Vector3 (_width * _tempXMin, _height * _tempYmin, 0.0f), new Vector3 (_width * _tempXMax, _height * _tempYmin, 0), _tempColor);
				Debug.DrawLine (new Vector3 (_width * _tempXMax, _height * _tempYmin, 0.0f), new Vector3 (_width * _tempXMax, _height * _tempYMax, 0), _tempColor);
				Debug.DrawLine (new Vector3 (_width * _tempXMax, _height * _tempYMax, 0.0f), new Vector3 (_width * _tempXMin, _height * _tempYMax, 0), _tempColor);
			}
		}
			

	}


}
