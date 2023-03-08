using UnityEngine;
using System.Collections.Generic;
using CoreLib;

public class HorizontalZigZagLayout : UIListBase, IUIList
{
	private Vector2 _cellDimension; //the size of a cell
	
	private Vector2 _gapDimension; //how much between each cell
	
	private bool _offScreen = false; //has this layout gone off screen
	
	public void Init(Vector2 cellDimension, Vector2 gapDimension)
	{
		base.Init ();
		_cellDimension = cellDimension;
		_gapDimension = gapDimension;
	}

	public void RemoveFromList(GameObject obj)
	{
		//Need implementing for drag and drop
		_objectsAttached.Remove (obj);
		UpdateLayout ();
	}

	public void AddToList(GameObject _object)
	{
		if(_objectsAttached == null)
		{
			_objectsAttached = new List<GameObject>();
		}
		AddToContents (_object);
		SetElementAnchorsAndParent (_object);
		UpdateLayout();
	} 

	public void SetElementAnchorsAndParent(GameObject _object)
	{
		_object.transform.SetParent(_touchArea); //add it to the touchable area

		//Vertical Layout variables -------------------------------------------
		RectTransform _rectTransform = _object.GetComponent<RectTransform>();
		_rectTransform.offsetMin = Vector2.zero;
		_rectTransform.offsetMax = _cellDimension;
		_rectTransform.anchorMin = new Vector2(0f, 0.5f);
		_rectTransform.anchorMax = new Vector2(0f, 0.5f);
		_rectTransform.anchoredPosition = Vector2.zero;
		//Vertical Layout variables -------------------------------------------

		_object.transform.localScale = Vector3.one;
	}

	public void RandomiseList()
	{
		CoreHelper.RandomizeList (GetContents ());
		UpdateLayout ();
	}
	
	public void UpdateLayout()
	{
		//Expand container if we need to.
		CheckContainerSize ();

		//has this layout gone off screen
		if(_items.rect.width >= LayerSystem.Instance.RefResolution.x && !_offScreen)
		{
			_offScreen = true;
			_scrollRect.enabled = true;
		}

		float totalElementSpacing = _cellDimension.x + _gapDimension.x;
		float last_x = _cellDimension.x / 2 - totalElementSpacing + _gapDimension.x;
		float spacingX = _items.rect.width / (_objectsAttached.Count + 1);
		float ySpacing = _gapDimension.y / 2;
		foreach(GameObject obj in _objectsAttached)
		{
			if(obj != null)
			{
				float new_x;
				if(IsTouchAreaBiggerThanScreen())
				{
					new_x = last_x + totalElementSpacing;
					last_x = new_x;
				}
				else
				{
					new_x = last_x +  spacingX;
					last_x = new_x;
				}
				ySpacing = -ySpacing;
				
				obj.GetComponent<RectTransform>().anchoredPosition = new Vector2(new_x, ySpacing);
			}
		}
	}

	private void CheckContainerSize()
	{
		int objectCount = _objectsAttached.Count;
		RectTransform touchAreaRect = _touchArea.GetComponent<RectTransform> ();

		//Touch area should always be at least the width of the parent rect transform
		if(touchAreaRect.rect.width < _items.rect.width)
		{
			touchAreaRect.sizeDelta = new Vector2(_items.rect.width, _items.sizeDelta.y);
		}
		float minWidthNeeded = (objectCount * (_cellDimension.x + _gapDimension.x)) + _gapDimension.x;
		//If greater than the width of the parent then the touch area should be as big as its containing objects
		if(touchAreaRect.rect.width < minWidthNeeded)
		{
			touchAreaRect.sizeDelta = new Vector2(minWidthNeeded, _items.sizeDelta.y);
		}
		//If we have shrunk then correct the width of the box
		else if(touchAreaRect.rect.width > minWidthNeeded && 
		        touchAreaRect.rect.width > _items.rect.width)
		{
			touchAreaRect.sizeDelta = new Vector2(minWidthNeeded, _items.sizeDelta.y);
		}
	}

	private bool IsTouchAreaBiggerThanScreen()
	{
		RectTransform touchAreaRect = _touchArea.GetComponent<RectTransform> ();
		return touchAreaRect.rect.width > _items.rect.width;
	}
}
