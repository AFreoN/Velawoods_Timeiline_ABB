using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using WellFired;
using UnityEngine.UI;

using CoreSystem;
using System.Text.RegularExpressions;

/// <summary>
/// A custom event that shows a speech bubble, plays a facial animation and audio file for a character at the given time
/// </summary>
public class DialogueEvent : TimelineBehaviour
{
    // Event properties
/*    protected DialogueEventData _data;
    public DialogueEventData Data
    {
        get
        {
            if (_data == null)
                _data = GetComponent<DialogueEventData>();
            if (_data == null)
                Debug.LogError("DialogueEvent: No Dialogue Data!");
            return _data;
        }
    }*/


    public DialogueEventData Data;
	// Relevant character FX data
	public enum FaceFXAudioCommand { Play, Pause, Stop, Resume }
	protected enum CharacterDataState { Stopped, Playing, Paused }
	protected struct CharacterData
	{
		[HideInInspector]
		public float delay;
		public Bones_FaceFXControllerScript_Setup faceFX;
		public AnimationClip animation;
		public AudioClip audioClip;
	}
	// Character(s) FX data, grabbed from _data.dialogueData
	protected List<CharacterData> _characterData = null;

	#region MonoBehaviours -------------------------------------------------------------------------------------------------------------------

	public void OnDestroy ()
	{
        RemoveListeners();
        StopAllCoroutines ();
    }

	#endregion

	#region SequencerEvents -------------------------------------------------------------------------------------------------------------------
	
	public void DataPlayback (FaceFXAudioCommand command)
	{
		FaceFXandAudio (command);
	}

    public override void OnClipStart(object o)
    {
		string s = Data.dialogueData.character ? Data.dialogueData.character.name : "Null";
		Debug.Log("Showing dialogue for : " + s);
		FireEvent();
    }

    public void FireEvent()
	{
		if (Data == null) return;
	
#if CLIENT_BUILD
        CoreEventSystem.Instance.AddListener (MainMenu.Messages.MENU_SHOWING, PauseEvent);
		CoreEventSystem.Instance.AddListener (MainMenu.Messages.MENU_HIDING, ResumeEvent);
#endif
        CoreEventSystem.Instance.AddListener(CoreEventTypes.ACTIVITY_REVERSE, OnActivityChange);
        CoreEventSystem.Instance.AddListener(CoreEventTypes.ACTIVITY_SKIP, OnActivityChange);
        CoreEventSystem.Instance.AddListener(ConversationManager.Messages.GO_BACK_ONE_BUBBLE, RemoveListeners);
		
		ParseData (); // Runtime parses
		LoadAnimationAndAudio (); // Load data
		
		// Check type
        if (Data.dialogueData.type != DialogueEventData.DialogueType.None)
        {
			// Queue conversation event
            ConversationManager.Instance.QueueDialogue (this);
        }
        else
        {
            DataPlayback(FaceFXAudioCommand.Play);
        }
	}

    public override void OnClipEnd(object o)
    {
		EndEvent();
    }

    public void EndEvent()
	{
		//if(!AffectedObject)
		//	return;
		
		if (Data.dialogueData.type != DialogueEventData.DialogueType.None)
		{
			if(Data != null) {
				// if not a carnegie bubble
				if (! Data.dialogueData.isLearner)
				{
					TimelineController.instance.PauseTimeline(); // pause timeline, 
					StartTimer (); // start timer (which will resume timeline on complete)
				}
				else
				{
					// If editing, check if we're skipping the learner dialouge. If so, just do a normal pause. Otherwise, just continue.
	#if UNITY_EDITOR && !CLIENT_BUILD
					if (ConversationManager.Instance._skipLearnerDialogue)
					{
						TimelineController.instance.PauseTimeline(); // pause timeline, 
						StartTimer (); // start timer (which will resume timeline on complete)
					}
					else
					{
						ConversationManager.Instance.EndDialogue (Data.dialogueData);
					}
	#else
					ConversationManager.Instance.EndDialogue (Data.dialogueData);
	#endif
				}
			}
			else {
				// warn that this event has no data!
				Debug.LogError("DialogueEvent has no data");
			}
		}
    }

