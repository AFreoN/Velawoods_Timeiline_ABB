using UnityEngine;
using System.Collections;

public class GenericButton_Record_Analyser : MonoBehaviour {
	[Header ("Analyser")] [Space (5)]

	public float _spinSpeed = 0.94f;


	//-Interface---------------------------------------------------------------------

	public void StartSpin (float fadeInTime)
	{
		gameObject.SetActive (true);
		UITween.fadeTo (gameObject, 0, 0, UITween.UIFadeType.easeInSine, false);
		UITween.fadeTo (gameObject, 1, fadeInTime, UITween.UIFadeType.easeInSine, false);
		
		StartCoroutine ("SpinRoutine", _spinSpeed);
	}
	
	public void StopSpin (float fadeOutTime)
	{
		UITween.fadeTo (gameObject, 1, fadeOutTime, UITween.UIFadeType.easeInSine, false);
		StartCoroutine ("DeactivateAfter", fadeOutTime + 0.001f);
		
		StopCoroutine ("SpinRoutine");
	}

		
	//-Coroutines-------------------------------------------------------------------

	IEnumerator DeactivateAfter (float seconds)
	{
		yield return new WaitForSeconds (seconds);
		gameObject.SetActive (false);
	}

	IEnumerator SpinRoutine (float speed)
	{
		while (1>0)
		{
			transform.Rotate (0.0f, 0.0f, -22.5f);
			yield return new WaitForSeconds (1.0f - speed);
		}
	}
}
