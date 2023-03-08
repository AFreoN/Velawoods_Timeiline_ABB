using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace CoreLib
{
	public class VerticalLayout : UIListBase, IUIList {

		public Scrollbar scrollbar;

		public float _cellHeight = 300.0f; //the size of a cell

		public float _gap_y = 0.0f; //how much between each cell

		private bool _offScreen = false; //has this layout gone off screen

		private bool _stretchObjects = false;

		public virtual void Init(float gap_y = 0.0f, float object_height=300.0f, bool stretchObjects=false)
		{
			base.Init ();
			_cellHeight = object_height;
			_gap_y = gap_y;
			_items = transform.GetChild(0).GetComponent<RectTransform>();
			_touchArea = _items.GetChild(0);

			_stretchObjects = stretchObjects;
		}

		public override void Reset ()
		{
			base.Reset ();

			_offScreen = false;
			_objectsAttached.Clear();
			_items.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0);
			_items.anchorMin = new Vector2(0, 0.5f);
			_items.anchorMax = new Vector2(1, 0.5f);
			_items.offsetMin = Vector2.zero;
			_items.offsetMax = Vector2.zero;
		}

		public override void ScrollbarEnabled (bool on)
		{
			// Allow turning scrolling off if not offscreen
			//if( _offScreen )
			base.ScrollbarEnabled (on);
		}

		public void RemoveFromList(GameObject obj)
		{
			if(_objectsAttached.Contains(obj)) _objectsAttached.Remove(obj);
		}

		public void AddToList(GameObject _object)
		{
			if(_objectsAttached == null) _objectsAttached = new List<GameObject>();

			_object.transform.SetParent(_touchArea, false); //add it to the touchable area

			//Vertical Layout variables -------------------------------------------
			RectTransform _rectTransform = _object.GetComponent<RectTransform>();

			if(_stretchObjects)
			{
				_rectTransform.anchorMin = new Vector2(0, 1.0f);
				_rectTransform.anchorMax = new Vector2(1, 1.0f);
			}
			//Vertical Layout variables -------------------------------------------

			//add a new cell
			_items.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _items.rect.height + _cellHeight);

			//has this layout go off screen
			if(_items.rect.height >= LayerSystem.Instance.RefResolution.y && !_offScreen)
			{
				_items.anchorMin = new Vector2(0, 0);
				_items.anchorMax = new Vector2(1, 0);
				_offScreen = true;
				if(_scrollRect)	_scrollRect.enabled = true;
			}
			
			_object.transform.localScale = Vector3.one;

			_objectsAttached.Add(_object);

			if(_autoUpdate) UpdateLayout();
		}

		public void UpdateLayout()
		{
			float spacingY = _items.rect.height / _objectsAttached.Count;
			float last_y = _cellHeight / 2;
			
			foreach(GameObject obj in _objectsAttached)
			{
				if(obj != null)
				{
					float new_y = (last_y - spacingY) + _gap_y;
					last_y = new_y;
					
					obj.GetComponent<RectTransform>().offsetMax = new Vector2(0, new_y);
					obj.GetComponent<RectTransform>().offsetMin = new Vector2(0, new_y);
				}
			}

			if (scrollbar) {

				ScrollRect scroll = GetComponent<ScrollRect> ();
				RectTransform contents = scroll.content.GetComponent<RectTransform> ();
				RectTransform container = GetComponent<RectTransform> ();

				// Hide the scrollbar when not needed
				if ( contents.rect.height > container.rect.height ) {
					scrollbar.gameObject.SetActive( true );
				} else {
					scrollbar.gameObject.SetActive( false );
				}

			}
		}
	}
}
