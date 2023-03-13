using UnityEngine;
using UnityEngine.UI;
using System.Collections;

using CoreSystem;

public class GenericButton_Speaker : GenericObject {
	[Header ("Speaker Button")]

	public AudioClip _audioClip;

	[HideInInspector]
	public const CoreSystem.AudioType _audioLayer = CoreSystem.AudioType.Dialogue;
	
	public override void Show (object[] paramList)
	{
		base.Show (paramList);
	}
	
	public override void Hide (object[] paramList)
	{
		base.Hide (paramList);
	}
	
	public virtual void Enable (bool enabled)
	{
		transform.GetChild (0).GetComponent<Button> ().enabled = enabled;
	}
	
	public virtual void SetAudioClip (AudioClip audioClip)
	{
		_audioClip = audioClip;
	}
	
	public virtual AudioClip GetAudioClip ()
	{
		return _audioClip;
	}

	public virtual void Play ()
	{
		if (_audioClip == null)
		{
			Debug.Log ("Speaker Button : No audioClip loaded");
			return;
		}

		AudioManager.Instance.PlayAudio (_audioClip, _audioLayer);
	}
	
	public virtual void ButtonClicked ()
	{
		Play ();
	}
}
