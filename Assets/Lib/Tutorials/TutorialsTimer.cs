using UnityEngine;
using System.Collections;

public class TutorialsTimer : MonoBehaviour {

	private static GameObject _attachObject = null;
	private static GameObject AttachToObject {
		get {
			if (_attachObject==null)
				_attachObject = GameObject.Find ("MainCanvas").gameObject;
			return _attachObject;
		}
	}

	public delegate void Callback (bool playTutorial);

	public static TutorialsTimer StartTimer (float time, Callback callback, bool playTutorialOnEnd)
	{
		if (AttachToObject.GetComponent<TutorialsTimer> () != null)
			DestroyImmediate (AttachToObject.GetComponent<TutorialsTimer> ());
		
		TutorialsTimer myTimer = AttachToObject.AddComponent<TutorialsTimer>();
		myTimer.StartCoroutine ("TimerSequence", new object[] {time, callback, playTutorialOnEnd});
		
		return myTimer;
	}
	
	public void StopTimer ()
	{
		DestroyImmediate (this);
	}
	
	void OnDestroy ()
	{
		StopCoroutine ("TimerSequence");
	}
	
	private IEnumerator TimerSequence (object[] args)
	{
		float time 		  = (float)args[0];
		Callback callback = (Callback)args[1];
		bool playTutorialOnEnd = (bool)args[2];
		
		yield return new WaitForSeconds (time);
		
		if (callback != null)
			callback (playTutorialOnEnd);
		StopTimer ();
	}
}
