using UnityEngine;
using UnityEditor;
using CoreSystem;
using System.Collections;

public static class TimelineControls {

	[MenuItem("VELA/Timeline Controls/Skip Forward %.")]
	static void SkipForward() {
		//Only allow skipping in play mode
		if(Application.isPlaying)
		{



			//Debug.Log ("Skipping ahead");
			CoreEventSystem.Instance.SendEvent ( CoreEventTypes.ACTIVITY_SKIP );
		}
	}


	[MenuItem("VELA/Timeline Controls/Skip Backward %,")]
	static void SkipBackward() {
		if(Application.isPlaying)
		{
			//Debug.Log ("Skipping back");
			CoreEventSystem.Instance.SendEvent ( CoreEventTypes.ACTIVITY_REVERSE );
		}
	}
}
