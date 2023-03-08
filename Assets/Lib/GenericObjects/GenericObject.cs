using UnityEngine;
using UnityEngine.UI;
using System.Collections;

using TMPro;
using CoreLib;

/// <summary>
/// Base class for generic objects. Contains widely used methods for generic UI objects.
/// </summary>
public class GenericObject : MonoBehaviour {

	[Header ("If ticked, base methods affect subComponents as well")] [Space (10)]
	public bool _affectSubComponents = true;
	/// <summary> Subcomponents which will be affected by the methods of this class. </summary>
	public GameObject[] _subComponents;
	
	protected string _UISelectSound = "Audio/UI_Select";
	
	
//-Interface-------------------------------------------------------------------------------------------------------

	public virtual void Show (float seconds = 0.0f)
	{
		Show (new object[] {seconds});
	}
	
	public virtual void Hide (float seconds = 0.0f)
	{
		Hide (new object[] {seconds});
	}

	/// Fade in this object. (Hides the object first and then fades it in) | Parameter list: {lerpTime}
	public virtual void Show (object[] paramList)
	{
		float seconds = (float) paramList [0];
		
		if (gameObject.activeSelf == false) gameObject.SetActive (true);
		
		Hide (new object[] {0.0f});
		UITween.fadeTo (gameObject, 1, seconds, UITween.UIFadeType.easeInSine, false);
		
		if (_affectSubComponents && _subComponents != null && _subComponents.Length > 0) {
			foreach (GameObject subComponent in _subComponents)
			{
				UITween.fadeTo (subComponent, 1, seconds, UITween.UIFadeType.easeInSine, false);
			}
		}
	}
	
	/// Fade out this object. | Parameter list: {lerpTime}
	public virtual void Hide (object[] paramList)
	{
		float seconds = (float) paramList [0];

		UITween.fadeTo (gameObject, 0, seconds, UITween.UIFadeType.easeInSine, false);
		
		if (_affectSubComponents && _subComponents != null && _subComponents.Length > 0) {
			foreach (GameObject subComponent in _subComponents)
			{
				UITween.fadeTo (subComponent, 0, seconds, UITween.UIFadeType.easeInSine, false);
			}
		}
	}
	
	public virtual void ObjectSelected ()
	{
		PlayAudio (_UISelectSound);
	}
	
	public virtual void PlayAudio (string audioFilePath, float volumeScale = 1.0f)
	{
		if (AudioManager.Instance != null)
			AudioManager.Instance.PlayAudio (audioFilePath, CoreLib.AudioType.SFX, volumeScale);
	}
}














