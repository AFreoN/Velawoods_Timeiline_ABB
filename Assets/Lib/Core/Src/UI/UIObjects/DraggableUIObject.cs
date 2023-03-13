using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UI;
using CoreSystem;

[RequireComponent (typeof (BoxCollider2D))]
public class DraggableUIObject : DraggableObjectBase
{
	public const float RETURN_DESTINATION_TOLERANCE = 1f;

	override public void Init()
	{
		base.Init();
		
		_distFromDestinationTolerance = RETURN_DESTINATION_TOLERANCE;

        ResizeCollider(Vector2.zero); //we want to be the size of the ui object
	}

	public void ToggleCollider (bool b){
		_collider.enabled = b;
	}

	protected override void MoveObjectWithTouch()
	{
		if (_snapToTouch)
		{
			transform.position = TouchPos + _cursorOffset;
		}
		else
		{
			transform.position = Vector3.Lerp(transform.position, TouchPos + _cursorOffset, 
			                                  _moveSpeed * Time.deltaTime);
		}
	}
}
