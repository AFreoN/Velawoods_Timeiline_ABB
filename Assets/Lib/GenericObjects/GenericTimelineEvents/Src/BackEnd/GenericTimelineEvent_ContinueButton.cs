using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GenericTimelineEvent_ContinueButton : MonoBehaviour {
	
	public void Show (float time)
	{
		Hide (0);
		UITween.fadeTo (gameObject, 1, time, UITween.UIFadeType.easeInSine, false);
	}
	
	public void Hide (float time, bool destroyOnEnd = false)
	{
		UITween.fadeTo (gameObject, 0, time, UITween.UIFadeType.easeInSine, false);
		if (destroyOnEnd) 
			Destroy (gameObject, time + 0.1f);
	}
	
	public void AddButtonListener (UnityEngine.Events.UnityAction call)
	{
		GetComponent<Button> ().onClick.AddListener (call);
	}
}
