using UnityEngine;
using System.Collections;

public class AmbientMusicTrigger : MonoBehaviour 
{
	public AmbientSoundManager SoundManager;
	public AudioClip Clip;
	public float Volume = 1.0f;

    private bool shouldBeInside = false;

	void OnTriggerEnter(Collider other) 
	{
		if (SoundManager == null) {
			Debug.LogError ("AmbientMusicTrigger : Sound Manager field is empty!");
			return;
		}

        if (SoundManager.IsActive == true && shouldBeInside == false)
        {
            SoundManager.FadeIn(Clip, Volume);
            shouldBeInside = true;
        }
	}

	void OnTriggerExit(Collider other) 
	{
		if (SoundManager == null) {
			Debug.LogError ("AmbientMusicTrigger : Sound Manager field is empty!");
			return;
		}

        if (shouldBeInside)
        {
            SoundManager.FadeOut();
            shouldBeInside = false;
        }
	}

    public void ForceTrigger()
    {
		if (SoundManager == null) {
			Debug.LogError ("AmbientMusicTrigger : Sound Manager field is empty!");
			return;
		}
		SoundManager.FadeIn(Clip, Volume);
        shouldBeInside = true;
    }

    public void Reset()
    {
        shouldBeInside = false;
    }
}
