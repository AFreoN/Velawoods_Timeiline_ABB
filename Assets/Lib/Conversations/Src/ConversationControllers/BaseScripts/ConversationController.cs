using UnityEngine;
using System.Collections;

/// <summary>
/// Base class for Conversation Controllers / Scene objects which manage bubbles, transitions, etc.
/// </summary>
public class ConversationController : MonoBehaviour
{
	/// <summary> General timing for transitions.</summary>
	public const float _lerpTime = 0.3f;
	
	/// <summary> Brings the dialogue event on screen. </summary>
	public virtual void StartDialogue (DialogueEventData.DialogueData dialogueData)
	{
		//;
	}
	
	/// <summary> End a dialogue event. </summary>
	public virtual void EndDialogue (DialogueEventData.DialogueData dialogueData)
	{
		//;
	}
	
	/// <summary> Hides all dialogues and destroys controller. Usually called at the end of the conversation. </summary>
	public virtual void EndConversation ()
	{
		//;
	}
	
	/// <summary> Returns the default AudioClip to be initially set as Model Answer (soon to be obsolete - replaced by model answers inside each DialogueText struct). </summary>
	public virtual AudioClip GetDefaultModelAnswer (DialogueEventData.DialogueData dialogueData)
	{
		bool tutorIsMale = false;
#if CLIENT_BUILD
		tutorIsMale	= (PlayerProfile.Instance.Tutor == PlayerProfile.Gender.Male);	
#endif
		return (tutorIsMale) ? dialogueData.tutorAudioClips.jack : dialogueData.tutorAudioClips.angela;
	}
	
	public void OnDestroy ()
	{
		StopAllCoroutines ();
	}
}

