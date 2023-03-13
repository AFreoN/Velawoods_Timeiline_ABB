using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UI;
using CoreSystem;

[RequireComponent (typeof (BoxCollider))]
public class Draggable3dObject : DraggableObjectBase
{	
	public const float RETURN_DESTINATION_TOLERANCE = 0.01f;

	private float _objectDistFromCamera;

	override public void Init()
	{
		base.Init();

		_distFromDestinationTolerance = RETURN_DESTINATION_TOLERANCE;

		AddStringListener (gameObject);
	}

	protected override void MoveObjectWithTouch()
	{
		if (_snapToTouch)
		{
			transform.position = WorldTouchPos + _cursorOffset;
		}
		else
		{
			transform.position = Vector3.Lerp(transform.position, WorldTouchPos + _cursorOffset, 
			                                  _moveSpeed * Time.deltaTime);
		}
	}

	protected void ObjectMoving(object acceptor)
	{
		_objectDistFromCamera = (transform.position - Camera.main.transform.position).magnitude;
	}

	//Convert the screen touch pos to a world coordinate with a constant distance from the camera.
	//This constant is original the pick up distance of the object from the camera.
	public Vector3 WorldTouchPos
	{
		get { return Camera.main.ScreenToWorldPoint (new Vector3 (_touchPos.x, _touchPos.y, _objectDistFromCamera)); }
	}
}
