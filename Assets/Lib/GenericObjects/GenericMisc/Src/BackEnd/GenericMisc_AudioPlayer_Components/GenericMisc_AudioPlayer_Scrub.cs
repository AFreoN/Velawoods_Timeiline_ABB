using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

public class GenericMisc_AudioPlayer_Scrub : Scrollbar {

	private GenericMisc_AudioPlayer _parent;
	private GenericMisc_AudioPlayer Parent {
		get {
			if (_parent == null)
				_parent = transform.parent.GetComponent<GenericMisc_AudioPlayer> ();
			return _parent;
		}
	}
	
	private float _valueTemp = 0;
	private bool _dragFlag = false;
	
	public override void OnPointerUp (PointerEventData eventData)
	{
		base.OnPointerUp (eventData);
		if (_dragFlag == false)
			Parent.ScrubOnClick ();
	}
	
	public override void OnPointerDown (PointerEventData eventData)
	{
		base.OnPointerDown (eventData);
		_dragFlag = false;
	}
	
	public override void OnBeginDrag (PointerEventData eventData)
	{
		base.OnBeginDrag (eventData);
		_dragFlag = true;
		Parent.ScrubOnBeginDrag ();
	}
	
	public void Update ()
	{
		if (value != _valueTemp)
		{
			Parent.ScrubOnValueChanged ();
			_valueTemp = value;
		}
		
		UpdateScrubSize ();
	}
	
	private void UpdateScrubSize ()
	{
		float height = handleRect.rect.height;
		float timelineWidth = transform.parent.GetComponent<RectTransform> ().rect.width;
		
		size = height/timelineWidth;
	}
}
