using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class DialogueEventData  {
	
	// Main data box
	[System.Serializable]
	public struct DialogueData
	{
        [Space(10)]
        [Header("Validation Data")]
        [Tooltip("Does the dialogue event require a Face FX animation.")]
        public bool FaceFXNotNeeded;
        [Tooltip("Does the dialogue event require to reference a character.")]
        public bool CharacterNotNeeded;


		[Space (10)]
		[Header ("Dialogue Data")] 
		
		[Tooltip ("ID for debugging purposes")]
		public int dialogueID;
		[Tooltip ("The 3D character speaking (if any)")]
		public GameObject character;
		[Tooltip("The 2D character speaking - enter the character's portrait sprite name (if any)")]
		public string characterIllustration;
		[Tooltip("Character's FaceFX")]
		public AnimationClip animationClip;
		[Tooltip("Character's dialogue")]
		public AudioClip audioClip;
		[Tooltip("AudioClip above is tutor's voice. Swap tutor voices according to Learner's Profile settings.")]
		public bool isTutorAudioClip;

		public List<CharacterAnimation> ExtraCharacters;
		
		[Space (10)]
		[Header ("Bubble Data")]
		
		[Tooltip("The first bubble of the conversation uses this field to set the conversation's layout. Not relevant in the other bubbles. Choose 'None' to play FaceFX and audio only, skipping the UI bubble.")]
		public DialogueType type;
		
		//[InspectorDisplayIfAttribute ("showIsStartEnd")]
		[Tooltip("Is this dialogue the first one in the conversation?")]
		public bool isStart;
		
		//[InspectorDisplayIfAttribute ("showBubbleParams")]
		[Tooltip("Fade bubble out at the end of the dialogue event. Use isEnd tickbox below to actually terminate the conversation.")]
		public bool fadeOnEnd;
		
		//[InspectorDisplayIfAttribute ("showIsStartEnd")]
		[Tooltip("Is this dialogue the last one in the conversation?")]
		public bool isEnd;
		
		//[InspectorDisplayIfAttribute ("showBubbleParams")]
		[Tooltip("Hide the pointer to the 3D character speaking. If no character is referenced, the pointer hides by default.")]
		public bool hidePointer;
		
		//[InspectorDisplayIfAttribute ("showIsLearner")]
		[Tooltip("Whether this dialogue is supposed to be recorded by the learner and analysed by Carnegie or not.")]
		public bool isLearner;

		//[InspectorDisplayIfAttribute ("isLearner")]
		[Tooltip("Hides the previous bubble from being displayed on top of this learner bubble.")]
		public bool hideReferenceBubble;

        //[InspectorDisplayIfAttribute("showBubbleParams")]
        [Tooltip("Whether this dialogue belongs to a did you notice timeline or not, if ticked will show bubble without skip or replay.")]
        public bool isDidYouNotice;

        //[InspectorDisplayIfAttribute ("showTutorAudioClips")]
		[Tooltip("(Soon to be obsolete) Dialogue's model answer. Please use the TutorAudioClips fields inside each DialogueText entry below instead.")]
		public TutorAudioClips tutorAudioClips;
		
		//[InspectorDisplayIfAttribute ("showDialogueText")]
		[Tooltip("Dialogue content. Each entry represents a speech bubble.")]
		public List<DialogueText> dialogueText;
		
		// Parameters display - flags
		private bool showBubbleParams { get { return (type != DialogueType.None && type != DialogueType.ButtonsOnly); } }
		private bool showIsStartEnd { get { return (type != DialogueType.None); } }
		private bool showIsLearner { get { return (type != DialogueType.None); } }
		private bool showDialogueText { get { return showTutorAudioClips; } }
		private bool showTutorAudioClips {
			get {
				if (type == DialogueType.None)
					return false;
				if (type == DialogueType.ButtonsOnly)
					return isLearner;
				return true;
			} }
	}
	
	// Main data box's strings
	[System.Serializable]
	public struct DialogueText
	{
		[Tooltip("Text to be contained by this bubble.")]
		public string text;
		[Tooltip("If isLearner is ticked above - is this answer considered correct?")]
		public bool isCorrect;
		[Tooltip("If isLearner is ticked above - tick this if this answer is to be ignored by Carnegie, but kept on screen as a reference for the user to use.")]
		public bool isReferenceAnswer;
		[Tooltip("Model answers (help button audio) for this answer.")]
		public TutorAudioClip tutorAudioClips;
		[HideInInspector]
		public string carnegieOriginalText;
		[Tooltip("Variations of the 'text' field to be sent to Carnegie for analysis.")]
		public string[] carnegieText;
	}

	[System.Serializable]
	public struct TutorAudioClip
	{
		public AudioClip female;
		public AudioClip male;
	}

	[System.Serializable]
	public struct TutorAudioClips
	{
		public AudioClip angela;
		public AudioClip jack;
	}

	[System.Serializable]
	public struct CharacterAnimation
	{
		[Tooltip("Play FX 'n' seconds after this event's fire time. Will change the sequence event duration. Negative values will be ignored.")]
		public float delay;
		[Tooltip("The 3D character speaking (if any)")]
		public GameObject character;
		[Tooltip("Character's FaceFX")]
		public AnimationClip animationClip;
		[Tooltip("Character's dialogue")]
		public AudioClip audioClip;
	}
	
	// Main data box's dialogue type / display mode
	public enum DialogueType
	{
		Subtitles,
		List,
		None,
		ButtonsOnly
	}
	
	
	//-------------------------------------------------------------------------------------------
	
	// Data instance
	public DialogueData dialogueData = new DialogueData ();
}











