using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using CoreLib;

using TMPro;
using System;

public class GenericMisc_AudioPlayer : MonoBehaviour, Route1Games.IPausable
{

	/*
		GENERIC AUDIO PLAYER
		**
		Public Properties:
		**
		(List<AudioClip>) _playlist 
		Show ()
		Hide ()
		Reset ()
		
		Summary:
		**
		This player can hold multiple audio files and play them in order.
		If you wish to change the size of this object, manipulate the parent's RectTransform.
		The scrubber of this player allows for shuffling about and playing audio file(s) starting from anywhere in the player's timeline.
		Changing the playlist during runtime is ok and will set the scrubber back to starting point.
	*/

	///<summary> Playlist container. </summary> 
	public List<AudioClip> _playlist = new List<AudioClip> ();
	
	//
	
	// Controls
	private const float _audioSpacing = 4.0f; // Space in between playlist's audio files
	
	// Playlist total length (seconds, including _audioSpacing) getter
	private float _playlistTotalLength = 0; 
	private List<AudioClip> _playlistTemp = new List<AudioClip> ();
    private bool _enabled;
	
	// State manager
	private enum PlayerState { Paused, Playing, Ended }
	private PlayerState _state = PlayerState.Paused;
	private PlayerState State {
		get { return _state; }
		set { 
			_state = value;
			switch (_state)
			{
			case PlayerState.Paused:
				PlayButton.gameObject.SetActive (true);
				PauseButton.gameObject.SetActive (false);
				ReplayButton.gameObject.SetActive (false);
				Timer.gameObject.SetActive (true);
				
				StopCoroutine ("PlayRoutine");
				StopAudio ();
				break;
			case PlayerState.Playing:
				PauseButton.gameObject.SetActive (true);
				ReplayButton.gameObject.SetActive (false);
				PlayButton.gameObject.SetActive (false);
				Timer.gameObject.SetActive (true);
				
				StopCoroutine ("PlayRoutine");
				StopAudio ();
				
				StartCoroutine ("PlayRoutine");
				break;
			case PlayerState.Ended:
				ReplayButton.gameObject.SetActive (true);
				PauseButton.gameObject.SetActive (false);
				PlayButton.gameObject.SetActive (false);
				Timer.gameObject.SetActive (false);
				
				StopCoroutine ("PlayRoutine");
				StopAudio ();
				break;
			}
		}
	}
	

//-Interface-------------------------------------------------------------------------

	/// <summary> Fade in </summary>
	public void Show (float seconds = 0)
	{
		UITween.fadeCanvasGroupTo (FadeComponent.gameObject, 0, 0, UITween.UIFadeType.easeInSine, false);
		UITween.fadeCanvasGroupTo (FadeComponent.gameObject, 1, seconds, UITween.UIFadeType.easeInSine, false);
		
		Reset ();
	}
	
	/// <summary> Fade out </summary>
	public void Hide (float seconds = 0, bool destroyOnEnd = false)
	{
		if (State == PlayerState.Playing)
			State = PlayerState.Paused;
			
		UITween.fadeCanvasGroupTo (_fadeComponent.gameObject, 0, seconds, UITween.UIFadeType.easeInSine, false);
		
		if (destroyOnEnd)
			Destroy (gameObject, seconds + 0.1f);
	}
	
	/// <summary> Pause the playback and set scrubber back to 0. </summary>
	public void Reset ()
	{
		Scrub.value = 0;
		State = PlayerState.Paused;
        _enabled = true;
    }
	
	
//-General---------------------------------------------------------------------------

	public void Start ()
	{
		Reset ();
        Route1Games.PauseManager.Instance.Register(this);
        CoreEventSystem.Instance.AddListener(CoreEventTypes.ACTIVITY_REVERSE, OnActivityChange);
        CoreEventSystem.Instance.AddListener(CoreEventTypes.ACTIVITY_SKIP, OnActivityChange);
	}

    private void OnActivityChange(object parameters = null)
    {
        StopAudio();
    }


