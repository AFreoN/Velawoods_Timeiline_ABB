using UnityEngine;
using System.Runtime.Serialization;
using System.Collections.Generic;
using WellFired;
using CoreSystem;

/*
 * Gore: Restoring the state of affected mission objects. This class could be moved to another file if it is needed elsewhere.
 * This could be extended to take snap shots of the positions at different times in the timeline.
 */

/// <summary>
/// A class used to manage the state of objects in a sequence.
/// </summary>
public class ObjectStateManager : MonoSingleton<ObjectStateManager>
{
	/// The information that we want to record for the objects.
	private class RestoreInformation
	{
		public GameObject GameObj;
		public Vector3 Position;
		public Quaternion Rotation;
		public Vector3 Scale;
		public Transform Parent;
		public bool Active;
		public long InstanceID;
		public Vector3 RootPosition;
		public Quaternion RootRotation;
		public bool HasAnimator;
		// Used to Debug.
		public string Name;
	}
		
	/// <summary>
	/// Has the information already been cached for the timeline.
	/// </summary>
	private bool HasCachedRestoreInformation = false;
	
	/// <summary>
	/// The objects to maintain the state of.
	/// </summary>
	private List<RestoreInformation> ObjectsRestoreInformation = new List<RestoreInformation>();

	private Material CurrentSkyboxMaterial;
	
	/// <summary>
	/// Determines if there has been at least one chaching.
	/// </summary>
	public bool HasBeenCached()
	{
		return HasCachedRestoreInformation;
	}
	
	protected override void Init ()
	{
		CoreEventSystem.Instance.AddListener (CoreEventTypes.LEVEL_CHANGE, LevelChanged);
	}
	
	private void LevelChanged(object callbackParams)
	{
		HasCachedRestoreInformation = false;
	}
	
	/// <summary>
	/// Store the state of each of the affected objects in the main sequence.
	/// </summary>
	public void CacheObjectInformation()
	{
		// Make sure that we don't double up the content.
		ObjectsRestoreInformation.Clear();
		
		/*foreach(USTimelineContainer timelineContainer in SequenceManager.Instance.MainSequence.TimelineContainers)
		{
			// Make sure to check if there is an affected object as the observer doesn't have one.
			if(timelineContainer.AffectedObject != null)
			{
				// Take a copy of all of the information we require.
				AddGameObjectToCache(timelineContainer.AffectedObject.gameObject);
			}

			foreach(USTimelineBase timeline in timelineContainer.Timelines)
			{
				if(timeline is USTimelineEvent)
				{
					foreach(USEventBase eventBase in (timeline as USTimelineEvent).Events)
					{
						if(eventBase.AffectedObjects.Count > 0)
						{
							foreach(Transform gameObject in eventBase.AffectedObjects)
							{
								if(gameObject == null)
								{
									Debug.LogError("One of the affected objects is not set on the event: " + eventBase.name + "at firetime: " + eventBase.FireTime + " on timeline: " + eventBase.Timeline.name);
								}
								else
								{
									AddGameObjectToCache(gameObject.gameObject);
								}
							}
						}
					}
				}
			}
		}*/

		// Remember what material is used as the skybox.
		CurrentSkyboxMaterial = RenderSettings.skybox;
		
		//Cache all objects that are referenced from events (not necesarrily on timeline)
		/*FlashAndTouch[] flashEvents = GameObject.FindObjectsOfType<FlashAndTouch> ();
		foreach(FlashAndTouch flashEvent in flashEvents)
		{
			foreach(GameObject flashObject in flashEvent._touchObjects)
			{
				AddGameObjectToCache(flashObject);
			}
		}*/
		
		HasCachedRestoreInformation = true;
	}
	
	private void AddGameObjectToCache(GameObject gameObj)
	{
		/*if (!gameObj) 
		{
			Debug.LogError("The affected game obejct is null");
			return;
		}

		Animator animator = gameObj.GetComponent<Animator>();

		ObjectsRestoreInformation.Add(new RestoreInformation() 
		                              { 
			GameObj = gameObj,
			Position = gameObj.transform.localPosition,
			Rotation = gameObj.transform.localRotation,
			Scale = gameObj.transform.localScale,
			Parent = gameObj.transform.parent,
			Active = gameObj.activeSelf,
			InstanceID = gameObj.GetInstanceID(),
			Name = gameObj.name,
			HasAnimator = animator != null,
			RootPosition = animator != null ? animator.rootPosition : Vector3.zero,
			RootRotation = animator != null ? animator.rootRotation : Quaternion.identity
		});*/
	}
	
	/// <summary>
	/// Restore all of the objects with the information stored.
	/// </summary>
	public void RestoreGameObjects()
	{
		/*if(!HasCachedRestoreInformation)
		{
			Debug.LogWarning("Trying to restore the state of affected objects without taking a snapshot");
			return;
		}

		foreach(RestoreInformation toRestore in ObjectsRestoreInformation)
		{
			if(toRestore == null || toRestore.GameObj == null)
			{
				Debug.LogError("Game object: deleted. Unable to restore");
				continue;
			}
			toRestore.GameObj.transform.SetParent(toRestore.Parent, true);
			toRestore.GameObj.transform.localPosition = toRestore.Position;
			toRestore.GameObj.transform.localEulerAngles = toRestore.Rotation.eulerAngles;
			toRestore.GameObj.transform.localRotation = toRestore.Rotation;
			toRestore.GameObj.transform.localScale = toRestore.Scale;
			toRestore.GameObj.SetActive(toRestore.Active);

			if(toRestore.HasAnimator)
			{
				Animator animator = toRestore.GameObj.GetComponent<Animator>();
				animator.rootPosition = toRestore.RootPosition;
				animator.rootRotation = toRestore.RootRotation;
			}
		}*/

		RenderSettings.skybox = CurrentSkyboxMaterial;
	}
}
