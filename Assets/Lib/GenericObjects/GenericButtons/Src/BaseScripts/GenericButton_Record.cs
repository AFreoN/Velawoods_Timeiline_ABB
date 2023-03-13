//#define CARNEGIE_DEBUG_LOG

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

using CoreSystem;

[RequireComponent (typeof (CarnegieMicrophone))]
public class GenericButton_Record : GenericObject {

	public const string FORCE_CARNEGIE_SKIP = "ForceCarnegieSkip";

	[Header ("Carnegie Input")] [Space (10)]
	/// <summary> If this is false and there has been a recording, the button will remain in its 'analysing..' state forever - until someone will call CarnegieFail/Success/Error methods, overriding the server. </summary>
	public bool _sendToServer = true;
	public List<DialogueEventData.DialogueText> _carnegieInput;
	
	[Header("Send Carnegie Feedback To")] [Space(10)]
	public GameObject[] _listeners;

	[Header("Animation Flags")] [Space(10)]
	public bool _initOnStart = true;
	public bool _showLearnerImage = true;
	public bool _pulsateWhenIdle = true;
	public bool _allowRepeatOnSuccess = false; // Allow the user to try again on success.
	public bool _allowRepeatOnFail = true; // Allow the user to try again on fail.

	[Header("General Tweaks")] [Space(10)]
	public float _learnerIdleTime = 1.0f;
	public float _learnerFadeOutTime = 0.5f;
	public float _buttonClickedReactionTime = 0.35f;
	public float _buttonReleasedReactionTime = 0.45f;
	public float _feedbackReactionTime = 0.5f;
	public float _feedbackIdleTime = 1.5f;
	
	[Header("SubComponents")] [Space(10)]
	public GameObject _pulse;
	public GameObject _notifications;
	public GameObject _background;
	public GameObject _icon;
	public GameObject _analyser;
	public GameObject _feedback;
	public GameObject _learner;
	
	[HideInInspector] 
	/// <summary> Send Fail Core message on skipping (pressing the Next bubble dropdown) </summary>
	public bool _sendFailOnSkip = true;
	[HideInInspector] 
	/// <summary> On Show (), don't go past the learner's image. Also, when this is on, the Next dropdown will work, regardless of Interactable. </summary>
	public bool _disableRecording = false;
	
	// Skip Carnegie analysis flags
	protected bool _isDebugSkip = false; // Skips recording and Carnegie analysis
	
	// Microphone clip (updated at the release of the button)
	[HideInInspector]
	public AudioClip _micClip;
	
	// SFX files' paths
	protected string _correctAnswerAudio = "Audio/Correct_Answer";
	protected string _wrongAnswerAudio = "Audio/Wrong_Answer";
	protected string _analysingAudio = "Audio/Analysing_Carnegie_Loop";
	
	private bool _recording = false;
	private bool _interactable = false;
	public bool Interactable {
		get { return _interactable; }
		set { _interactable = value; }
	}
	
	// Messages
	public struct Messages 
	{
		/// <summary> Record button has just been touched. Method: RecordButtonTouched </summary>
		public const string RECORD_BUTTON_TOUCHED = "RecordButtonTouched";
		/// <summary> Record button has just been released. Method: RecordButtonReleased </summary>
		public const string RECORD_BUTTON_RELEASED = "RecordButtonReleased";
		
		/// <summary> Carnegie has been successful and record button has finished all animations. Method: RecordButtonOnComplete </summary>
		public const string RECORD_BUTTON_ON_COMPLETE = "RecordButtonOnComplete";
		/// <summary> Skipping Carnegie. Method: RecordButtonOnSkip </summary>
		public const string RECORD_BUTTON_ON_SKIP = "RecordButtonOnSkip";
		
