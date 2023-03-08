using UnityEngine;
using System.Collections;

namespace CoreLib
{
	public class DelayedAudio : MonoBehaviour 
	{

		public AudioClip _clip;
		public AudioType _audioType;
		public float _delay;
		public float _volumeScale;
		
		private float _counter;

		public void Init(AudioClip clip, float volumeScale, float delay, AudioType type)
		{
			if(delay > 0 && clip != null)
			{
				_clip = clip;
				_delay = delay;
				_audioType = type;
				_volumeScale = volumeScale;
				_counter = 0;
			}
			else
			{
				Debug.Log("DelayedAudio: Clip was NUll or delay was below 0");
				Destroy(this);
			}
		}
		
		// Update is called once per frame
		void Update () {
			if(_counter < _delay)
			{
				_counter += 1.0f * Time.deltaTime;
			}
			else
			{
				AudioManager.Instance.PlayAudio(_clip, _audioType, _volumeScale);
				Destroy(this);
			}
		}
	}
}
