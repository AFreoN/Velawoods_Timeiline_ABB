using UnityEngine;
using System.Collections;

public class AmbientSoundManager : MonoBehaviour 
{
    [HideInInspector()]
    public bool IsActive = false;
	public AudioSource MainAudioSource;
	public AudioSource BlendingAudioSource;
	public float MainAudioVolume = 1.0f;

    private bool StartingInSecondSource = false;


	private float VolumeFadeModifier = 0.0f;
	private float inverseVolumeFadeModifier
	{
		get { return 1.0f - VolumeFadeModifier; }
	}

	private float fadeRatio = 0.0f;
	private float inverseFadeRatio
	{
		get { return 1.0f - fadeRatio; }
	}

	private bool fadingIn = false;
	private bool fading = false;
	private float lerpRatio = 0.0f;
	private float targetVolume = 1.0f;

	private bool isFadingVolume = false;
	private bool isFadingUp = false;
	private float volumeLerpRatio = 0.0f;
	private float volumeFadeRatio = 0.0f;
    //Used to only fade audio back up if fade up calls is equal to fade down calls.
    //E.g. Fade audio down in minigame, called again as main menu open. Main menu closed, don't want to fade up audio again until minigame complete.
    private int fadeDownCount = 0; 

	private bool volumeFadedDown = false;

	public void Start()
	{
		CoreEventSystem.Instance.AddListener(CoreEventTypes.MINIGAME_START, FadeVolumeDown);
        CoreEventSystem.Instance.AddListener(CoreEventTypes.MINIGAME_END, FadeVolumeUp);
#if CLIENT_BUILD
        CoreEventSystem.Instance.AddListener(MainMenu.Messages.MENU_SHOWING, FadeVolumeDown);
        CoreEventSystem.Instance.AddListener(MainMenu.Messages.MENU_HIDING, FadeVolumeUp);
#endif
        CoreEventSystem.Instance.AddListener (CoreEventTypes.MISSION_END_SEQUENCE, RemoveAllAudioSources);
        CoreEventSystem.Instance.AddListener(CoreEventTypes.MISSION_START, MissionStart);
        IsActive = false;

        // Fade volume down for the task list.
        FadeVolumeDown();
        volumeLerpRatio = 1.0f;
	}

    private void MissionStart(object parameters)
    {
        IsActive = true;
        Reset();
    }

    private void Init()
    {

        BoxCollider camera = GetComponent<BoxCollider>();

        if (camera)
        {
            Bounds cameraBounds = camera.bounds;

            foreach (AmbientMusicTrigger trigger in FindObjectsOfType<AmbientMusicTrigger>())
            {
                Bounds triggerBounds = trigger.GetComponent<BoxCollider>().bounds;
                if (cameraBounds.Intersects(triggerBounds))
                {
                    trigger.ForceTrigger();

                    MainAudioSource.volume = 0.0f;
                    BlendingAudioSource.volume = targetVolume;
                    fading = false;
                    StartingInSecondSource = true;
                    fadeRatio = 1.0f;

					// Check if a conversation is currently playing
                    if (ConversationManager.Instance._conversation != null)
                    {
                        fadeDownCount++;
                        isFadingUp = false;
                        volumeFadedDown = true;
                        volumeLerpRatio = 1.0f;
                        volumeFadeRatio= 1.0f;
                        MainAudioSource.volume = (MainAudioVolume * inverseFadeRatio) - ((MainAudioVolume * inverseVolumeFadeModifier) * volumeFadeRatio);
                        BlendingAudioSource.volume = (targetVolume * fadeRatio) - ((targetVolume * inverseVolumeFadeModifier) * volumeFadeRatio);
                    }
                    break;
                }
            }
        }

        if(!StartingInSecondSource)
        {
            MainAudioSource.volume = MainAudioVolume;
        }

        if (ConversationManager.Instance._conversationScript != null)
        {
            FadeVolumeDown();
            volumeLerpRatio = 1.0f;
            Update();
        }

        MainAudioSource.Play();
    }

    public void Reset()
    {
        MainAudioSource.Stop();
        BlendingAudioSource.Stop();

        VolumeFadeModifier = 0.0f;
        fadeRatio = 0.0f;
        fadingIn = false;
        fading = false;
        lerpRatio = 0.0f;
        targetVolume = 1.0f;
        isFadingVolume = false;
        isFadingUp = false;
        volumeLerpRatio = 0.0f;
        volumeFadeRatio = 0.0f;
        fadeDownCount = 0;
        volumeFadedDown = false;
        StartingInSecondSource = false;
        BlendingAudioSource.Stop();
        BlendingAudioSource.volume = 0.0f;

        foreach(AmbientMusicTrigger trigger in GameObject.FindObjectsOfType<AmbientMusicTrigger>())
        {
            trigger.Reset();
        }

        Init();
    }

	public void FadeIn(AudioClip clip, float volume)
    {
        BlendingAudioSource.clip = clip;
		BlendingAudioSource.Play();
		targetVolume = volume;

        if (!fading)
		{
			// Fade as normal if we are not currently fading.
			fadingIn = true;
			Fade();
		}
		else
		{
			if(fadingIn)
			{
				// Do nothing and let the current fade carry on.
			}
			else
			{
				// Switch the fade around and flip the lerp amount.
				fadingIn = true;
				lerpRatio = 1.0f - lerpRatio;
			}
		}
	}

	public void FadeOut()
	{
		// Fade as normal if we are not currently fading.
		if(!fading)
		{
			fadingIn = false;
			Fade();
		}
		else
		{

			if(fadingIn)
			{
				// Switch the fade around and flip the lerp amount.
				fadingIn = false;
				lerpRatio = 1.0f - lerpRatio;
			}
			else
			{
				// Do nothing and let the current fade carry on.
			}
		}
	}