		/// <summary> Carnegie Feedback, raw. Method: CarnegieFeedback (object[] (CarnegieXMLInterpreter.CarnegieFeedback)) </summary>
		public const string CARNEGIE_FEEDBACK = "CarnegieFeedback";
		/// <summary> Carnegie has been successful (Record button will finish all animations on RECORD_BUTTON_ON_COMPLETE). 
		/// Method: CarnegieSuccess (object[] (CarnegieXMLInterpreter.CarnegieFeedback)) </summary>
		public const string CARNEGIE_SUCCESS = "CarnegieSuccess";
		/// <summary> Carnegie has failed. Method: CarnegieFail (object[] (CarnegieXMLInterpreter.CarnegieFeedback)) </summary>
		public const string CARNEGIE_FAIL = "CarnegieFail";
		/// <summary> Carnegie error. Check parameter's internetConnection boolean to see if there's a data (unknown word) or 
		/// a connection problem. Method: CarnegieError (object[] (CarnegieXMLInterpreter.CarnegieFeedback)) </summary>
		public const string CARNEGIE_ERROR = "CarnegieError";
	}
	
#if UNITY_EDITOR_64
	public void Update () {
		if (Input.GetKeyDown (KeyCode.V)) {
#if CARNEGIE_DEBUG_LOG
            Debug.Log ("Record Button: Skip");
#endif
            StartCoroutine (DebugSkipRoutine (true));
		}
		if (Input.GetKeyDown (KeyCode.C)) {
#if CARNEGIE_DEBUG_LOG
            Debug.Log ("Record Button: Skip");
#endif
            StartCoroutine (DebugSkipRoutine (false));
		}
	}
#endif

	public void Start ()
	{	
		// Set RectTransform as being a square
		float width  = GetComponent<RectTransform> ().rect.width;
		float height = GetComponent<RectTransform> ().rect.height;
		if (Mathf.Abs (width-height) > 10)
		{
			if (width < height) GetComponent<RectTransform> ().SetSizeWithCurrentAnchors (RectTransform.Axis.Vertical, width);
			else                GetComponent<RectTransform> ().SetSizeWithCurrentAnchors (RectTransform.Axis.Horizontal, height);
		}
		
		if (_initOnStart)
		{
			Show (new object[] {0.0f, 0.0f});
		}
		
		// Debug skip
		CoreEventSystem.Instance.AddListener (FORCE_CARNEGIE_SKIP, SkipActivityButtonClicked);
	}
	// Debug skip
	private void OnDestroy() 
	{
		CoreEventSystem.Instance.RemoveListener (FORCE_CARNEGIE_SKIP, SkipActivityButtonClicked);
	}
	// Debug skip
	private void SkipActivityButtonClicked(object skipParams)
	{
		CoreEventSystem.Instance.SendEvent (CoreEventTypes.CARNEGIE_SKIP);
	
		StartCoroutine (DebugSkipRoutine ((bool)skipParams));
		
		OnSkip ();
	}
	
	
//-Interface---------------------------------------------------------------------------------------
	
	public override void Show (object[] paramList)
	{
		float lerpTime = (float) paramList [0];
	
		if (gameObject.activeSelf == false) gameObject.SetActive (true);	

		/*
		// HACK: Set the font size to 42
		TextMeshProUGUI notificationText = transform.Find ("Notifications/Message/BaseImage/TextMeshPro").GetComponent<TextMeshProUGUI> ();
		notificationText.fontSize = 42;
		*/

		StartCoroutine ("ShowRoutine", new object[] {lerpTime});
	}

	public override void Hide (object[] paramList)
	{
		base.Hide (paramList);
		
		StartCoroutine ("HideRoutine");
	}
	
	public virtual void Continue ()
	{
		Interactable = false;
	
		StartCoroutine (ContinueRoutine ());
	}
	
	public virtual void ButtonTouched ()
	{
		if (!Interactable || _recording) return;
		_recording = true;
		
		SendMesssageToListeners (Messages.RECORD_BUTTON_TOUCHED);
		
		StartCoroutine ("ButtonTouchedRoutine");
	}
	
	public virtual void ButtonReleased ()
	{
		if (!Interactable || !_recording) return;
		_recording = false;
	
		SendMesssageToListeners (Messages.RECORD_BUTTON_RELEASED);
		
		StartCoroutine ("ButtonReleasedRoutine");
	}
	
	public virtual void OnComplete ()
	{
		SendMesssageToListeners (Messages.RECORD_BUTTON_ON_COMPLETE);
	}
	
