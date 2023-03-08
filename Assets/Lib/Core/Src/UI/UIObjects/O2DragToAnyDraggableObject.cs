using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UI;
using CoreLib;

public class O2DragToAnyDraggableObject : TouchZone
{
    public Action<GameObject> ObjectReparented = delegate { };

    private bool shouldDetectTouch = true; // _detectTouch
    public bool ShouldDetectTouch
    {
        get
        {
            return shouldDetectTouch;
        }
        set
        {
            shouldDetectTouch = value;
        }
    }

    private float timeToActivate = 0.0f; // _holdtimeActive
    public float TimeToActivate
    {
        get
        {
            return timeToActivate;
        }
        set
        {
            timeToActivate = value;
        }
    }

    private bool snapToUserTouch = false; // _snapToTouch 
    public bool SnapToUserTouch
    {
        get
        {
            return snapToUserTouch;
        }
        set
        {
            snapToUserTouch = value;
        }
    }

    private Vector3 cursorOffset = Vector3.zero; // _cursorOffset
    public Vector3 CursorOffset
    {
        get
        {
            return cursorOffset;
        }
        set
        {
            cursorOffset = value;
        }
    }

    private float moveSpeed = 5.0f; // _moveSpeed
    public float MoveSpeed
    {
        get
        {
            return moveSpeed;
        }
        set
        {
            moveSpeed = value;
        }
    }
    private bool shouldReturnToPosition = true; // _returnOnNoTouch
    public bool ShouldReturnToPosition
    {
        get
        {
            return shouldReturnToPosition;
        }
        set
        {
            shouldReturnToPosition = value;
        }
    }

    private Vector3 ReturnPosition = Vector3.zero;
    private Vector3? NewPosition = null;

    private bool IsMoving = false; // _moving
    private bool HasBeenTouched = false; // _hasTouched
    private bool IsFollowingUserTouch = false; // _followTouch
    private bool IsMovingWithoutUser = false; // _movingWithoutTouch
    private float TimeHeld = 0.0f; // _holdTime
    private DraggableObjectBase.DragState CurrentState = DraggableObjectBase.DragState.STATIC; // _state

    public override void Init()
    {
        base.Init();
        
        _InitOnStart = false;

        ReturnPosition = Vector3.zero;
        NewPosition = null;
        IsMoving = false;
        HasBeenTouched = false;
        IsFollowingUserTouch = false;
        TimeHeld = 0.0f;
        CurrentState = DraggableObjectBase.DragState.STATIC;
    }

    protected override void Update()
    {
        if (false == TouchZone._isEnabled || Route1Games.PauseManager.Instance.IsMenuPaused)
        {
            return;
        }

        if (false == IsMoving && true == ShouldDetectTouch)
        {
            DetectTouch();
        }
        else
        {
            if (true == IsFollowingUserTouch)
            {
                MoveWithTouch();
            }
            else if (true == IsMovingWithoutUser)
            {
                MoveWithoutTouch();
            }
        }

        HandleDropReaction();
    }

    public void SetReturnPositionToCurrent()
    {
        ReturnPosition = transform.position;
    }

    public void SetnewPositionToCurrent()
    {
        NewPosition = transform.position;
    }

    public void SetnewPosition(Vector3 newPosition)
    {
        NewPosition = newPosition;
    }

    public void ResetNewPosition()
    {
        NewPosition = null;
    }

    private void DetectTouch()
    {
        if (false == HasBeenTouched)
        {
            HasBeenTouched = HasTouchDownOnObject();
        }
        else
        {
            HasBeenTouched = IsTouchInZone();
        }

        if (true == HasBeenTouched)
        {
            if (TimeHeld >= TimeToActivate)
            {
                IsMoving = true;
                IsFollowingUserTouch = true;

                TouchManager.Instance.ObjectPickedUp(gameObject);

                CurrentState = DraggableObjectBase.DragState.HELD;

                SendStringEvent(DraggableObjectBase.MOVING_EVENT, gameObject);
            }
            else
            {
                TimeHeld += (1.0f * Time.deltaTime);
            }
        }
        else
        {
            TimeHeld = 0.0f;
        }
    }

    private void MoveWithTouch()
    {
        if (false == GetTouchPos())
        {
            TimeHeld = 0.0f;
            IsFollowingUserTouch = false;
            ReturnToSetPosition();

            SendStringEvent(DraggableObjectBase.RELEASED_EVENT, gameObject);
            TouchManager.Instance.ObjectDropped(gameObject);
        }
        else
        {
            MoveObjectWithTouch();
        }
    }

    public void ReturnToSetPosition()
    {
        IsMoving = true;
        IsMovingWithoutUser = true;
        IsFollowingUserTouch = false;
    }

    protected void MoveObjectWithTouch()
    {
        if (true == SnapToUserTouch)
        {
            transform.position = WorldTouchPos + CursorOffset;
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, WorldTouchPos + CursorOffset, MoveSpeed * Time.deltaTime);
        }
    }

    private Vector3 WorldTouchPos
    {
        get 
        { 
            return Camera.main.ScreenToWorldPoint(new Vector3(_touchPos.x, _touchPos.y, (ReturnPosition - Camera.main.transform.position).magnitude)); 
        }
    }

    private void MoveWithoutTouch()
    {
        if (ShouldReturnToPosition)
        {
            Vector3 newPosition = true == NewPosition.HasValue ? NewPosition.Value : ReturnPosition;

            if (Vector3.Distance(transform.position, newPosition) < Draggable3dObject.RETURN_DESTINATION_TOLERANCE)
            {
                IsMoving = false;
                IsMovingWithoutUser = false;

                CurrentState = DraggableObjectBase.DragState.STATIC;
                SendStringEvent(DraggableObjectBase.RETURNED_EVENT, gameObject);
            }
            else
            {
                transform.position = Vector3.Lerp(transform.position, newPosition, MoveSpeed * Time.deltaTime);
            }
        }
        else
        {
            IsMoving = false;
            IsMovingWithoutUser = false;

            CurrentState = DraggableObjectBase.DragState.STATIC;
            SendStringEvent(DraggableObjectBase.RETURNED_EVENT, gameObject);
        }
    }

    private void HandleDropReaction()
    {
        switch (CurrentState)
        {
            case DraggableObjectBase.DragState.STATIC:
            case DraggableObjectBase.DragState.HELD:
                break;
            case DraggableObjectBase.DragState.ACCEPTED:
                {
                    SendStringEvent(DraggableObjectBase.REPARENT_EVENT, gameObject);
                    CurrentState = DraggableObjectBase.DragState.STATIC;
                    break;
                }
            case DraggableObjectBase.DragState.REJECTED:
                {
                    SendStringEvent(DraggableObjectBase.REPARENT_FAIL_EVENT, gameObject);
                    CurrentState = DraggableObjectBase.DragState.STATIC;
                }
                break;
        }
    }

    public void AcceptReparent(GameObject gameObj)
    {
        ObjectReparented(gameObj);
        CurrentState = DraggableObjectBase.DragState.ACCEPTED;
    }

    public void RejectReparent(GameObject gameObj)
    {
        if (CurrentState != DraggableObjectBase.DragState.ACCEPTED)
        {
            CurrentState = DraggableObjectBase.DragState.REJECTED;
        }
    }
}
