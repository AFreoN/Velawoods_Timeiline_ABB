using UnityEngine;
using System.Collections;

public class ConversationController_List : ConversationController {
	[Header ("List Controller")] [Space(10)]
	public float _listSpacing = 15;
	
	[Header ("Bubbles' Prefabs")]
	public GameObject _bubblePassive;
	public GameObject _bubbleActive;


	// Height of the list, updated when a bubble is added
	protected float _contentHeight = 0;
	
	// Parent of all bubbles
	private GameObject _listContent;
	protected GameObject Content {
		get {
			if (_listContent == null)
			{
				_listContent = transform.GetChild (0).GetChild (0).GetChild (0).gameObject;
			}
			return _listContent;
		}
	}
	
	// Current bubble's dialogue data
	private DialogueEventData.DialogueData _currentDialogueData = new DialogueEventData.DialogueData ();
	protected DialogueEventData.DialogueData CurrentDialogueData {
		get {
			return _currentDialogueData;
		}
		set {
			_currentDialogueData = value;
		}
	}
	
	// Current on-screen bubble
	protected ListBubble CurrentBubble {
		get {
			if (Content.transform.childCount > 0)
			{
				return Content.transform.GetChild (Content.transform.childCount - 1).GetComponent<ListBubble> ();
			}
			Debug.Log ("ConversationController_List: No Bubbles Present!");
			return null;
		}
	}
	
	
//-Interface-----------------------------------------------------------------------
	
	// Start of a dialogue bubble
	public override void StartDialogue (DialogueEventData.DialogueData dialogueData)
	{
		base.StartDialogue (dialogueData);
		
		// If previous bubble is available
		if (CurrentBubble != null)
		{
			// Avoid adding new bubble when the replay button is pressed if the same bubble is called 
			
			// Previously commented out by someone (?), but it messes things up in the case of replaying a bubble.
			if (CurrentDialogueData.dialogueText [0].text == dialogueData.dialogueText [0].text && CurrentDialogueData.character == dialogueData.character && CurrentDialogueData.characterIllustration == dialogueData.characterIllustration) {
				return;
			}
			
			// Fade previous bubble's buttons out if fadeOnEnd is unticked for it
			if (CurrentDialogueData.fadeOnEnd == false && CurrentDialogueData.isLearner == false)
			{
				CurrentBubble.FadeOutButtons (_lerpTime);
			}
		}
		
		// Add to list
		AddBubble (NewBubble (dialogueData));
		CurrentDialogueData = dialogueData;
	}
	
	// End of a dialouge bubble
	public override void EndDialogue (DialogueEventData.DialogueData dialogueData)
	{
		base.EndDialogue (dialogueData);
		
		// Fade out buttons if fade on end is ticked
		if (dialogueData.fadeOnEnd && CurrentDialogueData.isLearner == false)
		{
			CurrentBubble.FadeOutButtons (_lerpTime);
		}
	}
	
	// End conversatin, hide bubbles and destroy controller
	public override void EndConversation ()
	{
		base.EndConversation ();
		
		StartCoroutine (HideDialoguesRoutine ());
	}

	
//-Privates & Protected------------------------------------------------------------
	
	// Add argument bubble to the on-screen list
	protected void AddBubble (GameObject bubbleObj)
	{
		ListBubble bubbleSrc = bubbleObj.GetComponent<ListBubble> ();
		
		// Get heights
		Vector4 bounds = bubbleSrc.GetBounds ();
		float bubbleHeight = Mathf.Abs (bounds.w - bounds.z);
	
		float bubbleCount = Content.transform.childCount;
		// If other bubbles are present
		if (bubbleCount > 1)
		{
			// Slide already available bubbles up
			for (int i=0; i<bubbleCount - 1; i++)
			{
				Content.transform.GetChild (i).GetComponent<ListBubble> ().SlideVerticallyBy ((bubbleHeight + _listSpacing) / 2.0f, _lerpTime);
			}
			// Set new bubble's offset
			bubbleSrc.SlideVerticallyBy (-1 * ((_contentHeight + _listSpacing) / 2.0f));
		}
		else
		{
			// Manage initial offset
			_contentHeight -= _listSpacing / 2.0f;
		}
		
		// Slide new bubble from bottom
		bubbleSrc.SlideFromBottom (_lerpTime, -1 * Content.transform.localPosition.y );
		bubbleSrc.Hide ();
		bubbleSrc.Show (_lerpTime);
		
		// Update list's total height
		_contentHeight += bubbleHeight + _listSpacing;
		
		float listContentHeight = Content.GetComponent<RectTransform> ().rect.height;
		if (_contentHeight > listContentHeight)
		{
			Content.GetComponent<RectTransform> ().SetSizeWithCurrentAnchors (RectTransform.Axis.Vertical, _contentHeight);
			iTween.MoveTo(Content, iTween.Hash("y", Content.transform.localPosition.y + (_contentHeight - listContentHeight) / 2.0f, "easeType", "easeOutSine", "islocal", true, "time", _lerpTime));
		}
	}
	
	// Create new bubble
	protected GameObject NewBubble (DialogueEventData.DialogueData dialogueData)
	{
		GameObject newBubble;
		
		// Assign prefab
		GameObject prototype;
		if (dialogueData.isLearner)
		{
			prototype = _bubbleActive;
		}
		else
		{
			prototype = _bubblePassive;
		}
		
		// Add to scene
		newBubble = Object.Instantiate (prototype) as GameObject;
		newBubble.transform.SetParent (Content.transform, false);
		
		ListBubble newBubbleSrc = newBubble.GetComponent<ListBubble> ();
		
		// Add data
		newBubbleSrc.SetTextData (dialogueData.dialogueText);
		// If model answer hasn't been set by the bubble (aka. all DialogueTexts don't have model answers), set the default one (soon to be obsolete)
		if (dialogueData.isLearner && newBubbleSrc.Dropdowns.Help.GetAudioClip () == null)
		{
			newBubbleSrc.Dropdowns.Help.SetAudioClip (GetDefaultModelAnswer (dialogueData));
		}
		
		
		if (dialogueData.isLearner == false && dialogueData.characterIllustration != null && dialogueData.characterIllustration.Length > 0)
		{
			if (newBubbleSrc.Buttons.transform.Find ("LeftSide").childCount == 0)
			{
				GameObject characterCircle = Resources.Load<GameObject> ("GenericMisc_CharacterCircle");
				if (characterCircle == null)
					Debug.LogWarning ("ConversationController_List: GenericMisc_CharacterCircle prefab not found");
				else
				{
					characterCircle = Instantiate<GameObject> (characterCircle);
					characterCircle.transform.SetParent (newBubbleSrc.Buttons.transform.Find ("LeftSide"), false);
					characterCircle.GetComponent<GenericMisc_CharacterCircle> ().CharacterImageName = dialogueData.characterIllustration;
				}
			}
		}
		
		// Return
		return newBubble;
	}
	
	
//-Coroutines-----------------------------------------------------------------------------------
	
	// Hide bubbles and destroy controller
	IEnumerator HideDialoguesRoutine ()
	{
		float waitForSeconds = (Content.transform.childCount > 7) ? 0.0f : 0.2f;
	
		for (int i=0; i<Content.transform.childCount; i++)
		{
			Content.transform.GetChild (i).GetComponent<ListBubble> ().Hide (false, _lerpTime);
			
			yield return new WaitForSeconds (waitForSeconds);
		}
		
		yield return new WaitForSeconds ((_lerpTime * 0.5f) * 3.0f);
		
		Destroy (gameObject);
	}
}













