	public virtual void OnSkip ()
	{
		SendMesssageToListeners (Messages.RECORD_BUTTON_ON_SKIP);
	}
	
	public void AddCarnegieInput (string text, string[] carnegieText = null, bool isCorrect = true)
	{
		DialogueEventData.DialogueText data = new DialogueEventData.DialogueText ();
		
		data.text = text;
		data.isCorrect = isCorrect;
		data.carnegieText = carnegieText;
		
		_carnegieInput.Add (data);
	}
	
	public void ClearCarnegieInput ()
	{
		_carnegieInput.Clear ();
	}
	
	public List<DialogueEventData.DialogueText> GetCarnegieInput (bool isCorrect = true)
	{
		return _carnegieInput;
	}


//-"SendMessage" Receivers&Senders------------------------------------------------------------------
	
	public void CarnegieSuccess (CarnegieXMLInterpreter.CarnegieFeedback carnegieFeedback)
	{
		SendMesssageToListeners (Messages.CARNEGIE_SUCCESS, new object[] {carnegieFeedback});
		CoreEventSystem.Instance.SendEvent (CoreEventTypes.CARNEGIE_SUCCESS, carnegieFeedback);
	}
	
	public void CarnegieFail (CarnegieXMLInterpreter.CarnegieFeedback carnegieFeedback)
	{
		SendMesssageToListeners (Messages.CARNEGIE_FAIL, new object[] {carnegieFeedback});
		CoreEventSystem.Instance.SendEvent (CoreEventTypes.CARNEGIE_FAIL, carnegieFeedback);
	}
	
	public void CarnegieError (CarnegieXMLInterpreter.CarnegieFeedback carnegieFeedback)
	{
		SendMesssageToListeners (Messages.CARNEGIE_ERROR, new object[] {carnegieFeedback});
	}
	
	// Received from CarnegieInterpreter.cs with 
	// the final results of the analysis
	public void OnCarnegieComplete (object[] paramList)
	{
		CarnegieXMLInterpreter.CarnegieFeedback carnegieFeedback = (CarnegieXMLInterpreter.CarnegieFeedback) paramList [0];
		
		// Set audioClip as recorded by the mic, stored at Record Button's StopRecording () by _micClip.
		carnegieFeedback.micClip = _micClip;
		
		StartCoroutine (CarnegieFeedbackRoutine (carnegieFeedback));
		SendMesssageToListeners (Messages.CARNEGIE_FEEDBACK, new object[] {carnegieFeedback});
		
		if (carnegieFeedback.isError)
		{
			CarnegieError (carnegieFeedback);
		}
		else
		{
			if (carnegieFeedback.sentences != null && carnegieFeedback.sentences [carnegieFeedback.winningSentenceIndex].isCorrect == true)
			{
				// Sliders
				if (carnegieFeedback.sentences [carnegieFeedback.winningSentenceIndex].success == false)
				{
					CarnegieFail (carnegieFeedback);
				}
				else
				{
					CarnegieSuccess (carnegieFeedback);
				}
			}
			else
			{
				CarnegieFail (carnegieFeedback);
			}
		}
	}
	
	
//-Privates-----------------------------------------------------------------------------------------
	
	private void StartRecording (List<DialogueEventData.DialogueText> carnegieInput)
	{
		if (_isDebugSkip) return;
	
		GetComponent<CarnegieMicrophone> ().StartRecording (carnegieInput, _sendToServer);
	}
	
	private void StopRecording ()
	{
		if (_isDebugSkip) return;
	
		_micClip = GetComponent<CarnegieMicrophone> ().StopRecording ();
	}
	
	private string[] ListToString (List<string> list)
	{
		if (list != null && list.Count > 0)
		{
			string[] result = new string[list.Count];
			for (int i=0; i<list.Count; i++)
			{
				result [i] = list [i];
			}
			return result;
		}
		return null;
	}
	
	private void SendMesssageToListeners (string methodName, object[] paramList = null)
	{
		foreach (GameObject listener in _listeners)
		{
			listener.SendMessage (methodName, paramList, SendMessageOptions.DontRequireReceiver);
		}
	}
	
