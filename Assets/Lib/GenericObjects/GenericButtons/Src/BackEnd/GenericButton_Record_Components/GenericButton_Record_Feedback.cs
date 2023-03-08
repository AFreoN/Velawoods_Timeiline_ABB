using UnityEngine;
using System.Collections;

public class GenericButton_Record_Feedback : MonoBehaviour {
	[Header ("Feedback")] [Space (5)]
	
	private GameObject _tick;
	private GameObject Tick {
		get  
		{
			if (_tick == null)
				_tick = transform.Find ("Tick").gameObject;
			return _tick;
		}
	}
	
	private GameObject _cross;
	private GameObject Cross {
		get  
		{
			if (_cross == null)
				_cross = transform.Find ("Cross").gameObject;
			return _cross;
		}
	}
	
	private GameObject _noWifi;
	private GameObject NoWifi {
		get  
		{
			if (_noWifi == null)
				_noWifi = transform.Find ("NoWifi").gameObject;
			return _noWifi;
		}
	}
	
	
//-Interface----------------------------------------------------------------

	public void ShowTick (float fadeInTime)
	{
		Tick.SetActive (true);
		Tick.transform.localScale = new Vector3 (0.0f, 0.0f, 1.0f);
		iTween.ScaleTo (Tick, iTween.Hash ("x", 1.0f, "y", 1.0f, "time", fadeInTime, "islocal", true, "easetype", "easeInSine"));
	}
	
	public void ShowCross (float fadeInTime)
	{
		Cross.SetActive (true);
		Cross.transform.localScale = new Vector3 (0.0f, 0.0f, 1.0f);
		iTween.ScaleTo (Cross, iTween.Hash ("x", 1.0f, "y", 1.0f, "time", fadeInTime, "islocal", true, "easetype", "easeInSine"));
	}
	
	public void ShowNoWiFi (float fadeInTime)
	{
		NoWifi.SetActive (true);
		NoWifi.transform.localScale = new Vector3 (0.0f, 0.0f, 1.0f);
		iTween.ScaleTo (NoWifi, iTween.Hash ("x", 1.0f, "y", 1.0f, "time", fadeInTime, "islocal", true, "easetype", "easeInSine"));
	}
	
	public void FadeOut (float fadeOutTime)
	{
		if (Tick.activeSelf)
		{
			StartCoroutine (fadeOutRoutine (Tick, fadeOutTime));
		}
		else
		if (Cross.activeSelf)
		{
			StartCoroutine (fadeOutRoutine (Cross, fadeOutTime));
		}
		else
		if (NoWifi.activeSelf)
		{
			StartCoroutine (fadeOutRoutine (NoWifi, fadeOutTime));
		}
	}
	
	
//-Coroutines-------------------------------------------------------------
	
	IEnumerator fadeOutRoutine (GameObject obj, float seconds)
	{
		UITween.fadeTo (obj, 0, seconds, UITween.UIFadeType.easeInSine, false);
		yield return new WaitForSeconds (seconds + 0.001f);
		UITween.fadeTo (obj, 1, 0, UITween.UIFadeType.easeInSine, false);
		obj.SetActive (false);
	}
}