	/// <summary>
	/// Skips the event. | Offset's default value set to -0.01f because sometimes, when an event is skipped to the very end, EndEvent () is not called anymore. 
	/// | Returning the updated running time of the sequence here because it's updated at a later frame. </summary>
	public virtual void SkipEvent ()
	{
		FaceFXandAudio (FaceFXAudioCommand.Stop);
		
        StopTimer();

        float skipTo = startTime + (endTime - startTime) + 0.01f;
        //SequenceManager.Instance.SkipTo(Sequence, skipTo, true);
		TimelineController.instance.SkipTimeline(skipTo);
        RemoveListeners();

        // Cause the animators to update so that the last target is set in the IK script. 
  //      foreach (Animator a in GameObject.FindObjectsOfType<Animator>())
  //      {
		//	a.Update(0.01f);
		//} 

		ConversationManager.Instance.EndDialogue(Data.dialogueData);
		RemoveListeners();
	}
	
	public virtual void ReplayEvent ()
	{
		FaceFXandAudio (FaceFXAudioCommand.Stop);
        //SequenceManager.Instance.SkipTo(Sequence, FireTime - 0.01f, true);
		TimelineController.instance.SkipTimeline(startTime - 0.01f, true);
		//if (TimelineController.isPlaying == false)
		//	TimelineController.instance.PlayTimeline ();
		StopTimer ();
        RemoveListeners();
	}
	
	public void PauseEvent(object pauseParams = null)
	{
		if (_timerState == TimerState.Playing)
			PauseTimer ();
		FaceFXandAudio (FaceFXAudioCommand.Pause);
	}

	public void ResumeEvent(object resumeParams = null)
	{
		try {
			if ((bool)resumeParams == false)
				return;
		} catch (System.Exception e) 
        {
            Debug.LogError(e);
        }

		FaceFXandAudio (FaceFXAudioCommand.Resume);
		if (_timerState == TimerState.Paused)
			ResumeTimer ();
	}

    public override void OnReset()
    {
		Reset();
    }
    // Called once when skip is being pressed
    public void Reset() {}

    public override void OnSkip()
    {
		Skip();
    }
    // Called when skipped to / over this event. To actually skip this event, use SkipEvent()
    public void Skip ()
	{
		// If scrubber is inside this event
		//if ((Sequence.RunningTime < (FireTime + Duration)) && (Sequence.RunningTime > FireTime)) {
		//	FireEvent (); // Play event
		//}
	}

	private void OnTimerDone ()
	{
		TimelineController.instance.PlayTimeline();
		ConversationManager.Instance.EndDialogue(Data.dialogueData);
		RemoveListeners();
        _timerRoutine = null;
    }

#endregion

#region Events -------------------------------------------------------------------------------------------------------------------------

	private void OnActivityChange(object parameters = null)
	{
		if (_timerState == TimerState.Playing)
		{
			StopTimer ();
		}
		RemoveListeners ();
	}

	private void RemoveListeners(object parameters = null)
    {
#if CLIENT_BUILD
        CoreEventSystem.Instance.RemoveListener(MainMenu.Messages.MENU_SHOWING, PauseEvent);
        CoreEventSystem.Instance.RemoveListener(MainMenu.Messages.MENU_HIDING, ResumeEvent);
#endif
        CoreEventSystem.Instance.RemoveListener(CoreEventTypes.ACTIVITY_REVERSE, RemoveListeners);
        CoreEventSystem.Instance.RemoveListener(CoreEventTypes.ACTIVITY_SKIP, RemoveListeners);
        CoreEventSystem.Instance.RemoveListener(ConversationManager.Messages.GO_BACK_ONE_BUBBLE, RemoveListeners);
    }

#endregion

#region FaceFX and Audio Interface -----------------------------------------------------------------------------------------------------

