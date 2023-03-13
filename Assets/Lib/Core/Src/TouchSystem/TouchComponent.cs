using UnityEngine;
using System.Collections.Generic;
using CoreSystem;
using System;

public class TouchComponent : MonoBehaviour
{
	public Action<GameObject> OnTouch = delegate {};

	private List<GameObject> touchObjects;

	void Awake()
	{
		touchObjects = new List<GameObject> ();
		InitialiseTouchObjects (gameObject);

        //Dont allow flashing objects to be touched if there is a menu on top of them
#if CLIENT_BUILD
        CoreEventSystem.Instance.AddListener (MainMenu.Messages.MENU_SHOWING, Disable);
		CoreEventSystem.Instance.AddListener (MainMenu.Messages.MENU_HIDING, Enable);
#endif
    }

	private void InitialiseTouchObjects(GameObject touchObject)
	{
		Collider existingCollider = touchObject.GetComponent<Collider> ();
		MeshRenderer renderer = touchObject.GetComponent<MeshRenderer>();

		if(existingCollider)
		{
			// Add touch script
			TouchZone touchScript = touchObject.AddComponent<TouchZone>();
			touchScript.AddStringListener(gameObject);
          //  Debug.LogError("I am here");
		}
		else
		{
			if(renderer)
			{
				// add a collider and touch
               // Debug.LogError("I am here 1");
				touchObject.AddComponent<BoxCollider>();

				TouchZone touchScript = touchObject.AddComponent<TouchZone>();
				touchScript.AddStringListener(gameObject);
				touchObject.GetComponent<BoxCollider> ().center = new Vector3 (0, 0, 0);
			}
		}

		touchObjects.Add(touchObject);

		//Recursively add children
		foreach(Transform child in touchObject.transform)
		{
			InitialiseTouchObjects(child.gameObject);
		}
	}

	protected void TouchZoneDown()
	{
		//Pass this event on to any listeners
		OnTouch (gameObject);
	}

	public void Reset()
	{
		foreach(GameObject touchObject in touchObjects)
		{
		if(touchObject.GetComponent<Draggable3dObject>() == null){
                Debug.Log("I am here 2");
			GameObject.Destroy(touchObject.GetComponent<BoxCollider>());
			}
			GameObject.Destroy(touchObject.GetComponent<TouchZone>());
#if CLIENT_BUILD
            CoreEventSystem.Instance.RemoveListener (MainMenu.Messages.MENU_SHOWING, Disable);
			CoreEventSystem.Instance.RemoveListener (MainMenu.Messages.MENU_HIDING, Enable);
#endif
        }
	}

	private void Enable(object parameters)
	{
		EnableTouch (true);
	}

	private void Disable(object parameters)
	{
		EnableTouch (false);
	}
	
	public void EnableTouch(bool enable)
	{
		if(touchObjects != null)
		{
			foreach(GameObject touchObject in touchObjects)
			{
				if(touchObject != null && touchObject.GetComponent<TouchZone>() != null)
					touchObject.GetComponent<TouchZone>().enabled = enable;
			}
		}
	}
}
