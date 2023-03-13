using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using CoreSystem;

public class GenericBubble_Help : GenericBubble_DropdownBase {

	private AudioClip _audioClip;
	List<AudioClip> List_AudioClips = new List<AudioClip> ();

	public AudioClip GetAudioClip ()
	{
		return _audioClip;
	}

	public void SetAudioClip (AudioClip clip)
	{
		_audioClip = clip;
		List_AudioClips.Clear ();
	}

	public void SetAudioClips (List<AudioClip> lst){
		for (int i = 0; i < lst.Count; i++) {
			List_AudioClips.Add (lst[i]);
		}
		_audioClip = null;
	}

	public void Reset (){
		_audioClip = null;
		List_AudioClips.Clear ();
	}
	
	public override void DropdownClicked ()
	{
		base.DropdownClicked ();
	
		if (GetState != State.Showing) return;
		
		if (_audioClip)
		{
			if (AudioManager.Instance != null)
				AudioManager.Instance.PlayAudio (_audioClip, CoreSystem.AudioType.Dialogue);
		}
		else if (List_AudioClips.Count > 0){
			float auDelay = 0f;
			for (int i = 0; i < List_AudioClips.Count; i++) {
				CoreHelper.PlayAudioClip (List_AudioClips[i], CoreSystem.AudioType.Dialogue, 1f, auDelay);
				auDelay += List_AudioClips[i].length + 0.25f;
			}
		}else
			Debug.Log ("Help Button: AudioClip missing.");
	}
	
	public override void SlideDown (float slideTime, float waitTime = 0)
	{
		if (_audioClip == null)
			return;
	
		base.SlideDown (slideTime, waitTime);
	}
}
