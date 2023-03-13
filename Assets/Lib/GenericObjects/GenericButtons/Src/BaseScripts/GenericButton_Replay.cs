using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using CoreSystem;

public class GenericButton_Replay : GenericObject {

	/*
	[Header ("Timer")]
	public GameObject _timer;
	private bool _showTimer = false;
	*/

	[Header ("Icon")]
	private GameObject _icon;
	public GameObject Icon {
		get {
			if (_icon == null)
				_icon = transform.Find ("Icon").gameObject;
			return _icon;
			}
	}
	
	[Header ("Listeners")]
	public GameObject[] _listeners;
	
	private static Color _iconColor = new Color (203.0f/255.0f, 17.0f/255.0f, 34.0f/255.0f, 1.0f);
	
	
	public void OnDestroy ()
	{
		StopAllCoroutines ();
	}
	
//-Generic Interface------------------------------------------------------------------------------------
	
	public override void ObjectSelected ()
	{
		foreach (GameObject listener in _listeners) {
			listener.SendMessage ("ReplayButtonPressed", SendMessageOptions.DontRequireReceiver);
		}
	}
	
	public override void Show (object[] paramList)
	{
		base.Show (paramList);
		
		//if (!_showTimer) _timer.SetActive (false);
	}
	
	public override void Hide (object[] paramList)
	{
		base.Hide (paramList);
		
		//HideTimer ();
	}

	public bool IsInteractable ()
	{
		return GetComponent<Button> ().interactable;
	}
	
	public virtual void Interactable (bool interactable, bool changeColour = true, float setBackAfter = 0.0f)
	{
		GetComponent<Button> ().interactable = interactable;
		
		if (changeColour)
			UITween.fadeTo (Icon, (interactable) ? _iconColor : Color.grey, 0.1f, UITween.UIFadeType.easeInSine, false);
			
		if (setBackAfter > 0)
		{
			StartCoroutine (SetInteractableAfterRoutine (!interactable, setBackAfter));
		}
	}
	
	public void SetInteractableAfter (bool interactable, float time)
	{
		StartCoroutine (SetInteractableAfterRoutine (interactable, time));
	}

	private IEnumerator SetInteractableAfterRoutine (bool interactable, float time)
	{
		yield return new WaitForSeconds (time);
		Interactable (interactable);
	}
	
	/*
//-OnTimerComplete---------------------------------------------------------------------------------------
	
	private void OnTimerComplete ()
	{
		StopTimer ();
		
		foreach (GameObject listener in _listeners)
		{
			listener.SendMessage ("OnReplayButtonTimerComplete", SendMessageOptions.DontRequireReceiver);
		}
		//Debug.Log ("Replay Button Timer Complete!");
	}
	

//-Timer Interface---------------------------------------------------------------------------------------

	public enum TimerState { STOPPED, PLAYING, PAUSED }
	private TimerState _timerState = TimerState.STOPPED;
	
	private float _timerLerpTime = 0.2f;
	private bool _timerIsHidden = true;

	private float _time;
	
	public virtual void StartTimer (float seconds = -1)
	{
		if (_timerState == TimerState.PLAYING)
		{
			//Debug.Log ("Cannot start timer, it is currently playing. Try stopping or pausing it first");
			return;
		}
		
		if (seconds <= 0)
		{
			OnTimerComplete ();
			return;
		}
		else
		{
			seconds = (seconds % 1 == 0) ? seconds - 0.01f : seconds;
		}
		
		if (_timerIsHidden)
			ShowTimer ();

		StartCoroutine ("TimerSequence", seconds);
		_timerState = TimerState.PLAYING;
	}
	
	public virtual void StopTimer ()
	{
		StopCoroutine ("TimerSequence");
		
		//if (!_timerIsHidden)
		HideTimer ();

		_time = 0.0f;
		
		_timerState = TimerState.STOPPED;
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
	
		
//-Privates---------------------------------------------------------------------------------------------------------

	private void HideTimer ()
	{
		if (!_showTimer) return;
		
		UITween.fadeTo (_timer, 0, _timerLerpTime, UITween.UIFadeType.easeInSine, false);
		_timerIsHidden = true;
	}
	
	private void ShowTimer ()
	{
		if (!_showTimer) return;
	
		UITween.fadeTo (_timer, 1, _timerLerpTime, UITween.UIFadeType.easeInSine, false);
		_timerIsHidden = false;
	}
	
	
	

//-Coroutines-------------------------------------------------------------------------------------------------------
	
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
			
			//-Update timer
			string display = ((int) (_time) + 1).ToString ();
			
			if (_showTimer)
			{
				if (_timer.GetComponent<TextMeshProUGUI> ().text != display)
				{
					_timer.GetComponent<TextMeshProUGUI> ().text = display;
				}
			}
		}
		
		//if (seconds > 1)
		//{
			OnTimerComplete ();
		//}
	}
	*/
}







































