using UnityEngine;
using System.Collections;
using TMPro;
using CoreSystem;

[RequireComponent(typeof(RectTransform))]
public class TextResizer : BaseListener {

    public float _maxWidth = 600;
    public float _maxHeight = 300;
    public float _minWidth = 600;
    public float _minHeight = 300;

    public int _maxLines = 1;
    public float _growthSpeed = 25;

    public float _yMargin = 0;
    public float _xMargin = 0;
    public bool _addedMarginX, _addedMarginY;


    protected RectTransform _rect_trans;
    public TextMeshProUGUI _textObject;
    protected TMP_TextInfo _textInfo;

    public bool _automatic = true;

    public bool _lastResizeX, _lastResizeY;

    public bool _hasInit = false;

	// Use this for initialization
	void Start () {
        if(!_hasInit)
         Init();
	}

    public virtual void Init()
    {
        _hasInit = true;
        _rect_trans = GetComponent<RectTransform>();

        if (_textObject == null)
            _textObject = transform.GetChild(0).GetComponent<TextMeshProUGUI>();


        if (_textObject == null)
        {
            _automatic = false;
            enabled = false;
        }

        _addedMarginX = false;
        _addedMarginY = false;

        _lastResizeX = false;
        _lastResizeY = false;
    }
	
	// Update is called once per frame
	void Update () {
        if (_automatic)
        {
            while (!_lastResizeX || !_lastResizeY)
            {
                AutoResize();
            }
        }
	}

    public virtual void ResizeWidth(float min_width, float max_width, float growth_speed)
    {
        if (_rect_trans.rect.width < min_width)
        {
            _addedMarginX = false;
            _rect_trans.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, min_width);
            _textObject.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _rect_trans.rect.width);
        }
        else if (_rect_trans.rect.width < max_width)
        {
            _addedMarginX = false;
            _rect_trans.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _rect_trans.rect.width + growth_speed);
            _textObject.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _rect_trans.rect.width);
        }
        else if (_rect_trans.rect.width > max_width)
        {
            _lastResizeX = true;
            _addedMarginX = false;
            _rect_trans.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, max_width);
            _textObject.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _rect_trans.rect.width);
        }
        else
        {
            _lastResizeX = true;
        }
    }

    public virtual void ResizeHeight(float min_height, float max_height, float growth_speed)
    {
        if (_rect_trans.rect.height < min_height)
        {
            _addedMarginY = false;
            _rect_trans.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, min_height);
            _textObject.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _rect_trans.rect.height);
        }
        else if (_rect_trans.rect.height < max_height)
        {
            _addedMarginY = false;
            _rect_trans.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _rect_trans.rect.height + growth_speed);
            _textObject.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _rect_trans.rect.height);
        }
        else if (_rect_trans.rect.height > max_height)
        {
            _lastResizeY = true;
            _addedMarginX = false;
            _rect_trans.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, max_height);
            _textObject.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _rect_trans.rect.height);
        }
        else
        {
            _lastResizeY = true;
        }
    }

    public virtual void AddMargin(float x_margin, float y_margin)
    {
        if (!_addedMarginX)
        {
            _addedMarginX = true;
            _rect_trans.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _rect_trans.rect.width + x_margin);
        }

        if (!_addedMarginY)
        {
            _addedMarginY = true;
            _rect_trans.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _rect_trans.rect.height + y_margin);
        }
    }

    public virtual void AutoResize(float max_width, float max_height, float min_width, float min_height, int max_lines, float growth_speed, float x_margin, float y_margin)
    {
        _textObject.ForceMeshUpdate();
        _textInfo = _textObject.textInfo;

        if (_textInfo.lineCount > max_lines)
        {
            ResizeWidth(min_width, max_width, growth_speed);
            ResizeHeight(min_height, max_height, growth_speed);
        }
        else
        {
            ResizeWidth(min_width, max_width, growth_speed);
            ResizeHeight(min_height, max_height, growth_speed);

            _lastResizeX = true;
            _lastResizeY = true;
        }

        AddMargin(x_margin, y_margin);
    }

    public virtual void AutoResize()
    {
        AutoResize(_maxWidth, _minHeight, _minWidth, _minHeight, _maxLines, _growthSpeed, _xMargin, _yMargin);
    }
}