	protected virtual void LoadAnimationAndAudio ()
	{
		_characterData = new List<CharacterData> ();
		
		// No of potential FaceFX anims. Main character plus extra characters.
		int fcsCount = 1 + ((Data.dialogueData.ExtraCharacters != null) ? Data.dialogueData.ExtraCharacters.Count : 0);
		
		// Populate _characterData with character(s) properties
		_characterData.Clear ();
		for (int count=0; count<fcsCount; count++)
		{
			GameObject characterObj;
			CharacterData characterData = new CharacterData ();
			
			// Main character
			if (count==0)
			{
#if CLIENT_BUILD
				if (Data.dialogueData.isTutorAudioClip)
				{
					AudioClip mainAudio = DialogueAudioHelper.GetDialogueAudio (Data.dialogueData.audioClip.name);
                    if (mainAudio != null) Data.dialogueData.audioClip = mainAudio;
                    else Debug.LogWarning("Dialogue Event Data : Could not find tutor audio! Leaving it as is...");
				}
#endif

				characterObj = Data.dialogueData.character;
				characterData.animation = Data.dialogueData.animationClip;
				characterData.audioClip = Data.dialogueData.audioClip;
				characterData.delay = 0;
			}
			// Extra character(s)
			else
			{
				characterObj = Data.dialogueData.ExtraCharacters [count-1].character;
				characterData.animation = Data.dialogueData.ExtraCharacters [count-1].animationClip;
				characterData.audioClip = Data.dialogueData.ExtraCharacters [count-1].audioClip;
				characterData.delay     = Mathf.Max (0, Data.dialogueData.ExtraCharacters [count-1].delay);
			}
			
			// Check character & faceFX
			if (characterObj)
			{
				characterData.faceFX = characterObj.GetComponent<Bones_FaceFXControllerScript_Setup> ();
				//if (characterData.faceFX == null)
				//	Log ("Dialogue Event : No FaceFXControllerScript_Bones script found on " + ((count==0) ? "Main" : "Extra") + "character: " + characterObj.name);
			}
			else
			{
				//Log ("Dialogue Event : " + ((count==0) ? "Main" : "Extra") + " Character field is null.");
			}
			  
			// Check animation
			//if (characterData.animation == null)
			//	Log ("Dialogue Event : " + ((count==0) ? "Main" : "Extra") + " Animation field is null.");
			
			// Check audio
			//if (characterData.audioClip == null)
			//	Log ("Dialogue Event : " + ((count==0) ? "Main" : "Extra") + " Audio field is null.");
			
			// Add to FX list
			_characterData.Add (characterData);
		}
	}
	
	protected void FaceFXandAudio (FaceFXAudioCommand command)
	{
		if (Data.dialogueData.isLearner)
			return;

		for (int i=0; i<_characterData.Count; i++)
		{
			// Difference in time between characterData's start time (firetime+delay) and the sequence's running time (scrub bar position)
			float timeOffset = startTime + _characterData [i].delay - (float)TimelineController.instance.currentPlayableTime;

			if (Mathf.Abs (timeOffset) < 0.075f)
				timeOffset = 0;
				
			// Always play first one (main) with 0 offset
			if (i==0) 
				timeOffset = 0;
			
			switch (command)
			{
			case FaceFXAudioCommand.Play:   
				if (timeOffset >= 0) // Play if in front of scrubber, with their respective delays
					TriggerFX (_characterData [i], command, timeOffset);
				break;
				
			case FaceFXAudioCommand.Pause:  
				StopCoroutine ("TriggerDelayedFX"); // Stop delayed FX
				// If scrubber inside _characterData[i] event, pause
				if (_characterData[i].animation != null && timeOffset <= 0 && Mathf.Abs (timeOffset) < _characterData [i].animation.length)
					TriggerFX (_characterData [i], command, 0);
				break;
				
			case FaceFXAudioCommand.Resume: 
				// If scrubber inside _characterData[i] event, resume
                if (_characterData[i].animation != null && timeOffset <= 0 && Mathf.Abs(timeOffset) < _characterData[i].animation.length)
					TriggerFX (_characterData [i], command, 0);
				else
					// Else if _characterData[i] event is to be played, start delayed play process
					if (timeOffset > 0)
						TriggerFX (_characterData [i], FaceFXAudioCommand.Play, timeOffset);
				break;
				
			case FaceFXAudioCommand.Stop:   
				StopCoroutine ("TriggerDelayedFX");
				TriggerFX (_characterData [i], command, 0);
				break;
			}
		}
	}

