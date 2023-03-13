using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using CoreSystem;

public class ConversationController_ButtonsOnly : ConversationController {
	[Header ("No Bubbles Controller")] [Space(10)]

	// Bubbles prefabs
	public GameObject _bubbleActive;

	// On-screen learner bubble reference, null when none
	private SubtitleBubble_Active _currentActiveBubble = null;

	private const float _showTime = 0.7f;
	private const float _hideTime = 0.5f;

//-Components-Get/Set--------------------------------------------
	
	private bool _buttonsInteractable = false;
	public  bool ButtonsInteractable {
		get { return _buttonsInteractable; }
		set {
			_buttonsInteractable = value;
			ReplayButton.interactable = value;
			ForwardButton.interactable = value;
		} }

	private Transform _buttonsParent;
	private Transform ButtonsParent {
		get {
			if (_buttonsParent == null)
				_buttonsParent = transform.Find ("Buttons");
			return _buttonsParent;
		} }

	private Transform _bubblesParent;
	private Transform BubblesParent {
		get {
			if (_bubblesParent == null)
				_bubblesParent = transform.Find ("Bubbles");
			return _bubblesParent;
		} }
	
	private Button _replayButton;
	private Button ReplayButton {
		get {
			if (_replayButton == null)
				_replayButton = ButtonsParent.Find ("ReplayButton").GetComponent<Button> ();
			return _replayButton;
		} }

	private Button _forwardButton;
	private Button ForwardButton {
		get {
			if (_forwardButton == null)
				_forwardButton = ButtonsParent.Find ("ForwardButton").GetComponent<Button> ();
			return _forwardButton;
		} }


//-Init----------------------------------------------------------------------------

	public void Awake ()
	{
		ButtonsInteractable = false;

		// Set buttons initial position
		Vector3 pos = new Vector3 (0, -2 * ButtonsParent.transform.localPosition.y, 0);
		
		pos.x = ReplayButton.transform.localPosition.x;
		ReplayButton.transform.localPosition = pos;

		pos.x = ForwardButton.transform.localPosition.x;
		ForwardButton.transform.localPosition = pos;
	}

	public void Start ()
	{
		CoreEventSystem.Instance.AddListener (ConversationManager.Messages.GO_BACK_ONE_BUBBLE, OnReplayPreviousDialogue);
	}

	public new void OnDestroy ()
	{
		CoreEventSystem.Instance.RemoveListener (ConversationManager.Messages.GO_BACK_ONE_BUBBLE, OnReplayPreviousDialogue);
		base.OnDestroy ();
		StopAll ();
	}


//-Interface-----------------------------------------------------------------------

	public override void StartDialogue(DialogueEventData.DialogueData dialogueData)
	{
		base.StartDialogue(dialogueData);

		// Learner bubble
		if (dialogueData.isLearner)
		{
			Slide (false);
			SubtitleBubble_Active newBubble = NewLearnerBubble(dialogueData);
			newBubble.Show (_lerpTime);
			newBubble.SlideFromBottom (_lerpTime);
			newBubble.Buttons.BackButton.SetInteractableAfter (true, _lerpTime);
			_currentActiveBubble = newBubble;
		}
		// Buttons only
		else
		{
			Slide (true);
			if (!dialogueData.isStart)
			{
				ButtonsInteractable = true;
			}
		}
	}

	public override void EndDialogue(DialogueEventData.DialogueData dialogueData)
	{
		base.EndDialogue(dialogueData);

		ButtonsInteractable = false;

		if (_currentActiveBubble != null)
		{
			_currentActiveBubble.Hide (true, _lerpTime);
			_currentActiveBubble = null;
		}
	}

	public override void EndConversation()
	{
		base.EndConversation();

		Slide (false);
		if (_currentActiveBubble != null)
			_currentActiveBubble.Hide (true, _lerpTime);
		StartCoroutine (DestroyConversationAfter (_lerpTime + 0.1f));
	}


//-Buttons-------------------------------------------------------------------------
	
	public void ReplayOnClick ()
	{
		if (!ButtonsInteractable)
			return;

		ConversationManager.Instance.ReplayDialogue ();
		CoreHelper.PlayAudio (CoreHelper.SFX.SELECT, 0.7f);
	}

	public void ForwardOnClick ()
	{
		if (!ButtonsInteractable)
			return;

		ButtonsInteractable = false;
		ConversationManager.Instance.SkipDialogue ();
		CoreHelper.PlayAudio (CoreHelper.SFX.SELECT, 0.7f);
	}


//-Events--------------------------------------------------------------------------

	private void OnReplayPreviousDialogue (object param)
	{
		if (_currentActiveBubble != null)
			_currentActiveBubble.Hide (true, _lerpTime);
	}


//-Privates------------------------------------------------------------------------
	
	private bool _slideIn = false; // helper
	private void Slide (bool slideIn)
	{
		if (slideIn == _slideIn)
			return;
		_slideIn = slideIn;
		StopAll ();
		StartCoroutine (SlideConversationButtons (slideIn));
	}

	private void StopAll ()
	{
		StopAllCoroutines ();
		iTween.Stop (gameObject, true);
	}

	private SubtitleBubble_Active NewLearnerBubble (DialogueEventData.DialogueData dialogueData)
	{
		GameObject newBubble = Instantiate<GameObject> (_bubbleActive);
		newBubble.transform.SetParent (BubblesParent, false);

		SubtitleBubble_Active newBubbleSrc = newBubble.GetComponent<SubtitleBubble_Active> ();
		newBubbleSrc._isActive = true;
		// Add data
		newBubbleSrc._dialogueData = dialogueData;
		newBubbleSrc.SetTextData (dialogueData.dialogueText);
		// If model answer hasn't been set up in SetTextData, load default model answers
		if (dialogueData.isLearner && newBubbleSrc.Dropdowns.Help.GetAudioClip () == null)
			newBubbleSrc.Dropdowns.Help.SetAudioClip (GetDefaultModelAnswer (dialogueData));	
		
		return newBubbleSrc;
	}


//-Coroutines----------------------------------------------------------------------

	private IEnumerator SlideConversationButtons (bool slideIn)
	{
		yield return null;

		ButtonsInteractable = (slideIn) ? true : false;
		
		float moveTo_y = (slideIn) ? 0 : -2 * ButtonsParent.transform.localPosition.y;

		Hashtable hash = iTween.Hash ("y", moveTo_y, "time", (slideIn) ? _showTime : _hideTime, "islocal", true, "easetype", (slideIn) ? iTween.EaseType.easeOutQuart : iTween.EaseType.easeInQuart, "delay", 0f);

		iTween.MoveTo (ReplayButton.gameObject,  hash);
		hash ["delay"] = 0.07f;
		iTween.MoveTo (ForwardButton.gameObject, hash);

		//CoreHelper.PlayAudio ((slideIn) ? CoreHelper.SFX.UI_SLIDE_1 : CoreHelper.SFX.UI_SLIDE_2);
		
		//yield return new WaitForSeconds (((slideIn) ? _showTime : _hideTime) + 0.1f);
	}

	private IEnumerator DestroyConversationAfter (float time)
	{
		yield return new WaitForSeconds (time);
		Destroy (gameObject);
	}
}