	private void CheckCarnegieInput ()
	{
		if (_carnegieInput == null || _carnegieInput.Count == 0)
		{
#if CARNEGIE_DEBUG_LOG
            Debug.LogWarning ("Record Button: Carnegie Input is empty!");
#endif
        }
		
		for (int i=0; i<_carnegieInput.Count; i++)
		{
			if (_carnegieInput [i].carnegieText == null || _carnegieInput [i].carnegieText.Length == 0)
			{
				DialogueEventData.DialogueText newCarnegieInput = _carnegieInput [i];
				newCarnegieInput.carnegieText = new string[] {_carnegieInput [i].text};
				_carnegieInput [i] = newCarnegieInput;
			}
		}
	}
	
	
//-Coroutines--------------------------------------------------------------------------------------
	
	protected IEnumerator ShowRoutine (object[] paramList)
	{
		float lerpTime = (float) paramList [0];
	
		// Initialise
		_pulse.SetActive (false);
		
		if (_showLearnerImage)
			_learner.SetActive (true);
			
		if (_icon.activeSelf == false)
			_icon.SetActive (true);
		
		// Bring on screen
		base.Show (new object[] {lerpTime, 0.0f});
		yield return new WaitForSeconds (lerpTime + 0.001f);
		
		if (_disableRecording) yield break;
		
		// Initialise on-screen routine
		_pulse.SetActive (true);
		
		// Start on-screen routine
		if (_showLearnerImage)
		{
			yield return new WaitForSeconds (_learnerIdleTime);
			_learner.GetComponent<GenericButton_Record_Learner> ().HideLearner (_learnerFadeOutTime, true);
		}
			
		if (_pulsateWhenIdle)
		{
			yield return new WaitForSeconds (_learnerFadeOutTime - _learnerFadeOutTime / 4.0f);
			_background.GetComponent<GenericButton_Record_ColorPulse> ().StartPulse ();
			_icon.GetComponent<GenericButton_Record_ColorPulse> ().StartPulse ();
			yield return new WaitForSeconds (_learnerFadeOutTime / 4.0f);
		}
		
		Interactable = true;
		//GetComponent<GenericButton_Record_Touch> ().enabled = true;
	}
	
	protected IEnumerator HideRoutine ()
	{
		_pulse.SetActive (false);
		GenericButton_Record_Feedback fee = _feedback.GetComponent<GenericButton_Record_Feedback> ();
		fee.FadeOut (_feedbackReactionTime / 2.0f);
		
		Interactable = false;
		//GetComponent<GenericButton_Record_Touch> ().enabled = false;
		
		yield return null;
	}
	
	protected IEnumerator ButtonTouchedRoutine ()
	{
		PlayAudio (_UISelectSound);
	
		CheckCarnegieInput ();
		
		StartRecording (_carnegieInput);
		
		_background.GetComponent<GenericButton_Record_ColorPulse> ().ToColor (_buttonClickedReactionTime, false);
		_icon.GetComponent<GenericButton_Record_ColorPulse> ().ToColor (_buttonClickedReactionTime, false);
		_pulse.GetComponent<GenericButton_Record_RecordPulse> ().StartPulse ();
		yield return null;
	}
	
	protected IEnumerator ButtonReleasedRoutine ()
	{
		Interactable = false;
		//GetComponent<GenericButton_Record_Touch> ().enabled = false;
		
		_pulse.GetComponent<GenericButton_Record_RecordPulse> ().StopPulse ();
		_icon.GetComponent<GenericButton_Record_ColorPulse> ().FadeOut (_buttonReleasedReactionTime / 2.0f, false);
		_analyser.GetComponent<GenericButton_Record_Analyser> ().StartSpin (_buttonReleasedReactionTime);
		
		PlayAudio (_analysingAudio, 0.4f);
		
		yield return new WaitForSeconds (_buttonClickedReactionTime / 2.0f + 0.01f);
		
		StopRecording ();
		
		yield return null;
	}
	
