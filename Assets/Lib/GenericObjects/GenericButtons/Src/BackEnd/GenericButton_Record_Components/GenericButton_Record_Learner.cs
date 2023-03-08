using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GenericButton_Record_Learner : MonoBehaviour {

	private Image _ring;
	private Image Ring {
		get {
			if (_ring == null)
				_ring = transform.Find ("Ring").GetComponent<Image> ();
			return _ring;
		}
	}

	//-Interface--------------------------------------------------------------------------------

	public void Awake ()
	{
#if CLIENT_BUILD
		GetComponent<Image> ().sprite = PlayerProfile.Instance.Avatar.Picture;
//		Ring.sprite = PlayerProfile.Instance.AvatarRing;
//		Ring.color = PlayerProfile.Instance.AvatarRingColor;
#endif
	}

	public void ShowLearner (float seconds = 0.0f)
	{
		gameObject.SetActive (true);
		
		GetComponent<Image> ().color = new Color (1,1,1,0);
		
		StartCoroutine ("fadeTo", new object[] {1f, false, seconds});
	}
	
	public void HideLearner (float seconds = 0.0f, bool deactivateOnEnd = true)
	{
		//Debug.Break ();
		StartCoroutine ("fadeTo", new object[] {0f, deactivateOnEnd, seconds});
	}
	
	
	//-Coroutines------------------------------------------------------------------------------
	
	private IEnumerator fadeTo (object[] paramList)
	{
		float fadeToAlpha = (float) paramList [0];
		bool deactivateOnEnd = (bool) paramList [1];
		float seconds = (float) paramList [2];
		
		UITween.fadeTo (gameObject, fadeToAlpha, seconds, UITween.UIFadeType.easeInSine, false);
		UITween.fadeTo (Ring.gameObject, fadeToAlpha, seconds, UITween.UIFadeType.easeInSine, false);
		
		 yield return new WaitForSeconds (seconds + 0.001f);
		  if (deactivateOnEnd) gameObject.SetActive (false);
	}
}
