using UnityEngine;
using System.Collections;

public class ConversationBubble : GenericBubble {

//-Interface------------------------------------------------------------------------------------------

	//-Buttons--------------------------------------------------------------
	public void ForwardButtonPressed ()
	{
		ConversationManager.Instance.SkipDialogue ();

		if (Buttons.ReplayButton)
			Buttons.ReplayButton.Interactable (false);
		if (Buttons.ForwardButton)
			Buttons.ForwardButton.Interactable (false);
	}
	
	public override void ReplayButtonPressed ()
	{
		base.ReplayButtonPressed ();
		
		ConversationManager.Instance.ReplayDialogue ();
	}
	
	public override void RecordButtonOnComplete ()
	{
		base.RecordButtonOnComplete ();
		
		ConversationManager.Instance.OnCarnegieSuccess ();
	}
	
	public void BackButtonCliked ()
	{
		if (Buttons.BackButton.Interactable == false)
			return;

		if (Buttons.RecordButton != null && Buttons.RecordButton.Interactable == false)
		{
			Debug.Log ("AHA!");
			return;
		}

		Buttons.BackButton.Interactable = false;

		ConversationManager.Instance.ReplayPreviousDialogue ();
	}
	

//-Lerps----------------------------------------------------------------

	public virtual void SlideFromBottom (float slideTime, float offset = 0, bool destroyOnEnd = false)
	{
		Vector4 bounds = GetBounds ();
		SlideFrom (new Vector2 (0.0f, getOffsetUnderScreen (bounds) + offset), slideTime, destroyOnEnd);
	}
	
	public virtual void SlideToBottom (float slideTime, bool destroyOnEnd = false)
	{
		Vector4 bounds = GetBounds ();
		SlideTo (new Vector2 (0.0f, getOffsetUnderScreen (bounds)), slideTime, destroyOnEnd);
	}	
	
	
//-Privates & Protected-------------------------------------------------------------------------------
	
	protected float getOffsetUnderScreen (Vector4 bounds)
	{
		float yOffset = -10; // Offset
		yOffset += (-1 * 1536.0f / 2.0f - bounds.z); // Substract distance between bubble and screen bottom
		yOffset += bounds.z - bounds.w; // Substract total bubble height
		
		return yOffset;
	}
	
	protected virtual void FadeButtons (bool fadeOut, float lerpTime)
	{
		Buttons.gameObject.BroadcastMessage ((fadeOut) ? "Hide" : "Show", new object[] {lerpTime});
	}
}
