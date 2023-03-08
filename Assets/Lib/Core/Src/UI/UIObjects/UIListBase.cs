using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIListBase : DragAndDropAcceptor
{
	protected RectTransform _items; //the container for all items
	
	protected ScrollRect _scrollRect; //the scroll component
	
	protected Transform _touchArea; //an invisble component to allow touching all over the layout

	public bool _autoUpdate = true;

	public override void Init()
	{
		base.Init ();
		_scrollRect = GetComponent<ScrollRect>();
		_items = GetComponent<RectTransform>();
		_touchArea = _items.GetChild(0);
	}

	public Transform TouchArea
	{
		get{return _touchArea;}
	}

	public virtual void ScrollbarEnabled(bool on)
	{
		if(_scrollRect) _scrollRect.enabled = on;
	}

	public virtual void Reset()
	{

	}
}
