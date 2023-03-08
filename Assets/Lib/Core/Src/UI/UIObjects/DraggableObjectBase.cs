using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UI;
using CoreLib;

public class DraggableObjectBase : TouchZone
{
	//Will be fired when draggable object is assigned to another parent.
	public Action<GameObject> ObjectReparented = delegate {};
	
	public float _holdtimeActive = 0f; //The Amount of time the user has to hold down to drag
	public float _moveSpeed = 5; //the speed of the drag
	public bool _snapToTouch = false; //is the object following the users touch or snapping to it
	public Vector3 _moveTo; //where you want the object to go when it has no touch to follow
	public bool _detectTouch = true; //is this object allowed to detect touches
	public bool _StartPositionOverride = true; //Sets The MoveTo Variable to the objects current position on Start
	public bool _onlyTriggerOnMouseDownInObject = true; //if set the drag will only be triggered by touch down on this object.
	public Vector3 _cursorOffset = Vector3.zero; //An offset from where the touch is
	public bool _returnOnNoTouch = true; //should this object move towards the _moveTo variable when let go
	public bool _playDefaultSounds = true; //should this object play a select and release sound
	
	private string _selectSoundPath = "Audio/UI_Select";
	private string _releaseSoundPath = "Audio/UI_Slide_2";
	
	private bool _followTouch; // Is the object following the users touch
	private bool _movingWithoutTouch; //is the object moving by itself
	protected float _holdTime; //how long has the user held down on this object
	private bool _moving; //is this object moving in anyway
	private bool _useLocalPositions; // Position the object using local positions rather than world positions - useful for items in scrolling lists
	private DragState _state; //Are we static, held, accepted or rejected. Used to react to a drop in the correct way
	private bool _hasTouched;
	private bool _enabled;

	protected float _distFromDestinationTolerance = 1; //How close to our _moveTo pos to we need to be before we say we have returned
	
	public static string RELEASED_EVENT = "ObjectReleased"; //object was released
	public static string RETURNED_EVENT = "ObjectReturned"; //object has return to the start position
	public static string MOVING_EVENT = "ObjectMoving"; //object has begun moving
	public static string REPARENT_EVENT = "ObjectReparented"; //object has been attached to another object
	public static string REPARENT_FAIL_EVENT = "ObjectReparentingFailed"; //object has tried to be attached to an object that does not want it

	public bool JustSelected = false;
	public bool JustReleased = false;

	public enum DragState
	{
		STATIC,
		HELD,
		ACCEPTED,
		REJECTED
	}

	public bool UseLocalPositions {
		get{ return _useLocalPositions;}
		set{ _useLocalPositions = value; }
	}

	override public void Init( )
	{
		base.Init();

		_holdTime = 0.0f; //the hold time currently
		_moving = false; //the object is not moving
		_followTouch = false; //we are not following the touch to begin with
		_movingWithoutTouch = false; 
		_state = DragState.STATIC;
		_hasTouched = false;
		_enabled = true;

		if (_StartPositionOverride)
		{
			SetResetPosToCurrent();	
		} 
	}
	
	public void SetResetPosToCurrent()
	{
		if (_useLocalPositions) {
			_moveTo = transform.localPosition;
		} else {
			_moveTo = transform.position;
		}
	}
	
	public void SetNewPosition(Vector3 newPos)
	{
		_moveTo = newPos;
		//Debug.Log ("SetNewPosition _moveTo: " + _moveTo, gameObject);
	}
	
	private void MoveWithTouch()
	{
		if (!GetTouchPos())
		{
			_holdTime = 0;
			_followTouch = false;
			ReturnToSetPosition();

			JustReleased = true;

			SendStringEvent(RELEASED_EVENT, gameObject);
			TouchManager.Instance.ObjectDropped(gameObject);
			
			if (_playDefaultSounds)
				PlayAudio (_releaseSoundPath);
		}
		else
		{
			MoveObjectWithTouch();
		}
	}

	//Override to set how object should move with touch
	protected virtual void MoveObjectWithTouch(){}

