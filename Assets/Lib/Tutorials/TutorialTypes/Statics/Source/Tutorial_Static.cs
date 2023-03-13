using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using CoreSystem;

public class Tutorial_Static : Tutorial_Base {

	// Set this if a second tutorial is needed after this one (ex: Carnegie -> SpeechAnalysisPoints)
	public TutorialsManager.TutorialType _nextTutorial = TutorialsManager.TutorialType.None;

	// Lerping values

	protected float _tutorialEnterTime = 1.5f; // Time spent lerping tutorial window in
	protected float _tutorialExitTime = 0.8f;  // Time spent lerping tutorial window out
	
	protected float _promptSlideTime = 0.7f; // Time spent sliding the prompt button
	protected float _promptWaitTime = 6.0f;  // Time spent waiting for the user to click the prompt button

	protected bool  _promptClicked = false;
	protected float _contentSlideTime = 0.5f; // Time spent sliding content on ContinueButtonClicked ()

	private System.Timers.Timer m_tutorialServerTimer;

	// Sounds
	protected string _selectSound = "Audio/UI_Select";
	protected string _swooshSoundIn = "Audio/UI_Slide_1";
	protected string _swooshSoundOut = "Audio/UI_Slide_2";
	
	// Properties & Components
	
	private bool _interactable = true;
	private bool Interactable {
		get { return _interactable; }
		set {_interactable = value; }
	}
	
	private Tutorial_Static_Prompt _prompt;
	private Tutorial_Static_Prompt Prompt {
		get {
			if (_prompt == null)
				_prompt = transform.Find ("Prompt").GetComponent<Tutorial_Static_Prompt> ();
			return _prompt;
		} }
	
	private Image _background;
	private Image Background {
		get {
			if (_background == false)
				_background = GetComponent<Image> ();
			return _background;
		} }
	
	private Tutorial_Static_TutorialWindow _tutorial;
	private Tutorial_Static_TutorialWindow Tutorial {
		get {
			if (_tutorial == null)
				_tutorial = transform.Find ("Tutorial").GetComponent<Tutorial_Static_TutorialWindow> ();
			return _tutorial;
		} }
	
	// Set this to true for testing purposes. False for build.
	//private bool _testingMode = false;
	
//-Init---------------------------------------------------------------------------------------------------------------------------
	
	public void Awake ()
	{
		Background.enabled = false;
		Tutorial.gameObject.SetActive (false);

		CoreEventSystem.Instance.AddListener (CoreEventTypes.MINIGAME_COMPLETE, DestroyGameObject);
	}
	
	public void OnDestroy ()
	{	
		TouchZone._isEnabled = true;
		CoreEventSystem.Instance.RemoveListener (CoreEventTypes.MINIGAME_COMPLETE, DestroyGameObject);
		iTween.Stop (gameObject, true);
	}
	
	
//-Interface-----------------------------------------------------------------------------------------------------------------------------
	
	public override void Enter (bool showPromptFirst)
	{
		if (showPromptFirst)
			StartCoroutine ("PromptRoutine");
		else
			OnPromptClicked ();
	}
	
	public override void Exit ()
	{	
		Interactable = false;
	
		UITween.fadeTo (Background.gameObject, 0.0f, _tutorialExitTime/2.0f, UITween.UIFadeType.easeInSine, false);
		Tutorial.Exit (_tutorialExitTime);
		
		PlayAudio (_selectSound);
		PlayAudio (_swooshSoundOut, _tutorialExitTime / 3.0f);
		
		//Destroy (gameObject, _tutorialExitTime + 0.01f);
		StartCoroutine (Deactivate (_tutorialExitTime + 0.01f));
	}

	private void DestroyGameObject(object param)
	{
		if (gameObject)
			Destroy (gameObject, _tutorialExitTime + 0.01f);
	}
	
	
//-Buttons---------------------------------------------------------------------------------------------------------------------------
	
	public void OnPromptClicked ()
	{
		if (!Interactable) return;
	
		if (_promptClicked) return;
		_promptClicked = true;
		
		TouchZone._isEnabled = false;
		
		OnTutorialSeen ();
		
		StartCoroutine (SetInteractableFor (false, _tutorialEnterTime));
		
		PlayAudio (_selectSound);
		StopCoroutine ("PromptRoutine");
		Prompt.SwitchColors (0.2f);
		Prompt.Exit (_promptSlideTime);
		
		PlayAudio (_swooshSoundOut, _tutorialEnterTime);
		Tutorial.Init ();
		Tutorial.Enter (_tutorialEnterTime);
		Tutorial.Next (_contentSlideTime, _tutorialEnterTime);
		
		Background.enabled = true;
		UITween.fadeTo (Background.gameObject, 0.7f, _tutorialEnterTime/2.0f, UITween.UIFadeType.easeInSine, false);
	}
	
	public void OnContinueButtonClicked ()
	{
		if (!Interactable) return;
		
		PlayAudio (_selectSound);
		PlayAudio (_swooshSoundOut);
		
		StartCoroutine (SetInteractableFor (false, _contentSlideTime));
		Tutorial.Next (_contentSlideTime);
	}
	
	public void OnExitButtonClicked ()
	{
		if (!Interactable) return;
		
		TouchZone._isEnabled = true;
		
		Exit ();
	}
	
	private IEnumerator Deactivate(float waitTime)
	{
		if (waitTime>0)
			yield return new WaitForSeconds (waitTime);
		// Set active
		gameObject.SetActive (false);
		TouchZone._isEnabled = true;

		if (_nextTutorial != TutorialsManager.TutorialType.None)
			TutorialsManager.Instance.ShowTutorial (_nextTutorial);
	}
	
	
//-Privates-----------------------------------------------------------------------------------------------------------------------------

	private void PlayAudio (string audioPath, float waitTime = 0)
	{
		AudioManager.Instance.PlayAudio (audioPath, CoreSystem.AudioType.SFX, 1, waitTime);
	}

	
//-Coroutines---------------------------------------------------------------------------------------------------------------------------
	
	private IEnumerator PromptRoutine ()
	{
		AudioManager.Instance.PlayAudio (_swooshSoundIn);
		Prompt.Enter (_promptSlideTime);
		yield return new WaitForSeconds (_promptWaitTime);
		Interactable = false;
		Prompt.Exit (_promptSlideTime);
		yield return new WaitForSeconds (_promptSlideTime + 0.01f);
		//Destroy (gameObject);
		StartCoroutine (Deactivate(0));
	}
	
	private IEnumerator SetInteractableFor (bool interactable, float time)
	{
		float currentTime = 0;
		while (currentTime < time)
		{
			currentTime += Time.deltaTime;
			Interactable = interactable;
			yield return null;
		}
		Interactable = !interactable;
	}
}






























