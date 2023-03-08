using UnityEngine;
using System.Collections;

// Properties & methods shared by SUBTITLE BUBBLES: active, passive, etc.
public class SubtitleBubble : ConversationBubble {
	[Header ("Subtitle Bubble Base Params")] [Space (10)]
	
	public float _yOffset = 10.0f;
	public float _referenceBubbleScale = 0.8f;
	public float _referenceSpacing = 1.0f;
	
	public bool _isActive;
	
	public DialogueEventData.DialogueData _dialogueData = new DialogueEventData.DialogueData ();
	
	//-Interface----------------------------------------------------------------------------------------------------------------------------------

	public override void SlideFromBottom (float slideTime, float offset=0, bool destroyOnEnd = false)
	{
		//Show ();
		SlideTo (new Vector2 (0.0f, getOffsetUnderScreen (GetBounds ())), 0.0f, false);
		SlideTo (new Vector2 (0.0f, getOffsetAboveBottom (GetBounds ())), slideTime, destroyOnEnd);
		
		if (Buttons.BackButton != null && Buttons.BackButton.Interactable == true)
		{
			Buttons.BackButton.Interactable = false;
			if (Buttons.BackButton != null) Buttons.BackButton.SetInteractableAfter (true, slideTime);
		}
	}

	public float getOffsetAboveBottom (Vector4 bounds)
	{
		float yOffset = _yOffset; // Offset
		yOffset += (-1 * 1536.0f / 2.0f - bounds.z); // Substract distance between bubble and screen bottom
		
		return yOffset;
	}
	
	public virtual void ToReference (GameObject baseBubble, float slideTime, float buttonsFadeTime)
	{
		Vector4 baseBounds = baseBubble.GetComponent<SubtitleBubble> ().GetBounds ("Body");
		Vector4 bubbleBounds = GetBounds ();
		
		float yOffset = -1 * 1536.0f / 2.0f + _yOffset;
		yOffset += Mathf.Abs (baseBounds.z - baseBounds.w);
		yOffset += _referenceSpacing;
		yOffset += ((bubbleBounds.w - bubbleBounds.z) * _referenceBubbleScale) / 2.0f;
		yOffset -= transform.localPosition.y;
		
		if (Buttons.ReplayButton != null) Buttons.ReplayButton.Interactable (false);
		if (Buttons.ForwardButton != null) Buttons.ForwardButton.Interactable (false);

		Slide (Vector2.zero, new Vector2 (0.0f, yOffset), slideTime);
		FadeButtons (true, buttonsFadeTime);
		iTween.ScaleTo (gameObject, iTween.Hash ("x", _referenceBubbleScale, "y", _referenceBubbleScale, "time", slideTime, "islocal", true, "easetype", "easeInSine"));
	}
	
	public virtual void FromReference (float slideTime)
	{
		SlideTo (new Vector2 (0.0f, getOffsetAboveBottom (GetBounds ())), slideTime, false);
		FadeButtons (false, slideTime); 
		iTween.ScaleTo (gameObject, iTween.Hash ("x", 1.0f, "y", 1.0f, "time", slideTime, "islocal", true, "easetype", "easeInSine"));
		
		if (Buttons.ReplayButton != null) Buttons.ReplayButton.SetInteractableAfter (true, slideTime);
		if (Buttons.ForwardButton != null) Buttons.ForwardButton.SetInteractableAfter (true, slideTime);
	}
	
	public override void SwitchBubbleText (string dialogueText, bool replaceTextData = false, float time = 0.0f, int index = 0)
	{
		base.SwitchBubbleText (dialogueText, replaceTextData, time, index);
		
		if (Buttons.ReplayButton)
			Buttons.ReplayButton.Interactable (true);
		if (Buttons.ForwardButton)
			Buttons.ForwardButton.SetInteractableAfter (true, 0.2f);//.Interactable (true);
	}
}







































