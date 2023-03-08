using UnityEngine;
using System.Collections;

public class Tutorial_Static_Prompt : MonoBehaviour {


	private float _offset = 100;
	
	private GameObject _promptButton;
	private GameObject Prompt {
		get {
			if (_promptButton == null)
				_promptButton = transform.Find ("Button").gameObject;
			return _promptButton;
		}
	}
	
	
//-Public methods-------------------------------------------------------------------------------------------------------------------------
	
	public void Enter (float lerpTime)
	{
		iTween.Stop (Prompt);
		iTween.MoveTo (Prompt, iTween.Hash ("x" , -1 * Prompt.GetComponent<RectTransform> ().rect.width - _offset,  "time" , lerpTime, "islocal" , true, "easetype", "easeOutQuart"));
	}
	
	public void Exit (float lerpTime)
	{
		iTween.Stop (Prompt);
		iTween.MoveTo (Prompt, iTween.Hash ("x" , 0,  "time" , lerpTime, "islocal" , true, "easetype", "easeInQuart"));
	}
	
	public void SwitchColors (float lerpTime)
	{
		UITween.fadeTo (transform.Find ("Button/Text").gameObject, Color.red, lerpTime, UITween.UIFadeType.easeInSine, false);
		UITween.fadeTo (transform.Find ("Button/QuestionMark").gameObject, Color.red, lerpTime, UITween.UIFadeType.easeInSine, false);
	}
}




