	private void DetectTouch()
	{
		if(_onlyTriggerOnMouseDownInObject)
		{
			if(!_hasTouched)
			{
				_hasTouched = HasTouchDownOnObject();
			}
			else
			{
				_hasTouched = IsTouchInZone();
			}
		}
		else
		{
			_hasTouched = IsTouchInZone();
		}

		if (_hasTouched)
		{
			if (_holdTime >= _holdtimeActive && _enabled)
			{
				_moving = true;
				_followTouch = true;
				
				TouchManager.Instance.ObjectPickedUp(gameObject);
				_state = DragState.HELD;
				SendStringEvent(MOVING_EVENT, gameObject);
				
				if (_playDefaultSounds)
					PlayAudio (_selectSoundPath);
			}
			else
			{
				_holdTime += 1.0f * Time.deltaTime;
			}
		}
		else
		{
			_holdTime = 0;
		}
	}
	
	/// <summary>
	/// Tell this object to go to whereever the _moveTo variable
	/// is.
	/// </summary>
	public void ReturnToSetPosition()
	{
		_moving = true;
		_movingWithoutTouch = true;
		_followTouch = false;
	}
	
	private void PlayAudio (string audioFilePath)
	{
		if (AudioManager.Instance != null)
			AudioManager.Instance.PlayAudio (audioFilePath);
	}
	
	private void MoveWithoutTouch()
	{
		if(_returnOnNoTouch)
		{
			if (!_useLocalPositions) 
			{
				if (Vector3.Distance (transform.position, _moveTo) < _distFromDestinationTolerance)
				{
					_moving = false;
					_movingWithoutTouch = false;

					_state = DragState.STATIC;
					SendStringEvent (RETURNED_EVENT, gameObject);
				} 
				else
				{
					transform.position = Vector3.Lerp (transform.position, _moveTo, _moveSpeed * Time.deltaTime);
				}
			} 
			else
			{
				if (Vector3.Distance (transform.localPosition, _moveTo) < _distFromDestinationTolerance) {
					_moving = false;
					_movingWithoutTouch = false;
					
					_state = DragState.STATIC;
					SendStringEvent (RETURNED_EVENT, gameObject);
				} else {
					transform.localPosition = Vector3.Lerp (transform.localPosition, _moveTo, _moveSpeed * Time.deltaTime);
				}
			}
		}
		else
		{
			_moving = false;
			_movingWithoutTouch = false;
			
			_state = DragState.STATIC;
			SendStringEvent (RETURNED_EVENT, gameObject);
		}
	} 
	
	// Update is called once per frame
	protected override void Update ()
	{
		if (!TouchZone._isEnabled || Route1Games.PauseManager.Instance.IsMenuPaused) return;
	
		if(_autoResizeCollider) ResizeCollider(Vector2.zero);
		
		if (!_moving && _detectTouch)
		{
			DetectTouch();
		}
		else
		{
			if (_followTouch)
			{
				MoveWithTouch();
			}
			else if (_movingWithoutTouch)
			{
				MoveWithoutTouch();
			}
		}
		HandleDropReaction ();
	}

	public bool IsMoving 
	{
		get { return _moving; }
	}

	private void HandleDropReaction()
	{
		switch(_state)
		{
			case DragState.STATIC:
			case DragState.HELD:
				break;
			case DragState.ACCEPTED:
				SendStringEvent (REPARENT_EVENT, gameObject);
				_state = DragState.STATIC;
				break;
			case DragState.REJECTED:
				SendStringEvent (REPARENT_FAIL_EVENT, gameObject);
				_state = DragState.STATIC;
				break;
		}
	}

	public void AcceptReparent(GameObject gameObj)
	{
		ObjectReparented (gameObj);
		_state = DragState.ACCEPTED;
	}

	//Draggable has been unsuccesful in attaching to an acceptor.
	//If it was dropped on 2 or more acceptors and one is succesful the success will overide the rejected state.
	public void RejectReparent(GameObject gameObj)
	{
		if(_state != DragState.ACCEPTED)
		{
			_state = DragState.REJECTED;
		}
	}

	public void EnableDrag(bool enable)
	{
		_enabled = enable;
	}
}