	protected void ParseData ()
	{
		// If there are no model answers in the dialogue texts, but there are in the dialogue event data, assign the latter to the dialogue texts
		if (Data.dialogueData.dialogueText != null) {
			for (int i=0; i<Data.dialogueData.dialogueText.Count; i++) {
				DialogueEventData.DialogueText temp = Data.dialogueData.dialogueText [i];
				if (Data.dialogueData.tutorAudioClips.angela != null && temp.tutorAudioClips.female == null)
					temp.tutorAudioClips.female = Data.dialogueData.tutorAudioClips.angela;
				if (Data.dialogueData.tutorAudioClips.jack != null && temp.tutorAudioClips.male == null)
					temp.tutorAudioClips.male = Data.dialogueData.tutorAudioClips.jack;
				Data.dialogueData.dialogueText [i] = temp;
			}
		}

		// Set as 'correct' if only one dialogue text present
		if (Data.dialogueData.dialogueText != null && Data.dialogueData.dialogueText.Count == 1 && !Data.dialogueData.dialogueText [0].isCorrect)
		{
			DialogueEventData.DialogueText temp = Data.dialogueData.dialogueText [0];
			temp.isCorrect = true;
			Data.dialogueData.dialogueText [0] = temp;
		}
	}

#endregion

#region Internal FX Playback ----------------------------------------------------------------------------------------------------------------------------

	protected void TriggerFX (CharacterData animData, FaceFXAudioCommand command, float delay)
	{
		if (delay > 0)
		{
			StartCoroutine ("TriggerDelayedFX", new object[] {animData, command, delay});
			return;
		}

		if (animData.faceFX && animData.animation && animData.audioClip) {
			switch (command) {
				case FaceFXAudioCommand.Play:   animData.faceFX.PlayAnim   (animData.animation.name, animData.audioClip); break;
				case FaceFXAudioCommand.Pause:  animData.faceFX.PauseAnim  (); break;
				case FaceFXAudioCommand.Resume: animData.faceFX.ResumeAnim (); break;
				case FaceFXAudioCommand.Stop:   animData.faceFX.StopAnim   (); break;
			}
		}
		else {
			if (animData.audioClip) {
				switch (command) {
					case FaceFXAudioCommand.Play:   AudioManager.Instance.PlayAudio   (animData.audioClip, CoreSystem.AudioType.Dialogue); break;
					case FaceFXAudioCommand.Pause:  AudioManager.Instance.PauseAudio  (CoreSystem.AudioType.Dialogue); break;
					case FaceFXAudioCommand.Resume: AudioManager.Instance.ResumeAudio (CoreSystem.AudioType.Dialogue); break;
					case FaceFXAudioCommand.Stop:   AudioManager.Instance.StopAudio   (CoreSystem.AudioType.Dialogue); break;
				}
			}
		}	
	}
	
	protected IEnumerator TriggerDelayedFX (object[] args)
	{	
		CharacterData animData = (CharacterData)args[0];
		FaceFXAudioCommand command = (FaceFXAudioCommand)args[1];
		float delay = (float)args[2];
		
		if (delay > 0)
			yield return new WaitForSeconds (delay);
			
		TriggerFX (animData, command, -1);
	}

#endregion

#region Editor -----------------------------------------------------------------------------------------------------------------------------------------------

	// WARNING : Don't modify inspector (public) values here unless you really need to and the mission-building guys are ok with it
	public void Update()
	{
   //     if (Input.GetKeyUp(KeyCode.S))
   //     {
			//Debug.Log("Dialogue event input received");
			//OnClipStart(this);
   //     }
		// grab the Dialogue Event Data component, and if it's not there, bail out
		/*if (Data == null) 
			return;
				
		// Set comments on the timeline's UI
		this.comment = "";
		if (Data.dialogueData.isLearner) {
			this.comment = "Learner";
		} else {
			if (Data.dialogueData.dialogueText != null && Data.dialogueData.dialogueText.Count > 0)
				this.comment = Data.dialogueData.dialogueText [0].text + "\n";
			if (Data.dialogueData.character != null) {
				string name = Data.dialogueData.character.name;
				if ( name != null ) {
					if (name.Contains ("PRFB_")) {
						name = name.Replace ("PRFB_", "");
						this.comment += name.Contains ("_") ? name.Substring (name.IndexOf ("_") + 1) : name;
					} else
						this.comment += name;
				}
			}	
		}

		// Set length of event according to the longest character faceFX/audioClip duration, plus the delay
		Duration = 0.5f; // min 0.5
		int characterDataCount = (Data.dialogueData.ExtraCharacters == null) ? 1 : Data.dialogueData.ExtraCharacters.Count + 1;
		for (int i=0; i<characterDataCount; i++)
		{
			float animLength = 0, audioLength = 0, delay = 0;
			
			if (i==0) // Main character
			{
				animLength = (Data.dialogueData.animationClip) ? Data.dialogueData.animationClip.length : 0;
				audioLength = (Data.dialogueData.audioClip) ? Data.dialogueData.audioClip.length : 0;
				delay = 0;
			}
			else if ( Data.dialogueData.ExtraCharacters != null && Data.dialogueData.ExtraCharacters.Count > i )  // Extra character(s)
			{	
				animLength = (Data.dialogueData.ExtraCharacters [i-1].animationClip) ? Data.dialogueData.ExtraCharacters [i-1].animationClip.length : 0;
				audioLength = (Data.dialogueData.ExtraCharacters [i-1].audioClip) ? Data.dialogueData.ExtraCharacters [i-1].audioClip.length : 0;
				delay = Mathf.Max (0, Data.dialogueData.ExtraCharacters [i-1].delay);
			}

			if (Duration < animLength + delay) 
				Duration = animLength + delay;
            if (Duration < audioLength + delay)
                Duration = audioLength + delay;
		}*/
	}

