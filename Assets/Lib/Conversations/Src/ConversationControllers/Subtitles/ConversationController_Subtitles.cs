using UnityEngine;
using System.Collections;

public class ConversationController_Subtitles : ConversationController {
	[Header ("Subtitles Controller")] [Space(10)]
	
	// Bubbles prefabs
	public GameObject _bubblePassive;
	public GameObject _bubbleActive;
    public GameObject _bubblePassiveDYN;
	
	// Previous Dialogues Container
	private GameObject _previousBubblesParent;
	public GameObject PreviousBubblesParent {
		get {
			if (_previousBubblesParent == null)
				_previousBubblesParent = transform.Find ("PreviousDialogue").gameObject;
			return _previousBubblesParent;
		}
	}
	
	// Current Dialogues Container
	private GameObject _currentBubblesParent;
	public GameObject CurrentBubblesParent {
		get {
			if (_currentBubblesParent == null)
				_currentBubblesParent = transform.Find ("CurrentDialogue").gameObject;
			return _currentBubblesParent;
		}
	}
	
	/// <summary>
	/// Current main bubble.</summary>
	private SubtitleBubble CurrentBubble {
		get {
			if (CurrentBubblesParent.transform.childCount > 0) {
				return CurrentBubblesParent.transform.GetChild (CurrentBubblesParent.transform.childCount - 1).GetComponent<SubtitleBubble> ();
			}
			return null;
		}
	}
	
	/// <summary>
	/// Previous bubble (stored as inactive / active in case of a reference bubble). </summary>
	private SubtitleBubble PreviousBubble 
	{
		get {
			if (PreviousBubblesParent.transform.childCount > 0) {
				return PreviousBubblesParent.transform.GetChild (PreviousBubblesParent.transform.childCount - 1).GetComponent<SubtitleBubble> ();
			}
			return null;
		}
	}
	
	
//-Interface------------------------------------------------------------------------------------------------

