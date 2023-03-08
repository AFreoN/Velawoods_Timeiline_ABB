using UnityEngine;
using System.Collections;

using CoreLib;

public class GenericBubble_ReplaySelfAssessment : GenericBubble_DropdownBase {

	private AudioClip _audioClip;
	
	public void SetAudioClip (AudioClip clip)
	{
		_audioClip = clip;
	}
	
	public override void DropdownClicked ()
	{
		base.DropdownClicked ();
		
		if (GetState != State.Showing) return;
		
		if (_audioClip)
		{
			if (AudioManager.Instance != null)
				AudioManager.Instance.PlayAudio (_audioClip, CoreLib.AudioType.Dialogue);
		}
		else
			Debug.Log ("Replay Self Assessment Button: AudioClip missing.");
	}
	
	public override void SlideDown (float slideTime, float waitTime = 0)
	{
		if (_audioClip == null)
			return;
		
		base.SlideDown (slideTime, waitTime);
	}
}