	public void UpdateData(DialogueEventData.DialogueData data)
    {
        for (int index = 0; index < Data.dialogueData.dialogueText.Count; ++index)
        {
            if(index < data.dialogueText.Count)
            {
                if (Data.dialogueData.dialogueText[index].carnegieOriginalText != data.dialogueText[index].carnegieOriginalText) {
                    Debug.Log("Updated carnegieOriginalText");
                }
                if (Data.dialogueData.dialogueText[index].carnegieText.Length == data.dialogueText[index].carnegieText.Length) {
                    for(int textIndex = 0; textIndex < data.dialogueText[index].carnegieText.Length; ++textIndex) {
                        if(Data.dialogueData.dialogueText[index].carnegieText[textIndex] != data.dialogueText[index].carnegieText[textIndex]) {
                            Debug.Log("Updated carnegieText at index: " + textIndex);
                        }
                    }
                }
                else {
                    Debug.Log("There is not the same amount of carnegie text values. Updated to be the same as the database.");
                }
                if (Data.dialogueData.dialogueText[index].isCorrect != data.dialogueText[index].isCorrect) {
                    Debug.Log("Updated isCorrect");
                }
                if (Data.dialogueData.dialogueText[index].isReferenceAnswer != data.dialogueText[index].isReferenceAnswer) {
                    Debug.Log("Updated isReferenceAnswer");
                }
                if (Data.dialogueData.dialogueText[index].text != data.dialogueText[index].text) {
                    Debug.Log("Updated text");
                }
            }
        }
        Data.dialogueData.dialogueText = data.dialogueText;
        Data.dialogueData.isLearner = data.isLearner;

        Data.dialogueData.character = data.character;
        Data.dialogueData.characterIllustration = data.characterIllustration;
        Data.dialogueData.animationClip = data.animationClip;
        Data.dialogueData.audioClip = data.audioClip;
        Data.dialogueData.isTutorAudioClip = data.isTutorAudioClip;
        
        //Data.SaveState();
    }
	
	private void Log (string message)
	{
		Debug.LogWarning (message);
	}

#endregion

	/// <summary> General timing for replay delay.</summary>
	public const float _replayDelay = 0.0f;
	
	public enum TimerState { Stopped, Paused, Playing }
	[HideInInspector] public TimerState _timerState = TimerState.Stopped;

    private Coroutine _timerRoutine;

    /// <summary> Called by the ConversationManager. Handle which bubble should start the timer here. </summary>
    public virtual void StartTimer (float time = _replayDelay)
	{
		_timerState = TimerState.Playing;

        if (_timerRoutine != null)
        {
            StopCoroutine(_timerRoutine);
        }
        _timerRoutine = StartCoroutine(TimerSequence(time));
	}

	public virtual void StopTimer ()
	{
		_timerState = TimerState.Stopped;

        if (_timerRoutine != null)
        {
            StopCoroutine(_timerRoutine);
        }
        _timerRoutine = null;
    }

	public virtual void PauseTimer ()
	{
		_timerState = TimerState.Paused;
	}

	public virtual void ResumeTimer ()
	{
		_timerState = TimerState.Playing;
	}

	private IEnumerator TimerSequence (float time)
	{
		float currentTime = 0;
		while (currentTime < time)
		{
            while (_timerState == TimerState.Paused)
            {
                yield return null;
            }

			currentTime += Time.deltaTime;
			yield return null;
		}
		_timerState = TimerState.Stopped;
		
		OnTimerDone ();
	}
}