	protected IEnumerator ContinueRoutine ()
	{
		Interactable = false;
		
		if (_sendFailOnSkip)	
		{
			CoreEventSystem.Instance.SendEvent (CoreEventTypes.CARNEGIE_FAIL, new CarnegieXMLInterpreter.CarnegieFeedback ());
		}
		
		if (_disableRecording) 
		{
			yield return new WaitForSeconds (_feedbackReactionTime / 2.0f);
			OnComplete ();
			yield break;
		}
		
		//_analyser.GetComponent<GenericButton_Record_Analyser> ().StopSpin (_feedbackReactionTime / 2.0f);
		_background.GetComponent<GenericButton_Record_ColorPulse> ().ToColor (_feedbackReactionTime / 2.0f, true);
		
		yield return new WaitForSeconds (_feedbackReactionTime / 2.0f);
		
		_icon.GetComponent<GenericButton_Record_ColorPulse> ().ToColor (0.0f, true);
		_icon.GetComponent<GenericButton_Record_ColorPulse> ().FadeIn (_feedbackReactionTime / 2.0f);
		
		_learner.GetComponent<GenericButton_Record_Learner> ().ShowLearner (_feedbackReactionTime / 2.0f);
		yield return new WaitForSeconds (_feedbackReactionTime / 2.0f + 0.1f);
		_icon.SetActive (false);
		OnComplete ();
	}
	
	protected IEnumerator CarnegieFeedbackRoutine (CarnegieXMLInterpreter.CarnegieFeedback carnegieFeedback)
	{
		_analyser.GetComponent<GenericButton_Record_Analyser> ().StopSpin (_feedbackReactionTime / 2.0f);
		_background.GetComponent<GenericButton_Record_ColorPulse> ().ToColor (_feedbackReactionTime / 2.0f, true);
		
		// Stop analysing sound if still playing
		AudioManager.Instance.StopAudio (CoreSystem.AudioType.SFX);
		
		// Check success
		bool success;
		if (carnegieFeedback.isError) {
			success = false;
		}
		else {
			success = (carnegieFeedback.sentences [carnegieFeedback.winningSentenceIndex].isCorrect) ? carnegieFeedback.sentences [carnegieFeedback.winningSentenceIndex].success : false;
		}
		
		if (success)
		{
			PlayAudio (_correctAnswerAudio);
		
			_feedback.GetComponent<GenericButton_Record_Feedback> ().ShowTick (_feedbackReactionTime / 2.0f);
			yield return new WaitForSeconds (_feedbackReactionTime / 2.0f + _feedbackIdleTime);
			_feedback.GetComponent<GenericButton_Record_Feedback> ().FadeOut (_feedbackReactionTime / 2.0f);
			yield return new WaitForSeconds (_feedbackReactionTime / 2.0f);
			_icon.GetComponent<GenericButton_Record_ColorPulse> ().ToColor (0.0f, true);
			_icon.GetComponent<GenericButton_Record_ColorPulse> ().FadeIn (_feedbackReactionTime / 2.0f);
			
			if (_allowRepeatOnSuccess == false)
			{
				_learner.GetComponent<GenericButton_Record_Learner> ().ShowLearner (_feedbackReactionTime / 2.0f);
				yield return new WaitForSeconds (_feedbackReactionTime / 2.0f + 0.1f);
				_icon.SetActive (false);
				OnComplete ();
			}
			else
			{
				if (_pulsateWhenIdle)
				{
					yield return new WaitForSeconds (_feedbackReactionTime / 2.0f);
					_background.GetComponent<GenericButton_Record_ColorPulse> ().StartPulse ();
					_icon.GetComponent<GenericButton_Record_ColorPulse> ().StartPulse ();
				}
				Interactable = true;
				//GetComponent<GenericButton_Record_Touch> ().enabled = true;
			}
		}
		else
		{
			PlayAudio (_wrongAnswerAudio);
			
			if (carnegieFeedback.internetConnection)
				_feedback.GetComponent<GenericButton_Record_Feedback> ().ShowCross (_feedbackReactionTime / 2.0f);
			else
				_feedback.GetComponent<GenericButton_Record_Feedback> ().ShowNoWiFi (_feedbackReactionTime / 2.0f);
				
			yield return new WaitForSeconds (_feedbackReactionTime / 4.0f);
			_notifications.GetComponent<GenericButton_Record_Notifications> ().ShowMessage (carnegieFeedback.warningText);
			yield return new WaitForSeconds (_feedbackReactionTime / 2.0f + _feedbackIdleTime - _feedbackReactionTime / 4.0f);
			_feedback.GetComponent<GenericButton_Record_Feedback> ().FadeOut (_feedbackReactionTime / 2.0f);
			yield return new WaitForSeconds (_feedbackReactionTime / 2.0f);
			_icon.GetComponent<GenericButton_Record_ColorPulse> ().ToColor (0.0f, true);
			_icon.GetComponent<GenericButton_Record_ColorPulse> ().FadeIn (_feedbackReactionTime / 2.0f);

			if (_allowRepeatOnFail == false)
			{
				_learner.GetComponent<GenericButton_Record_Learner>().ShowLearner(_feedbackReactionTime / 2.0f);
				yield return new WaitForSeconds(_feedbackReactionTime / 2.0f + 0.1f);
				_icon.SetActive(false);
				OnComplete();
			}
			else
			{
				if (_pulsateWhenIdle)
				{
					yield return new WaitForSeconds(_feedbackReactionTime / 2.0f);
					_background.GetComponent<GenericButton_Record_ColorPulse>().StartPulse();
					_icon.GetComponent<GenericButton_Record_ColorPulse>().StartPulse();
				}
				Interactable = true;
				//GetComponent<GenericButton_Record_Touch> ().enabled = true;
			}
		}
	}
	