	public void Update ()
	{
		// Check for changes in playlist
		bool change = false;
		if (_playlistTemp.Count != _playlist.Count)
			change = true;
		if (!change)
			for (int i=0; i<_playlist.Count; i++)
				if (_playlistTemp [i] != _playlist [i])
					change = true;
		if (change)
		{
			Reset ();
			
			_playlistTemp.Clear ();
			_playlistTotalLength = 0;
			foreach (AudioClip audio in _playlist)
			{
				if (audio != null) 
					_playlistTotalLength += audio.length + _audioSpacing;
				_playlistTemp.Add (audio);
			}
			_playlistTotalLength -= _audioSpacing;
		}
	}
	
	public void OnDestroy ()
	{
		StopAllCoroutines ();
        Route1Games.PauseManager.Instance.Unregister(this);
    }

//-Buttons---------------------------------------------------------------------------

	public void PlayButtonOnClick ()
	{
        if(_enabled == false)
        {
            return;
        }
		State = PlayerState.Playing;
	}
	
	public void ReplayButtonOnClick ()
	{
		Scrub.value = 0;
		State = PlayerState.Playing;
	}
	
	public void PauseButtonOnClick ()
	{
		State = PlayerState.Paused;
	}
	
	public void ScrubOnClick ()
	{	// Override clicking on the button currently active
		
		if (PlayButton.gameObject.activeSelf) 
			PlayButtonOnClick ();
		else
		if (ReplayButton.gameObject.activeSelf) 
			ReplayButtonOnClick ();
		else
		if (PauseButton.gameObject.activeSelf) 
			PauseButtonOnClick ();
	}
	
	public void ScrubOnBeginDrag ()
	{
		if (State == PlayerState.Playing)
		{
			State = PlayerState.Paused;
		}
	}
	
	public void ScrubOnValueChanged ()
	{
		// Update fill
		Vector3 fillScale = TimelineFill.transform.localScale;
		fillScale.x = 1.0f - Scrub.value;
		TimelineFill.transform.localScale = fillScale;
		
		// Update timer
		SetTimer ((int)(Scrub.value * _playlistTotalLength));
		
		// Show replay button if at the end of playlist
		if (Scrub.value > 0.99999) {
			State = PlayerState.Ended;
		}
		else {
			if (State == PlayerState.Ended)
				State = PlayerState.Paused;
		}
	}
	

//-Privates--------------------------------------------------------------------------

	private void PlayAudio (AudioClip clip, float offset = 0)
	{
		if (offset < 0) offset = 0;
		
		AudioManager.Instance.CustomAudioSource.clip = clip;
		AudioManager.Instance.CustomAudioSource.time = offset;
		AudioManager.Instance.CustomAudioSource.Play ();
	}
	
	private void StopAudio ()
	{
		AudioManager.Instance.CustomAudioSource.Stop ();
		AudioManager.Instance.CustomAudioSource.clip = null;
	}
	
	private void SetTimer (int totalSeconds)
	{
		int minutes = totalSeconds / 60;
		int seconds = totalSeconds % 60;
		
		Timer.text = (((minutes < 10) ? "0" : "") + minutes.ToString ()) + ":" + (((seconds < 10) ? "0" : "") + seconds.ToString ());
	}
	

//-Coroutines------------------------------------------------------------------------

