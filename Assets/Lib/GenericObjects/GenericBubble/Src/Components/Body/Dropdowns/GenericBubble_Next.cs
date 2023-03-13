using UnityEngine;
using System.Collections;

using CoreSystem;

public class GenericBubble_Next : GenericBubble_DropdownBase {
	
	public GameObject _bubbleParent;
	
	bool _interactable = true;
	
	public override void SlideUp (float slideTime, float waitTime)
	{
		base.SlideUp (slideTime, waitTime);
		
		_interactable = false;
	}
	
	public override void SlideDown (float slideTime, float waitTime)
	{
		base.SlideDown (slideTime, waitTime);
		
		_interactable = true;
	}
	
	public override void DropdownClicked ()
	{
		if (GetState != State.Showing) return;
			
		GenericBubble bubbleScript = _bubbleParent.GetComponent<GenericBubble> ();

		if (bubbleScript.Buttons.RecordButton.Interactable || bubbleScript.Buttons.RecordButton._disableRecording)
		{		
			if (_interactable)
			{
				base.DropdownClicked ();
				
				_interactable = false;
				
				AudioManager.Instance.StopAudio (CoreSystem.AudioType.Dialogue);
			
				bubbleScript.HideAllDropdowns ();
				bubbleScript.NextDropdownPressed();
				if (bubbleScript.Buttons.RecordButton) bubbleScript.Buttons.RecordButton.Continue ();
				bubbleScript.CarnegieNext();
			}
		}
	}
}