	private void Fade()
	{
		// Start a fade.
		fading = true;
		lerpRatio = 0.0f; 
	}
	
	void Update()
	{
		if(fading)
		{
			lerpRatio += Time.deltaTime;

			float startingValue = fadingIn ? 0.0f : 1.0f;
			float endingValue = fadingIn ? 1.0f : 0.0f;

			fadeRatio = Mathf.Lerp(startingValue, endingValue, lerpRatio);

			MainAudioSource.volume = (MainAudioVolume * inverseFadeRatio) - ((MainAudioVolume * inverseVolumeFadeModifier) * volumeFadeRatio);
			BlendingAudioSource.volume = (targetVolume * fadeRatio) - ((targetVolume * inverseVolumeFadeModifier) * volumeFadeRatio);

			fading = fadeRatio != endingValue;
		}

		if(isFadingVolume)
		{
			volumeLerpRatio += Time.deltaTime;
			
			float startingValue = isFadingUp ? 1.0f : 0.0f;
			float endingValue = isFadingUp ? 0.0f : 1.0f;
			
			volumeFadeRatio = Mathf.Lerp(startingValue, endingValue, volumeLerpRatio);
			
			isFadingVolume = volumeFadeRatio != endingValue;

			if(!isFadingVolume)
			{
				volumeFadedDown = !isFadingUp;
			}

			if(!fading)
			{
				MainAudioSource.volume = (MainAudioVolume * inverseFadeRatio) - ((MainAudioVolume * inverseVolumeFadeModifier) * volumeFadeRatio);
				BlendingAudioSource.volume = (targetVolume * fadeRatio) - ((targetVolume * inverseVolumeFadeModifier) * volumeFadeRatio);
			}
		}
	}
	
	public void FadeVolumeUp(object paramter = null)
	{
        fadeDownCount--;
        //Fade out called more times than fade in. Don't want to reenable audio yet as should still be faded down.
        if (fadeDownCount > 0)
        {
            return;
        }

        if (volumeFadedDown)
		{
			if(!isFadingVolume)
			{
				// Fade as normal if we are not currently fading.
				isFadingUp = true;
				FadeVolume();
			}
			else
			{
				if(isFadingUp)
				{
					// Do nothing and let the current fade carry on.
				}
				else
				{
					// Switch the fade around and flip the lerp amount.
					isFadingUp = true;
					volumeLerpRatio = 1.0f - volumeLerpRatio;
				}
			}
		}
	}

	public void FadeVolumeDown(object paramter = null)
    {
        fadeDownCount++;
        if (!volumeFadedDown)
		{
			// Fade as normal if we are not currently fading.
			if(!isFadingVolume)
			{
				isFadingUp = false;
				FadeVolume();
			}
			else
			{
				
				if(isFadingUp)
				{
					// Switch the fade around and flip the lerp amount.
					isFadingUp = false;
					volumeLerpRatio = 1.0f - volumeLerpRatio;
				}
				else
				{
					// Do nothing and let the current fade carry on.
				}
			}
		}
	}

	private void FadeVolume()
	{
		isFadingVolume = true;
		volumeLerpRatio = 0.0f;
	}

	public void OnDestroy()
	{
		CoreEventSystem.Instance.RemoveListener(CoreEventTypes.MINIGAME_START, FadeVolumeDown);
        CoreEventSystem.Instance.RemoveListener(CoreEventTypes.MINIGAME_END, FadeVolumeUp);
		CoreEventSystem.Instance.RemoveListener(CoreEventTypes.MISSION_END_SEQUENCE, RemoveAllAudioSources);
#if CLIENT_BUILD
        CoreEventSystem.Instance.RemoveListener(MainMenu.Messages.MENU_SHOWING, FadeVolumeDown);
        CoreEventSystem.Instance.RemoveListener(MainMenu.Messages.MENU_HIDING, FadeVolumeUp);
#endif
        CoreEventSystem.Instance.RemoveListener(CoreEventTypes.MISSION_START, MissionStart);
    }

	private IEnumerator FadeOutAtMissionEnd(AudioSource audioSourceFade)
	{
		float originalVolume = audioSourceFade.volume;
		while(audioSourceFade.volume > 0)
		{
			audioSourceFade.volume -= Time.deltaTime;
			yield return null;
		}
		audioSourceFade.Stop ();
		audioSourceFade.volume = originalVolume;
	}

	private void RemoveAllAudioSources(object parameters)
	{
		AudioSource[] audioSources = FindObjectsOfType<AudioSource> ();
		foreach(AudioSource audioSource in audioSources)
		{
			StartCoroutine(FadeOutAtMissionEnd(audioSource));
		}
	}

    public void Resume()
    {
        if (MainAudioSource)
        {
            MainAudioSource.volume = (MainAudioVolume * inverseFadeRatio) - ((MainAudioVolume * inverseVolumeFadeModifier) * volumeFadeRatio);
        }

        if (BlendingAudioSource)
        {
            BlendingAudioSource.volume = (targetVolume * fadeRatio) - ((targetVolume * inverseVolumeFadeModifier) * volumeFadeRatio);
        }
    }

    public void Stop()
    {
        fading = false;
        volumeFadedDown = false;

        volumeFadeRatio = isFadingUp ? 0.0f : 1.0f;
        fadeRatio = fadingIn ? 1.0f : 0.0f;

        if(MainAudioSource)
        {
            MainAudioSource.volume = 0.0f; 
        }

        if (BlendingAudioSource)
        {
            BlendingAudioSource.volume = 0.0f;
        }
    }
}