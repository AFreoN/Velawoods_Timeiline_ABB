using UnityEngine;
using System.Collections;

public class GenericMisc_Timer : GenericObject {

	//-Public
	public GameObject[] _listeners;
	
	//-Private
	private float _originalTime = 0.0f;
	private float _time = 0.0f;
	
	private Color _timerColor = Color.red;
	private Color _greenColor = new Color (80.0f/255.0f, 122.0f/255.0f, 1.0f/255.0f);
	private Color _amberColor  = new Color (255.0f/255.0f, 168.0f/255.0f, 0);
	private Color _redColor  = new Color (195.0f/255.0f, 0.0f/255.0f, 21.0f/255.0f);
	
	private float _colorLerpTime = 0.4f;
	
	private enum ColorState { RED, AMBER, GREEN }
	private ColorState _colorState = ColorState.RED;
	
	public enum TimerState { STOPPED, PLAYING, PAUSED }
	private TimerState _timerState = TimerState.STOPPED;
	
	
//-Interface-----------------------------------------------------------------------------------------
	
//-Available methods: Show (object[]{float}), Hide (object[]{float}), StartTimer (float), StopTimer (), PauseTimer (), ResumeTimer ()	
//-When completed, the code sends a message to any listener in _listeners: "OnTimerComplete ()"

	public virtual void SetTimer (float seconds)
	{
		_originalTime = seconds;
		
		if (seconds <= 0)
		{
			StopTimer ();
			return;
		}
		
		_originalTime = seconds;
		_time = seconds;
		
		UpdateTimerColor ();
		UpdateTimer ();
	}

	public virtual void StartTimer (float seconds = -1)
	{
		if (_timerState == TimerState.PLAYING)
		{
			Debug.Log ("Cannot start timer, it is currently playing. Try stopping or pausing it first");
			return;
		}
		
		if (seconds >= 0)
			_originalTime = seconds;
		
		StartCoroutine ("TimerSequence", seconds);
		_timerState = TimerState.PLAYING;
	}
	
	public virtual void StopTimer ()
	{
		StopCoroutine ("TimerSequence");
		
		_originalTime = 0.0f;
		_time = 0.0f;
		
		_timerState = TimerState.STOPPED;
		
		UpdateTimer ();
	}
	
	public virtual void PauseTimer ()
	{
		if (_timerState != TimerState.PLAYING)
		{
			Debug.Log ("Cannot pause timer, it is not playing.");
			return;
		}
		
		StopCoroutine ("TimerSequence");
		_timerState = TimerState.PAUSED;
	}
	
	public virtual void ResumeTimer ()
	{
		if (_timerState != TimerState.PAUSED)
		{
			Debug.Log ("Cannot resume timer, it is not paused");
			return;
		}
		
		StartCoroutine ("TimerSequence", _time);
		_timerState = TimerState.PLAYING;
	}
	
	public TimerState GetState ()
	{
		return _timerState;
	}
	
//-Messaging-----------------------------------------------------------------------------------------------------------------
	
	protected virtual void OnComplete ()
	{
		//StopTimer ();
		_timerState = TimerState.STOPPED;
		
		foreach (GameObject listener in _listeners) {
			listener.SendMessage ("OnTimerComplete", SendMessageOptions.DontRequireReceiver);
		}
	}
	
	
//-Privates--------------------------------------------------------------------------------
	
	private void UpdateTimerColor ()
	{
		if (_timerState == TimerState.STOPPED && _colorState != ColorState.RED)
		{
			ChangeColorTo (ColorState.RED);
		}
		else
			if (_timerState == TimerState.PLAYING)
		{
			if (_time >= (_originalTime / 3.0f) * 2.0f && _time < _originalTime) {
				if (_colorState != ColorState.GREEN) {
					ChangeColorTo (ColorState.GREEN);
				}
			}
			else if(_time >= _originalTime / 3.0f && _time < (_originalTime / 3.0f) * 2.0f) {
				if (_colorState != ColorState.AMBER) {
					ChangeColorTo (ColorState.AMBER);
				}
			}
			else
			{
				if (_colorState != ColorState.RED) {
					ChangeColorTo (ColorState.RED);
				}
			}
		}
	}
	
	private void ChangeColorTo (ColorState colorState)
	{
		StopCoroutine ("LerpTimerColorTo");
		StartCoroutine ("LerpTimerColorTo", colorState);
	}
	
	private void UpdateTimer ()
	{
		UpdateTimerColor ();
		
		for (int i=0; i<transform.childCount; i++)
		{
			if (transform.GetChild (i).GetComponent<GenericMisc_Timer_ComponentBase> ())
				transform.GetChild (i).GetComponent<GenericMisc_Timer_ComponentBase> ().TimerUpdate (_time, _originalTime);
		}
	}
	
	private void ColorUpdate ()
	{
		for (int i=0; i<transform.childCount; i++)
		{
			if (transform.GetChild (i).GetComponent<GenericMisc_Timer_ComponentBase> ())
				transform.GetChild (i).GetComponent<GenericMisc_Timer_ComponentBase> ().ColorUpdate (_timerColor);
		}
	}
	
//-Coroutines-------------------------------------------------------------------------------
	
	private IEnumerator TimerSequence (float seconds)
	{
		float initialTime = seconds;
		_time = seconds;
		
		float currentTime = 0;
		while (currentTime/seconds < 1)
		{
			currentTime += Time.deltaTime;
			
			//-Timer
			_time = initialTime - (currentTime/seconds) * initialTime;
			if (_time <= 0) _time = 0;
			
			yield return null;
			UpdateTimer ();
		}
		
		if (seconds > 1)
		{
			OnComplete ();
		}
	}
	
	private IEnumerator LerpTimerColorTo (ColorState colorState)
	{
		_colorState = colorState;
		
		Color initialColor = _timerColor;
		Color finalColor;
		if (_colorState == ColorState.AMBER)
			finalColor = _amberColor;
		else if (_colorState == ColorState.GREEN)
			finalColor = _greenColor;
		else
			finalColor = _redColor;
		
		float currentTime = 0;
		while (currentTime/_colorLerpTime < 1)
		{
			currentTime += Time.deltaTime;
			
			_timerColor = Color.Lerp (initialColor, finalColor, currentTime/_colorLerpTime);
			
			yield return null;
			ColorUpdate ();
		}
		_timerColor = finalColor;
		ColorUpdate ();
	}
}
