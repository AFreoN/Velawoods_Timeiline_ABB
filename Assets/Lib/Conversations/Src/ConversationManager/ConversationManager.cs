using UnityEngine;
using System.Collections;

using CoreLib;
using System.Text.RegularExpressions;

public class ConversationManager : MonoSingleton<ConversationManager> {
	
	// Conversation Controller 
	public GameObject _conversation;
	public ConversationController _conversationScript {
		get {
			if (_conversation == null) return null;
			return _conversation.GetComponent<ConversationController> ();
		} }

	protected DialogueEvent _queuedDialogueEvent   = null;
	protected DialogueEvent _currentDialogueEvent  = null;
	protected DialogueEvent _previousDialogueEvent = null;
	
	// Messaging (used mainly for the scoring system)
	public struct Messages
	{
		public const string NEW_BUBBLE = "NewBubble";
		public const string END_BUBBLE = "EndBubble";
		public const string GO_BACK_ONE_BUBBLE = "GoBackOneDialogue";
		public const string CONVERSATION_START = "ConversationStart";
		public const string CONVERSATION_END = "ConversationEnd";	
	}
	
#if UNITY_EDITOR && !CLIENT_BUILD
	// EDITOR Feature: Don't pause for learner dialogues
	public bool _skipLearnerDialogue = false;
#endif
//-MonoSingleton---------------------------------------------------------------------------------------------------------------------------------------
	
	protected override void Init ()
	{
        base.Init();
        CoreEventSystem.Instance.AddListener (CoreEventTypes.ACTIVITY_SKIP, OnActivitySkip);
		CoreEventSystem.Instance.AddListener (CoreEventTypes.ACTIVITY_REVERSE, OnActivityReverse);
		CoreEventSystem.Instance.AddListener (CoreEventTypes.LEVEL_CHANGE, OnLevelChange);
		CoreEventSystem.Instance.AddListener (CoreEventTypes.SEQUENCE_EDITING_SKIP, SequenceEditingSkip);
	}

	protected override void Dispose()
	{
		base.Dispose();
        CoreEventSystem.Instance.RemoveListener (CoreEventTypes.ACTIVITY_SKIP, OnActivitySkip);
		CoreEventSystem.Instance.RemoveListener (CoreEventTypes.ACTIVITY_REVERSE, OnActivityReverse);
		CoreEventSystem.Instance.RemoveListener (CoreEventTypes.LEVEL_CHANGE, OnLevelChange);
		CoreEventSystem.Instance.RemoveListener (CoreEventTypes.SEQUENCE_EDITING_SKIP, SequenceEditingSkip);
	}


//-Playback Interface---------------------------------------------------------------------------------------------------------------------------------------

	// Add an event to the queue.
	public void QueueDialogue(DialogueEvent dialogueEvent)
	{
		// Check
		if (_queuedDialogueEvent != null)
			Debug.LogError("ConversationManager : Ooops, we already have a queued event!");
		
		// Assign
		_queuedDialogueEvent = dialogueEvent;

		// If none currently playing, play this
		if (_currentDialogueEvent == null)
		{
			_queuedDialogueEvent = null; // Discard
			PlayDialogue (dialogueEvent);
		}
		else
		{
			//Debug.Log("Queueing Dialogue :" + dialogueEvent.Data.dialogueData.dialogueText[0].text);
		}
	}
	
	private void PlayDialogue (DialogueEvent dialogueEvent)
	{
        if (dialogueEvent.Data.dialogueData.isStart)
		{
			// Fade down ambient sounds
			AmbientSoundManager soundManager = GameObject.FindObjectOfType<AmbientSoundManager>();
			if(soundManager)
				soundManager.FadeVolumeDown();
			
			_previousDialogueEvent = null;
			_currentDialogueEvent = null;
		}

		if (_conversation == null)
			LoadConversationController (dialogueEvent.Data.dialogueData.type);

		// Init
		_currentDialogueEvent = dialogueEvent;
		
		// Play
		_currentDialogueEvent.DataPlayback (DialogueEvent.FaceFXAudioCommand.Play);
		_conversationScript.StartDialogue (dialogueEvent.Data.dialogueData);
		
		// Check if learner dialogue
        if (_currentDialogueEvent.Data.dialogueData.isLearner)
        {
#if UNITY_EDITOR && !CLIENT_BUILD
			// Check editor flag
			if (!_skipLearnerDialogue)
            {
#endif
                PauseConversationSequence();
#if UNITY_EDITOR && !CLIENT_BUILD
            }
#endif
        }
		
		// Send events
		if (dialogueEvent.Data.dialogueData.isStart) 
			CoreEventSystem.Instance.SendEvent (Messages.CONVERSATION_START, dialogueEvent.Data.dialogueData);
		CoreEventSystem.Instance.SendEvent (Messages.NEW_BUBBLE, dialogueEvent.Data.dialogueData);
    }	
	
