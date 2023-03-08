using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GenericButton_Record_ColorPulse : MonoBehaviour {
	[Header ("Color Pulse")] [Space (5)]
	
	public float _cycleTime = 3.0f;
	public Color _startingColor = Color.white;
	public Color _finalColor = Color.red;
	
	
	//-Interface-----------------------------------------------------------------------------------------
	
	public void StartPulse ()
	{
		StopCoroutine ("PulseColor");
		StartCoroutine ("PulseColor");
	}
	
	public void StopPulse ()
	{
		StopCoroutine ("PulseColor");
		UITween.fadeTo (gameObject, _startingColor, _cycleTime / 2.0f, UITween.UIFadeType.easeInSine, false);
	}
	
	public void ToColor (float seconds, bool startingColor = false)
	{
		StopCoroutine ("PulseColor");
		UITween.fadeTo (gameObject, (startingColor) ? _startingColor : _finalColor, seconds, UITween.UIFadeType.easeInSine, false);
	}
	
	public void FadeOut (float seconds, bool deactivateOnEnd = false)
	{
		StartCoroutine (HideRoutine (seconds, deactivateOnEnd));
	}
	
	public void FadeIn (float seconds)
	{
		if (gameObject.activeSelf == false) gameObject.SetActive (true);
		
		UITween.fadeTo (gameObject, 0, 0, UITween.UIFadeType.easeInSine, false);
		UITween.fadeTo (gameObject, 1, seconds, UITween.UIFadeType.easeInSine, false);
	}
	
	//-Coroutines----------------------------------------------------------------------------------------
	
	private IEnumerator PulseColor ()
	{	
		float currentTime = 0;	
		while (currentTime / _cycleTime < 1)
		{
			currentTime += Time.deltaTime;
			
			float lerpValue = Mathf.Sin ((currentTime / _cycleTime) * 2.0f * Mathf.PI);
			if (lerpValue < 0) lerpValue *= -1;
			GetComponent<Image> ().color = Color.Lerp (_startingColor, _finalColor, lerpValue);
			
			yield return null;
		}
		GetComponent<Image> ().color = _startingColor;
		
		StartCoroutine ("PulseColor");
		yield return null;
	} 
	
	private IEnumerator HideRoutine (float seconds, bool deactivateOnEnd)
	{
		UITween.fadeTo (gameObject, 0, seconds, UITween.UIFadeType.easeInSine, false);
		
		yield return new WaitForSeconds (seconds + 0.001f);
		if (deactivateOnEnd)
		{
			gameObject.SetActive (false);
		}
	}
}
