using UnityEngine;
using CoreSystem;
using System;
using System.Collections.Generic;

public class DragAndDropAcceptor : TouchZone
{
	public delegate void DraggableCallback();
	public event DraggableCallback AcceptedNewDraggable;

	protected List<GameObject> _objectsAttached;

	public override void Init ()
	{
		base.Init ();

		_objectsAttached = new List<GameObject> ();

		ResizeCollider(Vector2.zero); //we want to be the size of the ui object

		AddStringListener (gameObject);
	}
	
	//Override this and set your own conditions for accepting a draggable object
	protected virtual bool AcceptObject(GameObject dragged)
	{
		return false;
	}
	
	//Once you've agreed to accept a draggable. Override this function to attach it to yourself and position it.
	protected virtual void AttachAndPositionDraggable(GameObject draggable){}

	//This function will be called when a draggable is taken away from you and attached to another parent.
	protected virtual void DraggableContentsReparented(GameObject draggable)
	{
		_objectsAttached.Remove (draggable);
	}

	protected virtual void TouchZoneDown(object acceptor){}

	protected virtual void TouchZoneHit(object pairImage){}
	
	protected virtual void TouchZoneMiss(object pairImage){}
	
	protected virtual void TouchZoneUp(object accepter)
	{
		GameObject heldObject = TouchManager.Instance.GetHeldObject ();
		//If an object is being held
		if(heldObject != null)
		{
			DraggableObjectBase heldDraggableScript = heldObject.GetComponent<DraggableObjectBase>();

			//Does any class sub class accept this object?
			if(AcceptObject(heldObject))
			{
				//Tell the old parent of this object that it has a new parent.
				heldDraggableScript.AcceptReparent(heldObject);

				//Add to the data structure of this acceptor
				AddToContents(heldObject);

				//Accept new coordinates
				AttachAndPositionDraggable(heldObject);

				if (AcceptedNewDraggable != null)
				{
					//Let any listeners know we have accepted new contents
					AcceptedNewDraggable();
				}
			}
			else
			{
				//Go back to position originally dragged from

				//Tell the draggable it has been unsuccesful in attaching to this acceptor.
				//If it was dragged on top of 2 or more acceptors there is still a chance it will be successful.
				heldDraggableScript.RejectReparent(heldObject);
			}
		}
	}

	//Add to the data structure for this object. Ca
	protected void AddToContents(GameObject content)
	{
		//Set our new content
		_objectsAttached.Add(content);

		//Listen for if this object gets reparented again.
		DraggableObjectBase heldDraggableScript = content.GetComponent<DraggableObjectBase>();
		heldDraggableScript.ObjectReparented = DraggableContentsReparented;
	}

	public List<GameObject> GetContents()
	{
		return _objectsAttached;
	}
}