	// Beginning of a dialogue bubble
	public override void StartDialogue (DialogueEventData.DialogueData dialogueData)
	{
		base.StartDialogue (dialogueData);
		
	// If no bubble is present
		if (CurrentBubble == null)
		{
		// And next one is active
			if (dialogueData.isLearner)
			{
				NewBubble (dialogueData);
				
				if (PreviousBubble != null)
				{
					if (PreviousBubble._dialogueData.isLearner)
						PreviousBubble.Buttons.RecordButton._disableRecording = true;
					
					if (dialogueData.hideReferenceBubble == false)
					{
						CurrentBubble.gameObject.GetComponent<SubtitleBubble_Active> ().SetReferenceBubble (PreviousBubble.gameObject);
						PreviousBubble.Show (_lerpTime);
						PreviousBubble.ToReference (CurrentBubble.gameObject, _lerpTime, 0.0f);
					}
					else
					{
						PreviousBubble.Hide (true, _lerpTime);
					}
				}
				
				CurrentBubble.Show (_lerpTime);
				CurrentBubble.SlideFromBottom (_lerpTime);
			}
		// And next one is passive
			else
			{
				NewBubble (dialogueData);
				CurrentBubble.Show (_lerpTime);
				CurrentBubble.SlideFromBottom (_lerpTime);
			}
			return;
		}
		else
	// If current bubble is passive
		if (CurrentBubble._isActive == false)
		{
		// And next bubble is active
			if (dialogueData.isLearner)
			{
				SetPreviousBubble (CurrentBubble.gameObject);
				PreviousBubble.gameObject.SetActive (true);
				CurrentBubble.Hide (true, 0.0f);
				
				NewBubble (dialogueData);
				if (dialogueData.hideReferenceBubble == false)
					CurrentBubble.gameObject.GetComponent<SubtitleBubble_Active> ().SetReferenceBubble (PreviousBubble.gameObject);
				CurrentBubble.Show (_lerpTime);
				CurrentBubble.SlideFromBottom (_lerpTime);
				if (dialogueData.hideReferenceBubble == false)
					PreviousBubble.ToReference (CurrentBubble.gameObject, _lerpTime, _lerpTime);
				else
					PreviousBubble.Hide (true, _lerpTime);
				
				return;
			}
		// And next bubble is passive
			else
			{
				// But they're both actually the same
				if (CurrentBubble && (CurrentBubble._textData [0].text == dialogueData.dialogueText [0].text) && (CurrentBubble._dialogueData.character == dialogueData.character) && (CurrentBubble._dialogueData.characterIllustration == dialogueData.characterIllustration))
				{
					// Do nothing
				}
				else // They're different
				{
					SetPreviousBubble (CurrentBubble.gameObject);
					// One bubble transition
					SwitchBubble (CurrentBubble.gameObject, dialogueData);
				}
				return;
			}
		}
		else
	// If current bubble is active
		if (CurrentBubble._isActive)
		{
			// And the reference bubble equals the next bubble
			if (PreviousBubble && PreviousBubble._textData [0].text == dialogueData.dialogueText [0].text && PreviousBubble._dialogueData.character == dialogueData.character && PreviousBubble._dialogueData.characterIllustration == dialogueData.characterIllustration)
			{
				CurrentBubble.SlideToBottom (_lerpTime, true);

				GameObject referenceBubble = Instantiate (PreviousBubble.gameObject) as GameObject;
				referenceBubble.transform.SetParent (CurrentBubblesParent.transform, false);
				referenceBubble.GetComponent<SubtitleBubble> ().FromReference (_lerpTime);
				referenceBubble.GetComponent<SubtitleBubble> ()._textData = PreviousBubble._textData;
				referenceBubble.GetComponent<SubtitleBubble> ()._dialogueData = dialogueData;
				
				// If active bubble is being re-played
				if (dialogueData.isLearner)
				{	
					// Show dropdowns so user can skip
					referenceBubble.GetComponent<SubtitleBubble_Active> ().SecondAttemptDropdownsAfter (_lerpTime + 0.1f);
					// Don't count skipping as a fail this time
					referenceBubble.GetComponent<SubtitleBubble_Active> ().Buttons.RecordButton._sendFailOnSkip = false;
					// Disable re-recording
					referenceBubble.GetComponent<SubtitleBubble_Active> ().Buttons.RecordButton._disableRecording = true;
					// Reset model answer
					//referenceBubble.GetComponent<SubtitleBubble_Active> ().Dropdowns.Help.SetAudioClip (GetDefaultAudioClip (dialogueData));
					// Continue playing 
					ConversationManager.Instance.ResumeConversationSequence ();
				}
				
				PreviousBubble.gameObject.SetActive (false);
			}
		// And the reference bubble is different from the next bubble
			else
			{
			// And next bubble is active
				if (dialogueData.isLearner)
				{
					SetPreviousBubble (CurrentBubble.gameObject, _lerpTime);
					PreviousBubble.gameObject.SetActive (true);
					
					CurrentBubble.Hide (true, 0.0f);
					NewBubble (dialogueData);
					if (dialogueData.hideReferenceBubble == false)
						CurrentBubble.gameObject.GetComponent<SubtitleBubble_Active> ().SetReferenceBubble (PreviousBubble.gameObject);
					CurrentBubble.Show (_lerpTime);
					CurrentBubble.SlideFromBottom (_lerpTime);
					if (dialogueData.hideReferenceBubble == false)
						PreviousBubble.ToReference (CurrentBubble.gameObject, _lerpTime, _lerpTime);
					else
						PreviousBubble.Hide (true, _lerpTime);
				} 
				else
			// And next bubble is passive
				{
					SetPreviousBubble (CurrentBubble.gameObject, _lerpTime);
					
					CurrentBubble.Hide (true, _lerpTime);
					NewBubble (dialogueData);
					CurrentBubble.Show (_lerpTime);
					CurrentBubble.SlideFromBottom (_lerpTime);
				}
			}	
			return;
		}
	}
	
	// End of a dialogue bubble
	public override void EndDialogue (DialogueEventData.DialogueData dialogueData)
	{
		base.EndDialogue (dialogueData);
		
		if (dialogueData.fadeOnEnd && CurrentBubble != null)
		{
			SetPreviousBubble (CurrentBubble.gameObject);
			CurrentBubble.Hide (true, _lerpTime);
		}
	}
	
