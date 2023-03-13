using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;

namespace CoreSystem
{
	public class TextBubble : MonoBehaviour, IComponent
	{
		public enum AttachedObjectType
		{
			Left,
			Right,
			None
		}

		public GameObject _left_objects; //the objects on the left of the text object
        public GameObject _left_triangle; //the objects on the left of the text object
        public GameObject _right_objects; //the objects on the right of the text object
        public GameObject _right_triangle; //the objects on the right of the text object

        public TextBubbleObject _currentBubbleObject; //the current set of objects in use (left or right)

		public TextMeshProUGUI _text; //the text inside the bubble
		public RectTransform _bubbleImage; //the bubble bg itself

        public float _minHeight = 125.0f;
		public float _heightPadding = 45.0f; //how much we resize the bubble depending on the number of lines
		private float _oldHeight; //value storing how many lines we had last time

		private float _currentBubbleHeight; 
		private float _currentBubbleWidth;

		public bool _resizeHeight = true;
		public bool _resizeWidth = false;

		protected bool _hasInited = false;

		AudioClip _Audio;
		public AudioClip Audio {
			get {	return _Audio;	}
			set {	_Audio = value;	}
		}

		public void Init(string text, string layer, float start_width=800, float start_height=125 )
		{
			_hasInited = true;

			if (layer != "")
			{
				LayerSystem.Instance.AttachToLayer (layer, gameObject);
			}

            _oldHeight = _text.bounds.size.y;

			_currentBubbleHeight = 0;
			_currentBubbleWidth = 0;


			ChangeSize (start_width, start_height);

			UpdateText (text);
		}

        public void Init (string text, bool hasSeveralAnswers = false)
        {
            _hasInited = true;
            UpdateText(text);
        }

		public virtual void Start()
		{
			if(!_hasInited)
				Init(_text.text, "Bubbles");

		}

		public GameObject GetSideGameObject (AttachedObjectType objectType){

			if (objectType == AttachedObjectType.Left)
				return _left_objects;
			else if(objectType == AttachedObjectType.Right)
				return _right_objects;
			
			return null;
		}

		public void TurnOnObjects(AttachedObjectType objectType, bool deactivateOthers = true)
		{
			switch(objectType)
			{
				case AttachedObjectType.Left:
					_left_objects.SetActive(true);

                    if (null != _left_triangle)
                    {
                        _left_triangle.SetActive(true);
                    }

                    if (deactivateOthers)
                    {
                        _right_objects.SetActive(false);

                        if (null != _right_triangle)
                        {
                            _right_triangle.SetActive(false);
                        }
                    }
					
					_currentBubbleObject = _left_objects.GetComponent<TextBubbleObject>();
					break;
				case AttachedObjectType.Right:
					_right_objects.SetActive(true);

                    if (null != _right_triangle)
                    {
                        _right_triangle.SetActive(true);
                    }

                    if (deactivateOthers)
                    {
                        _left_objects.SetActive(false);

                        if (null != _left_triangle)
                        {
                            _left_triangle.SetActive(false);
                        }
                    }
					_currentBubbleObject = _right_objects.GetComponent<TextBubbleObject>();
					break;
				case AttachedObjectType.None:
					_right_objects.SetActive(false);
                    _left_objects.SetActive(false);

                    if (null != _left_triangle)
                    {
                        _left_triangle.SetActive(false);
                    }

                    if (null != _right_triangle)
                    {
                        _right_triangle.SetActive(false);
                    }
                    break;
			}
		}

		public void ChangeBubbleImage(Sprite new_image)
		{
			if(new_image != null)
			{
				_bubbleImage.GetComponent<Image>().sprite = new_image;
			}
		}

		public void Show(object paramters)
		{
			gameObject.SetActive (true);
		}

		public void Hide(object paramters)
		{
			gameObject.SetActive (false);
		}

		/// <summary>
		/// Changes the size of the bubble image.
		/// 
		/// Leave as -1 to not change the size of that variable
		///  e.g. width = 160 height = -1
		/// only the width will be changed
		/// </summary>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		public void ChangeSize(float width=-1, float height=-1)
		{
			if(height > -1)
				_currentBubbleHeight = height;

			if(width > -1)
				_currentBubbleWidth = width;

			_bubbleImage.SetSizeWithCurrentAnchors (RectTransform.Axis.Vertical, _currentBubbleHeight);
			_bubbleImage.SetSizeWithCurrentAnchors (RectTransform.Axis.Horizontal, _currentBubbleWidth);
		}

		protected void AutoHeight(float lineHeight)
		{
            _currentBubbleHeight = lineHeight;
            _bubbleImage.SetSizeWithCurrentAnchors (RectTransform.Axis.Vertical, _currentBubbleHeight);
		}

		/// <summary>
		/// Auto scales the bubble width.
		/// NOT YET IMPLEMENTED, does nothing
		/// TODO The rule for how/when a box changes width
		/// </summary>
		/// <param name="lineNum">Line number.</param>
		protected void AutoWidth(int lineNum)
		{
			_bubbleImage.SetSizeWithCurrentAnchors (RectTransform.Axis.Horizontal, _currentBubbleWidth);
		}

		public void ChangeBubbleColor(Color new_color, bool changeRing=false)
		{
			if(_currentBubbleObject == null)
			{
				return;
			}
			_bubbleImage.GetComponent<Image> ().color = new_color;
			_currentBubbleObject.ChangeColor (new_color, changeRing);
		}

		public void ChangeBubbleTextColor(Color new_color)
		{
			if(_text == null)
			{
				return;
			}
			_text.color = new_color;
		}

		public TextBubbleObject CurrentBubbleObject
		{
			get{return _currentBubbleObject;}
		}

		public AttachedObjectType CurrentAttachedObjectType
		{
			get{
				if ( _left_objects.activeSelf ) {
					return AttachedObjectType.Left;
				} else if ( _right_objects.activeSelf ) {
					return AttachedObjectType.Right;
				}
					
				return AttachedObjectType.None;

			}
		}

		public string Text
		{
			set{UpdateText(value);}
			get{return _text.text;}
		}

        public Bounds GetTextBounds()
        {
            return _text.bounds;
        }

        public Vector2 GetCharacterPosition(int characterIndex)
        {
			TMP_Text t;
			return _text.textInfo.characterInfo[characterIndex].bottomLeft;
            //return _text.GetCharacterPosition(characterIndex);
        }

		private void UpdateText(string value)
		{
			_text.text = value;
			_text.ForceMeshUpdate ();

            float newHeight = Mathf.Max(_minHeight, (_text.bounds.size.y + _heightPadding));

            if (_oldHeight != newHeight)
			{
                _oldHeight = newHeight;

                if (_resizeHeight)
					AutoHeight(newHeight);
			}
		}

        public void UpdateHeight()
        {
            float newHeight = Mathf.Max(_minHeight, (_text.bounds.size.y + _heightPadding));

            if (_oldHeight != newHeight)
            {
                _oldHeight = newHeight;

                if (_resizeHeight)
                    AutoHeight(newHeight);
            }
        }
	}
}
