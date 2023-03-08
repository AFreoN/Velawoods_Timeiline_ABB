using UnityEngine;
using System.Collections;

public class SubtitleBubble_Active : SubtitleBubble {
	[Header ("Active Bubble")]

	public GameObject _referenceBubble;
	
	// If space has been made for the sliders (legend || help)
	private bool _sliderSpace = false;
	private float _makeSliderSpaceTime = 1.0f;
	
	
//-Interface-------------------------------------------------------------------------------------------------
	
	public override void Show (float lerpTime)
	{
// Disable recording if skipping learner dialogues is enabled in editor mode
#if UNITY_EDITOR && !CLIENT_BUILD
		if (ConversationManager.Instance._skipLearnerDialogue && Buttons.RecordButton != null)
			Buttons.RecordButton._disableRecording = true;
#endif

		base.Show (lerpTime);
		
		if (_referenceBubble == null)
		{
			Buttons.BackButton.Interactable = false;
		}
	}
	
	public override void FirstAttemptDropdowns ()
	{
		base.FirstAttemptDropdowns ();
		
		if (_sliderSpace == false)
		{
			_sliderSpace = true;
			StartCoroutine (SlidersRoutine (true, true));
		}
	}
	
	public override void SecondAttemptDropdowns ()
	{
		base.SecondAttemptDropdowns ();
		
		if (_sliderSpace == false)
		{
			_sliderSpace = true;
			StartCoroutine (SlidersRoutine (false, true));
		}
	}
	
	public void SecondAttemptDropdownsAfter (float seconds)
	{
		Invoke ("SecondAttemptDropdowns", seconds);
	}
	
	public override void HideAllDropdowns ()
	{
		base.HideAllDropdowns ();
		
		if (_sliderSpace == true)
		{
			_sliderSpace = false;
			StartCoroutine (SlidersRoutine (false, false));
		}
	}
	
	public override void ToReference (GameObject baseBubble, float slideTime, float buttonsFadeTime)
	{
		Buttons.BackButton.Interactable = false;
	
		base.ToReference (baseBubble, slideTime, buttonsFadeTime);
	}
	
	public override void FromReference (float slideTime)
	{
		base.FromReference (slideTime);
		if (Buttons.BackButton != null && _referenceBubble != null)
			Buttons.BackButton.SetInteractableAfter (true, slideTime);
	}
	
	public void SetReferenceBubble (GameObject refBubble)
	{
		_referenceBubble = refBubble;
	}
	

//-Coroutines-----------------------------------------------------------------------------------------------

	private IEnumerator SlidersRoutine (bool isCarnegieFeedback, bool makeSpace)
	{
		float sliderHeight = Dropdowns.MaxSliderHeight;
	
		if (makeSpace)
		{
			SlideTo (new Vector2 (0.0f, sliderHeight), 0.3f, false);
			
			if (_referenceBubble)
				_referenceBubble.GetComponent<SubtitleBubble> ().SlideTo (new Vector2 (0.0f, sliderHeight), _makeSliderSpaceTime, false);
			
			yield return new WaitForSeconds (_makeSliderSpaceTime);
			
			if (isCarnegieFeedback) base.FirstAttemptDropdowns ();
			else base.SecondAttemptDropdowns ();
		}
		else
		{
			base.HideAllDropdowns ();
			
			yield return new WaitForSeconds (_dropdownsSlideTime);
		
			SlideTo (new Vector2 (0.0f, -1 * sliderHeight), 0.3f, false);
			
			if (_referenceBubble)
				_referenceBubble.GetComponent<SubtitleBubble> ().SlideTo (new Vector2 (0.0f, -1 * sliderHeight), _makeSliderSpaceTime, false);
		}
	}
}





