	// End this conversation (destroy on end)
	public override void EndConversation ()
	{
		base.EndConversation ();
		
		StartCoroutine (EndConversationRoutine ());
	}
	
	
//-Privates---------------------------------------------------------------------------------------------------
	
	/// <summary>
	/// Create new bubble in the CurrentBubblesParent </summary>
	private GameObject NewBubble (DialogueEventData.DialogueData dialogueData)
	{
		GameObject newBubble;
		
		// Assign prefab
		GameObject prototype;
		if (dialogueData.isLearner)
		{
			prototype = _bubbleActive;
			prototype.GetComponent<SubtitleBubble> ()._isActive = true;
		}
		else if(dialogueData.isDidYouNotice)
		{
            prototype = _bubblePassiveDYN;
            prototype.GetComponent<SubtitleBubble>()._isActive = false;
        }
        else{
            prototype = _bubblePassive;
            prototype.GetComponent<SubtitleBubble>()._isActive = false;
        }

		// Place in scene
		newBubble = Object.Instantiate (prototype) as GameObject;
		newBubble.transform.SetParent (CurrentBubblesParent.transform, false);

		SubtitleBubble newBubbleSrc = newBubble.GetComponent<SubtitleBubble> ();

		// Add data
		newBubbleSrc._dialogueData = dialogueData;
		newBubbleSrc.SetTextData (dialogueData.dialogueText);
		if (dialogueData.hidePointer == false)
			newBubbleSrc.SetCharacter (dialogueData.character);
		// If model answer hasn't been set by the bubble (aka. all DialogueTexts don't have model answers), set the default one (soon to be obsolete)
		if (dialogueData.isLearner && newBubbleSrc.Dropdowns.Help.GetAudioClip () == null)
			newBubbleSrc.Dropdowns.Help.SetAudioClip (GetDefaultModelAnswer (dialogueData));	
		
		// Return
		return newBubble;
	}
	
	// One-Bubble transition
	private void SwitchBubble (GameObject bubble, DialogueEventData.DialogueData newDialogueData)
	{
		SubtitleBubble bubbleController = bubble.GetComponent<SubtitleBubble> ();
		
		bubbleController._dialogueData = newDialogueData;
		bubbleController.SwitchBubbleText (newDialogueData.dialogueText [0].text, true, _lerpTime);
		bubbleController.SetCharacter (newDialogueData.character);
		bubbleController.FadePointer (!newDialogueData.hidePointer);
		bubbleController.PointToTarget (newDialogueData.character, _lerpTime);
	}
	
	// Pass in a bubble to be moved to the PreviousBubblesParent (deep copy)
	private void SetPreviousBubble (GameObject bubble, float fadeOutTime = 0.0f)
	{
		if (PreviousBubblesParent.transform.childCount > 0)
		{
			GameObject previousBubble = PreviousBubblesParent.transform.GetChild (0).gameObject;
			if (previousBubble.activeSelf)
			{
				previousBubble.GetComponent<SubtitleBubble> ().Hide (true, fadeOutTime);
			}
			else
			{
				Destroy (previousBubble);
			}
		}
		
		GameObject refBubble;
		refBubble = Object.Instantiate (bubble) as GameObject;
		refBubble.transform.SetParent (PreviousBubblesParent.transform, false);
		refBubble.GetComponent<GenericBubble> ()._textData = bubble.GetComponent<GenericBubble> ()._textData;
		refBubble.GetComponent<SubtitleBubble> ()._dialogueData = bubble.GetComponent<SubtitleBubble> ()._dialogueData;
		
		refBubble.SetActive (false);
	}
	
//-Coroutines-----------------------------------------------------------------------------------------------------

	// End conversation routine
	private IEnumerator EndConversationRoutine ()
	{
		if (CurrentBubble != null && CurrentBubble.gameObject.activeSelf) CurrentBubble.Hide (true, _lerpTime);
		if (PreviousBubble != null && PreviousBubble.gameObject.activeSelf) PreviousBubble.Hide (true, _lerpTime);
		
		yield return new WaitForSeconds ((_lerpTime * 0.5f) * 3.0f);
		
		Destroy (gameObject);
	}
}
