	protected IEnumerator DebugSkipRoutine (bool success)
	{
		// Set flag for not starting up Carnegie
		_isDebugSkip = true;
		
		// Check if button is supposed to be interactable
		//if (GetComponent<GenericButton_Record_Touch> ().enabled == false) yield break;
		//GetComponent<GenericButton_Record_Touch> ().enabled = false; // Set to false here, as it is normally set when the button is released
		
		if (Interactable == false) yield break;
		Interactable = false;

#if CARNEGIE_DEBUG_LOG
        if (Debug.isDebugBuild)
		Debug.Log ("REC BUTTON: SKIPPING");
#endif

        // Let any attached bubble know that the button has been touched
        foreach (GameObject listener in _listeners) 
			if (listener.GetComponent<GenericBubble> ()) 
				listener.GetComponent<GenericBubble> ().RecordButtonTouched ();
		
		// Fake Touch and Release
		StartCoroutine (ButtonTouchedRoutine ());
		yield return new WaitForSeconds (0.5f);
		StartCoroutine (ButtonReleasedRoutine ());
		yield return new WaitForSeconds (0.5f);
		
		// Set fake feedback
		CarnegieXMLInterpreter.CarnegieFeedback debugFeedback = new CarnegieXMLInterpreter.CarnegieFeedback ();
		
		debugFeedback.isError = false;
		debugFeedback.internetConnection = true;
		debugFeedback.micClip = null;
		
		// Get index of correct input
		for (int i=0; i<_carnegieInput.Count; i++) {
			if (_carnegieInput [i].isCorrect) {
				debugFeedback.winningSentenceIndex = i;
				break;
			}
		}
		// Set other
		debugFeedback.warningText = "Skipping...";
		debugFeedback.sentences = new List<CarnegieXMLInterpreter.CarnegieFeedbackPerSentence> ();
		// Fill sentences field with the above found correct answer
		for (int i=0; i<_carnegieInput.Count; i++)
		{
			CarnegieXMLInterpreter.CarnegieFeedbackPerSentence debugSentence = new CarnegieXMLInterpreter.CarnegieFeedbackPerSentence ();
			debugSentence.isCorrect = true;
			debugSentence.richText = _carnegieInput [debugFeedback.winningSentenceIndex].text;
			debugSentence.score = 1.0f;
			debugSentence.success = success;
			debugFeedback.sentences.Add (debugSentence);
		}
		// Send feedback
		OnCarnegieComplete (new object [] {debugFeedback});
		// Reset debug flag
		_isDebugSkip = false;
		
		OnSkip ();
	}
}