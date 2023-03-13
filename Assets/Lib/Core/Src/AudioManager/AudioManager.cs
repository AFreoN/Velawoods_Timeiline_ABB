using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace CoreSystem
{
	public enum AudioType
	{
		SFX,
		Music,
		Dialogue,
		All
	}

	public delegate void AudioCallback();

	public class AudioManager : MonoSingleton<AudioManager>
	{
		private AudioSource _audioPlayerDialogue; //plays audio clips as dialogue and only one can play at a time
		private AudioSource _audioPlayerSFX; //plays audio clips as one shot
		private AudioSource _audioPlayerMusic; //plays a looping sound
		private AudioSource _customPlayer; //access to a audio source and any class can change its variables
		private Dictionary<string, AudioClip> _cachedAudioFiles; //any files loaded via this script are cached

		private bool _dialoguePaused = false;
		private bool _sfxPaused = false;
		private bool _musicPaused = false;

		private float _sfxVolume = 1.0f;
		private float _musicVolume = 1.0f;
		private float _dialogueVolume = 1.0f;

		private void SetUpPlayers()
		{
			List<AudioSource> sources = new List<AudioSource>(Core.Instance.gameObject.GetComponents<AudioSource>());

			while(sources.Count < 4)
			{
				sources.Add(Core.Instance.gameObject.AddComponent<AudioSource>());
			}

			_audioPlayerSFX = sources[0];
			_audioPlayerMusic = sources[1];
			_audioPlayerDialogue = sources [2];
			_customPlayer = sources[3];

			_audioPlayerMusic.loop = true;

			_audioPlayerSFX.volume = _sfxVolume;
			_audioPlayerMusic.volume = _musicVolume;
			_audioPlayerDialogue.volume = _dialogueVolume;
		}

		public AudioSource CustomAudioSource
		{
			get{return _customPlayer;}
		}

		protected override void Init ()
		{
			try
			{
				SetUpPlayers();
				_cachedAudioFiles = new Dictionary<string, AudioClip>();
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}
		}

		/// <summary>
		/// Changes the volume of a given type.
		/// </summary>
		/// <param name="audioType">Audio type.</param>
		/// <param name="volume">Volume.</param>
		public void ChangeVolume(AudioType audioType, float volume)
		{
			switch(audioType)
			{
			case AudioType.SFX:
				_sfxVolume = volume;
				_audioPlayerSFX.volume = volume;
				break;
				
			case AudioType.Music:
				_audioPlayerMusic.volume = volume;
				_musicVolume = volume;
				break;
			case CoreSystem.AudioType.Dialogue:
				_audioPlayerDialogue.volume = volume;
				_dialogueVolume = volume;
				break;
			}
		}

		/// <summary>
		/// Switchs the mute boolean i.e true -> false.
		/// </summary>
		/// <param name="audioType">Audio type.</param>
		public void SwitchMute(AudioType audioType)
		{
			switch(audioType)
			{
			case AudioType.SFX:
				_audioPlayerSFX.mute = !_audioPlayerSFX.mute;
				_audioPlayerDialogue.mute = _audioPlayerSFX.mute;
				break;
				
			case AudioType.Music:
				_audioPlayerMusic.mute = !_audioPlayerMusic.mute;
				break;
			}
		}

		/// <summary>
		///	Loads the sound effect and Plays it. This is cached. Returns the audio clip that is played
		/// </summary>
		/// <param name="soundFileName">Sound file name.</param>
		public AudioClip PlayAudio(string soundFileName, AudioType audioType=AudioType.SFX, float volumeScale = 1.0f, float delay = 0)
		{
			if(audioType==AudioType.SFX && _cachedAudioFiles.ContainsKey(soundFileName))
			{
				PlayAudio(_cachedAudioFiles[soundFileName], audioType, volumeScale, delay);
				return _cachedAudioFiles[soundFileName];
			}
			else
			{
				AudioClip soundEffect = Resources.Load<AudioClip>(soundFileName);

				if(soundEffect != null)
				{
					_cachedAudioFiles[soundFileName] = soundEffect;
					PlayAudio(soundEffect, audioType, volumeScale, delay);
					return soundEffect;
				}
				else
				{
					Debug.LogWarning("Audio Manager: No Audio could be loaded from resources called: " + soundFileName);
				}
			}

			return null;
		}

		private void DelayAudio(AudioClip soundEffect, AudioType audioType, float volumeScale, float delay)
		{
			DelayedAudio script = Core.Instance.gameObject.AddComponent<DelayedAudio>();
			script.Init(soundEffect, volumeScale, delay, audioType);
		}

		/// <summary>
		/// Adds a audio source and plays the audio clip.
		/// </summary>
		/// <param name="attach_to">Attach_to.</param>
		/// <param name="clip">Clip.</param>
		public void AddSourceAndPlay(GameObject attach_to, AudioClip clip, float volumeScale = 1.0f)
		{
			/// TODO
			/// Different types of playing audio (e.g Fade)
			/// Limit number of audio sources on one game object (3 maybe)
			if(attach_to != null && clip != null)
			{
				AudioSource source = attach_to.AddComponent<AudioSource>();
				source.PlayOneShot(clip, volumeScale);
			}
		}

        /// <summary>
		/// Adds a audio source and plays the audio clip in 3D.
		/// </summary>
		/// <param name="attach_to">Attach_to.</param>
		/// <param name="clip">Clip.</param>
		public AudioSource AddSourceAndPlay3D(GameObject attach_to, AudioClip clip, float volumeScale = 1.0f)
        {
            AudioSource source = null;

            if (attach_to != null && clip != null)
            {
                GameObject footStepObject = null;

                Transform footStepTransform = attach_to.transform.Find("FootStepSound(Clone)");

                if (footStepTransform == null)
                {
                    UnityEngine.Object audioPrefab = Resources.Load("FootStepSound");

                    if(null != audioPrefab)
                    {
                        GameObject audioGameobject = GameObject.Instantiate(audioPrefab, Vector3.zero, Quaternion.identity) as GameObject;

                        if(null != audioGameobject)
                        {
                            audioGameobject.transform.SetParent(attach_to.transform, false);

                            footStepObject = audioGameobject;
                        }
                    }                    
                }
                else
                {
                    footStepObject = footStepTransform.gameObject;
                }

                if (null != footStepObject)
                {
                    source = footStepObject.GetComponent<AudioSource>();

                    if (source == null)
                    {
                        source = footStepObject.AddComponent<AudioSource>();
                    }

                    source.spatialBlend = 1.0f;

                    source.PlayOneShot(clip, volumeScale);
                }
            }

            return source;
        }

        /// <summary>
        /// Plays the given sound effect.
        /// 
        /// You can play multiple sound effects with this.
        /// </summary>
        /// <param name="soundEffect">Sound effect.</param>
        public void PlayAudio(AudioClip soundEffect, AudioType audioType, float volumeScale = 1.0f, float delay = 0, MonoBehaviour caller = null, AudioCallback callback = null)
		{
			if(soundEffect == null)
			{
				Debug.LogWarning("Audio Manager: Audio Clip was null on PlayAudio()");
				return;
			}

			if(_audioPlayerSFX == null || _audioPlayerMusic == null || _audioPlayerDialogue == null ) SetUpPlayers();

			if(delay > 0)
			{
				DelayAudio(soundEffect, audioType, volumeScale, delay);
				if (caller != null)
				{
					caller.StartCoroutine(AudioFinishedCallback(soundEffect.length + delay, callback));
				}
			}
			else
			{
				switch(audioType)
				{
				case AudioType.Dialogue:
					_audioPlayerDialogue.Stop ();
					_audioPlayerDialogue.PlayOneShot( soundEffect, volumeScale );
					break;

				case AudioType.SFX:
					_audioPlayerSFX.PlayOneShot(soundEffect, volumeScale);
					break;

				case AudioType.Music:
					_audioPlayerMusic.clip = soundEffect;
					_audioPlayerMusic.Play();
					break;
				}

				if (caller != null)
				{
					caller.StartCoroutine(AudioFinishedCallback(soundEffect.length, callback));
				}
			}
		}

		public void StopAudio( AudioType type ) {
			switch( type ) {
			case AudioType.Dialogue:
				_audioPlayerDialogue.Stop();
				break;
			case AudioType.SFX:
				_audioPlayerSFX.Stop();
				break;
			case AudioType.Music:
				_audioPlayerMusic.Stop();
				break;
			}
		}

		public void PauseAudio( AudioType type ) {
			switch( type ) {
			case AudioType.Dialogue:
				_audioPlayerDialogue.Pause();
				_dialoguePaused = true;
				break;
			case AudioType.SFX:
				_audioPlayerSFX.Pause();
				_sfxPaused = true;
				break;
			case AudioType.Music:
				_audioPlayerMusic.Pause();
				_musicPaused = true;
				break;
			}
		}

		public void ResumeAudio( AudioType type ) {
			switch( type ) {
			case AudioType.Dialogue:
				_audioPlayerDialogue.UnPause();
				_dialoguePaused = false;
				break;
			case AudioType.SFX:
				_audioPlayerSFX.UnPause();
				_sfxPaused = false;
				break;
			case AudioType.Music:
				_audioPlayerMusic.UnPause();
				_musicPaused = false;
				break;
			}
		}

		public bool IsPaused( AudioType type ) {
			switch( type ) {
			case AudioType.Dialogue:
				return _dialoguePaused;
			case AudioType.SFX:
				return _sfxPaused;
			case AudioType.Music:
				return _musicPaused;
			}
			return false;
		}

		private IEnumerator AudioFinishedCallback(float time, AudioCallback callback)
		{
			yield return new WaitForSeconds(time);
			if (callback != null)
			{
				callback ();
			}
		}
	}


}