	private IEnumerator PlayRoutine ()
	{	// Start playback
	
		int audioIndex = 0;  // Index of audio under the scrub
		float audioOffset = 0; // Delta time between start of audio at startAudioIndex and scrub position
		float scrubPosition = Scrub.value * _playlistTotalLength;
		float waitBeforePlay = 0; // Used to wait if playback started in between audios
		
		// Populate above values
		float timeCount = 0;
		for (int i=0; i<_playlist.Count; i++)
		{
			AudioClip audio = _playlist [i];
			
			if (audio == null)
			{
				audioIndex++;
				continue;
			}
			
			// Check if inside audio (at audioIndex)
			if (scrubPosition >= timeCount && scrubPosition < timeCount + audio.length)
			{
				audioOffset = scrubPosition - timeCount;
				break;
			}
			
			// If last audio, get out of loop
			if (i==_playlist.Count-1) break;
			
			timeCount += audio.length;
			audioIndex ++;
			
			// If in between this and next audio file, assign to waitBeforePlay
			if (scrubPosition >= timeCount && scrubPosition < timeCount + _audioSpacing)
			{
				waitBeforePlay = _audioSpacing - (scrubPosition - timeCount);
				break;
			}
			
			// Jump to next audio
			timeCount += _audioSpacing;
		}
		
		// If in between audios, wait for next audio
		if (waitBeforePlay > 0)
		{
			float currentTime = 0;
			while (currentTime < waitBeforePlay)
			{
				currentTime += Time.deltaTime;
				Scrub.value += Time.deltaTime/_playlistTotalLength;
				yield return null;
			}
		}
		
		// Start playing audio files w/ _audioSpacing in between them
		while (audioIndex < _playlist.Count)
		{
			if (_playlist [audioIndex] != null)
			{
				PlayAudio (_playlist [audioIndex], audioOffset);
				
				float waitFor = _playlist [audioIndex].length - audioOffset;
				waitFor += (audioIndex != _playlist.Count-1) ? _audioSpacing : 0;
				
				float currentTime = 0;
				while (currentTime < waitFor)
				{
					currentTime += Time.deltaTime;
					Scrub.value += Time.deltaTime/_playlistTotalLength;
					yield return null;
				}
			}
			audioIndex ++;
			audioOffset = 0;
		}
	}

    public void Disable()
    {
        _enabled = false;
        PauseButtonOnClick();
    }

    public void Enable()
    {
        _enabled = true;
    }
    
    public void MenuPause()
    {
        State = PlayerState.Paused;
    }

    public void MenuResume()
    {
        State = PlayerState.Playing;
    }

    public void SequencePause()
    {
        // No action needed.
    }

    public void SequenceResume()
    {
        // No action needed.
    }

    //-Component getters-----------------------------------------------------------------

    private Scrollbar _scrub;
	public Scrollbar Scrub {
		get { 
			if (_scrub == null)
				_scrub = transform.Find ("Scrub").GetComponent<Scrollbar> ();
			return _scrub;
		}}
	
	private Image _timeline;
	public Image Timeline {
		get {
			if (_timeline == null)
				_timeline = transform.Find ("Timeline").GetComponent<Image> ();
			return _timeline;
		}}
	
	private Image _timelineFill;
	public Image TimelineFill {
		get {
			if (_timelineFill == null)
				_timelineFill = Timeline.transform.Find ("Fill").GetComponent<Image> ();
			return _timelineFill;
		}}
	
	private CanvasGroup _fadeComponent;
	public CanvasGroup FadeComponent {
		get {
			if (_fadeComponent == null)
				_fadeComponent = GetComponent<CanvasGroup> ();
			return _fadeComponent;
		}}
	
	private Button _playButton;
	private Button PlayButton {
		get {
			if (_playButton == null)
				_playButton = Scrub.transform.Find ("Buttons").Find ("PlayButton").GetComponent<Button> ();
			return _playButton;
		}}
	
	private Button _pauseButton;
	private Button PauseButton {
		get {
			if (_pauseButton == null)
				_pauseButton = Scrub.transform.Find ("Buttons").Find ("PauseButton").GetComponent<Button> ();
			return _pauseButton;
		}}
	
	private Button _replayButton;
	private Button ReplayButton {
		get {
			if (_replayButton == null)
				_replayButton = Scrub.transform.Find ("Buttons").Find ("ReplayButton").GetComponent<Button> ();
			return _replayButton;
		}}
	/*
	private TextMeshProUGUI _replayCount;
	private string ReplayCount {
		set {
			if (_replayCount == null)
				_replayCount = ReplayButton.transform.Find ("Count").GetComponent<TextMeshProUGUI> ();
			_replayCount.text = value;
		}
		get {
			if (_replayCount == null)
				_replayCount = ReplayButton.transform.Find ("Count").GetComponent<TextMeshProUGUI> ();
			return _replayCount.text;
		}}
	*/
	private TextMeshProUGUI _timer;
	private TextMeshProUGUI Timer {
		get {
			if (_timer == null)
				_timer = Scrub.transform.Find ("Buttons").Find ("Timer").GetComponent<TextMeshProUGUI> ();
			return _timer;
		}}
}