using UnityEngine;
using System.Collections;

/*
 * Gore: A class to extend the Animator to track if someone is taking control of its speed. 
 */

public static class AnimatorExtention 
{
	public class TrackedInstanceInformation
	{
		public long InstanceID;
		public long TrackerID; 
		public bool InUse;
	}
	
	private static System.Collections.Generic.List<TrackedInstanceInformation> TrackedList = new System.Collections.Generic.List<TrackedInstanceInformation>();
	private static System.Runtime.Serialization.ObjectIDGenerator IDGenerator = new System.Runtime.Serialization.ObjectIDGenerator();

	public static bool IsBeingTracked(this Animator animator)
	{
		bool isNewInList = false;
		
		long instanceID = IDGenerator.GetId(animator, out isNewInList);
		
		if(!isNewInList)
		{
			//return TrackedList.Where((key) => { return key.InstanceID == instanceID;}).ToList().Count > 0;

			// Changed so it doesn't use Linq
			foreach( TrackedInstanceInformation tii in TrackedList ) {
				if ( tii.InstanceID == instanceID ) return true;
			}

			return false;
		}
		else
		{
			return false;
		}
	}


	public static bool IsBeingTracked(this Animator animator, object tracker)
	{		
		bool isNewInList = false;
		bool isTrackerNewInList = false;

		long instanceID = IDGenerator.GetId(animator, out isNewInList);
		long trackerID = IDGenerator.GetId(tracker, out isTrackerNewInList);

		if(!isNewInList && !isTrackerNewInList)
		{
			//return TrackedList.Where((key) => { return key.InstanceID == instanceID && key.TrackerID == trackerID;}).ToList().Count > 0;

			// Changed so it doesn't use Linq
			foreach( TrackedInstanceInformation tii in TrackedList ) {
				if ( tii.InstanceID == instanceID && tii.TrackerID == trackerID ) return true;
			}

			return false;
		}
		else
		{
			return false;
		}
	}
	
	public static bool AddTracking(this Animator animator, object tracker)
	{
		bool isNewInList = false;
		bool _notNeeded = false;
		long instanceID = IDGenerator.GetId(animator, out isNewInList);
		long trackerID = IDGenerator.GetId(tracker, out _notNeeded);
		
		if(isNewInList)
		{
			// The item has not been tracked before so just add it. 
			TrackedList.Add(new TrackedInstanceInformation() { InstanceID = instanceID, InUse = true, TrackerID = trackerID});
			
			return true;
		}
		else
		{
			//System.Collections.Generic.List<TrackedInstanceInformation> itemsTrackingAnimator = TrackedList.Where((key) => { return key.InstanceID == instanceID;}).ToList();

			// Changed so it doesn't use Linq
			System.Collections.Generic.List<TrackedInstanceInformation> itemsTrackingAnimator = new System.Collections.Generic.List<TrackedInstanceInformation>();

			foreach( TrackedInstanceInformation tii in TrackedList ) {
				if ( tii.InstanceID == instanceID ) itemsTrackingAnimator.Add( tii );
			}

			if(itemsTrackingAnimator.Count > 0)
			{
				// The animator is already being tracked.
				
				if(itemsTrackingAnimator[0].TrackerID == trackerID)
				{
					// The item is already being tracked by this tracker.
					
					return false;
				}
				else
				{
					// The item is being tracked by someone else.
					
					return true;
				}
			}
			else
			{
				// The animator is not being tracked anymore so add it again. 
				
				TrackedList.Add(new TrackedInstanceInformation() { InstanceID = instanceID, InUse = true, TrackerID = trackerID});
				
				return true;
			}
		}
	}
	
	public static void RemoveTracking(this Animator animator, object tracker)
	{
		bool _notNeeded = false;
		long instanceID = IDGenerator.GetId(animator, out _notNeeded);
		long trackerID = IDGenerator.GetId(tracker, out _notNeeded);
		
		TrackedList.RemoveAll((key) => { return key.InstanceID == instanceID && key.TrackerID == trackerID;});
	}
}