	public void EndDialogue (DialogueEventData.DialogueData dialogueData)
	{
		if (_conversation == null) 
			return;
		
		if (dialogueData.isEnd)
		{
			CoreEventSystem.Instance.SendEvent (Messages.END_BUBBLE, dialogueData);
			EndConversation ();
		}
		else
		{
			// End conversation dialogue
			_conversationScript.EndDialogue (dialogueData);

			_previousDialogueEvent = _currentDialogueEvent;
			_currentDialogueEvent = null;

			// If there are any queued events waiting..
			if (_queuedDialogueEvent != null) {
				PlayDialogue (_queuedDialogueEvent); // Play
				_queuedDialogueEvent = null; // Discard
			}
		}
	}	
	
	public void SkipDialogue ()
	{
		if (_currentDialogueEvent)
		{
			_currentDialogueEvent.SkipEvent ();
		}
		else {
			if (Debug.isDebugBuild) 
				Debug.Log ("ConversationManager: SkipDialogue: Dialogue event not present!");
		}
	}
	
	public void ReplayDialogue ()
	{
		if (_currentDialogueEvent)
		{
			_currentDialogueEvent.ReplayEvent ();
			_currentDialogueEvent = null;
			_queuedDialogueEvent = null;
		}
		else {
			if (Debug.isDebugBuild) 
				Debug.Log ("ConversationManager: ReplayDialogue: Dialogue event not present!");
		}
	}
	
	public void ReplayPreviousDialogue ()
	{
		if (_previousDialogueEvent != null)
		{
			if (_currentDialogueEvent && _currentDialogueEvent.Data.dialogueData.isLearner == false)
				_currentDialogueEvent.DataPlayback (DialogueEvent.FaceFXAudioCommand.Stop);
			
			_previousDialogueEvent.ReplayEvent ();
			_currentDialogueEvent = null;
			_previousDialogueEvent = null;
			_queuedDialogueEvent = null;
			CoreEventSystem.Instance.SendEvent (Messages.GO_BACK_ONE_BUBBLE);
		}
		else {
			if (Debug.isDebugBuild) 
				Debug.Log ("ConversationManager: ReplayDialogue: Previous dialogue event not present!");
		}
	}

	public void EndConversation (bool isSkipping = false)
	{
		_previousDialogueEvent = null;
		_currentDialogueEvent = null;
		_queuedDialogueEvent = null;

		if(_conversation) {
			if(_conversationScript)
			{
				_conversationScript.EndConversation ();

                if (false == isSkipping)
                {
                    AmbientSoundManager soundManager = GameObject.FindObjectOfType<AmbientSoundManager>();

                    if (soundManager)
                    {
                        Debug.Log("Ambient Audio Fading Up");
                        soundManager.FadeVolumeUp();
                    }
                }
			}
			_conversation = null;
			
			CoreEventSystem.Instance.SendEvent (Messages.CONVERSATION_END);
		}
	}


//-Local Sequence Handlers---------------------------------------------------------------------------------------------------------------------------------
	
	public void PauseConversationSequence ()
	{
		if (_currentDialogueEvent) {
			TimelineController.instance.PauseTimeline();
		}
		else {
			if (Debug.isDebugBuild) {
				Debug.Log ("ConversationManager: SkipDialogue: Dialogue event not present!");
			}
		}
	}
	
	public void ResumeConversationSequence ()
	{
		if (_currentDialogueEvent) {
			TimelineController.instance.PlayTimeline ();
		}
		else {
			if (Debug.isDebugBuild) {
				Debug.Log ("ConversationManager: SkipDialogue: Dialogue event not present!");
			}
		}
	}

	public void OnCarnegieSuccess ()
	{
		if (_currentDialogueEvent) {
			ResumeConversationSequence ();
			//EndDialogue (_currentDialogueEvent.Data.dialogueData);
		}
		else {
			if (Debug.isDebugBuild) {
				Debug.Log ("ConversationManager: SkipDialogue: Dialogue event not present!");
			}
		}
	}
	
	
//-Privates & Protected--------------------------------------------------------------------------------------------------------------------------------------

	protected void LoadConversationController (DialogueEventData.DialogueType type)
	{
		string prefabTag;
		
		switch (type)
		{
			case DialogueEventData.DialogueType.Subtitles:   prefabTag = "Subtitles";   break;
			case DialogueEventData.DialogueType.List:	     prefabTag = "List";		break;
			case DialogueEventData.DialogueType.ButtonsOnly: prefabTag = "ButtonsOnly"; break;	
			//
			default: prefabTag = "ButtonsOnly"; break;
		}
		
		_conversation = Resources.Load ("Prefabs/ConversationControllers/ConversationController_" + prefabTag) as GameObject;
		_conversation = Object.Instantiate (_conversation, _conversation.transform.position, _conversation.transform.rotation) as GameObject;
		LayerSystem.Instance.AttachToLayer ("Bubbles", _conversation);
	}
	

//-Events-------------------------------------------------------------------------------------------------------------------------------------------------

	private void SequenceEditingSkip (object parameters)
	{
		EndConversation ();
	}

	private void OnActivitySkip (object parameters) {
		EndConversation (true);
	}
	
	private void OnActivityReverse (object parameters) {
		EndConversation ();
	}
	
	private void OnLevelChange (object parameters) {
		EndConversation ();
	}
}
