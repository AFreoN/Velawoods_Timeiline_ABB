using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class FaceAnim : MonoBehaviour 
{
	AudioSource audioSource = null;

	public bool IsMorphCharacter;
	AudioClip clip;
	[HideInInspector] public AudioClip footStepSound = null; //Foot step sound played by animator keyframes, values set by FootStepSound Timeline Track

	Transform footStepTransform = null;
	AudioSource footAudioSource = null;
	[HideInInspector] public AudioClip _FootStepSound;

	void Start () {

		audioSource = GetComponent<AudioSource>();

		#region Getting the Foot Step AudioSource
		footStepTransform = transform.Find("FootStepSound(Clone)");
		if(footStepTransform == null)
        {
			Object audioPrefab = Resources.Load("FootStepSound");
			GameObject g = Instantiate(audioPrefab) as GameObject;
			footStepTransform = g.transform;
			footStepTransform.SetParent(transform);
			footStepTransform.position = Vector3.zero;
			footAudioSource = g.GetComponent<AudioSource>();
        }
        else
        {
			footAudioSource = footStepTransform.GetComponent<AudioSource>();
        }
        #endregion
	}

	public void FootStepAudio()
    {
		if (footStepSound == null) return;
        UnityEngine.Debug.Log("Playing footstep sound : " + footStepSound.name);
		footAudioSource.spatialBlend = 1f;
		footAudioSource.PlayOneShot(footStepSound);
    }

	[ContextMenu("Play Anim & Audio")]
	public void playAnimAudio(AudioClip _clip, string _animClipName)
    {
		clip = _clip;
		//animName = "Default_" + _animClip.name;
		string animName = string.IsNullOrEmpty(_animClipName) ? null : _animClipName;
		audioSource.clip = clip;

		if (IsMorphCharacter)
		{
			Morph_FaceFXControllerScript_Setup fcs = GetComponent<Morph_FaceFXControllerScript_Setup>();
			if (fcs)
				fcs.PlayAnim(animName, clip);
			else
                UnityEngine.Debug.LogError("No FaceFXControllerScript_Morph script found!");
		}
		else
		{
			Bones_FaceFXControllerScript_Setup fcs = GetComponent<Bones_FaceFXControllerScript_Setup>();
			if (fcs)
				fcs.PlayAnim(animName, clip);
			else
                UnityEngine.Debug.LogError("No FaceFXControllerScript_Bones script found!");
		}
	}
}
