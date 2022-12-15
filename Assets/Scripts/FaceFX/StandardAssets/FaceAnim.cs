using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class FaceAnim : MonoBehaviour {

	[HideInInspector] public string animName = "AnimName";    //Default_taming_the_bicycle_twain_ep_001

	AudioSource audioSource = null;

	public bool IsMorphCharacter;
	public AudioClip clip;
	[SerializeField] AudioClip footStepSound = null;

	Transform footStepTransform = null;
	AudioSource footAudioSource = null;

	void Start () {

		audioSource = GetComponent<AudioSource>();

		if (clip != null)
		{
			audioSource.clip = clip;
			animName = "Default_" + clip.name;
		}

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
        UnityEngine.Debug.Log("Playing footstep sound");
		if (footStepSound == null) return;
		footAudioSource.spatialBlend = 1f;
		footAudioSource.PlayOneShot(footStepSound);
    }

	[ContextMenu("Play Anim & Audio")]
	public void playAnimAudio(AudioClip _clip, string _animClipName)
    {
		clip = _clip;
		//animName = "Default_" + _animClip.name;
		animName = _animClipName;
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
