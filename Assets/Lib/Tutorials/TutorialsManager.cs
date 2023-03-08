using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CoreLib;


public class TutorialsManager : MonoSingleton<TutorialsManager> {

	/// <summary> Always play tutorials - ignore play history. </summary>
	public bool _alwaysPlay = false; 
	// All tutorial types
	public enum TutorialType 
	{
		Carnegie,
		CarnegiePoints,
		DragAndDrop,
		Gestures,
		MultipleChoiceSelect,
		MultipleChoiceSpeech,
		ReArrangeConversations,
		MediaAssets,
		FillTheGaps,
		None
	}
	
	public struct Messages 
	{
		public static string TUTORIAL_STARTED_PLAYING = "TutorialStartedPlaying";
	}
	
	// PlayerPrefs tags
	private string _seen   = "1"; // Tutorial has already been played
	private string _unseen = "0"; // Tutorial has never been played
	private string _nameTag= "tutorial"; // Name added to tutorial's name in playerprefs & serverside (TutorialType + _nameTag)

	/// <summary> UI Layer to attach the tutorial object to. </summary>
	private string _attachToLayer = 
#if CLIENT_BUILD
	UILayers.ImageViewer.ToString ();
#else
	"ImageViewer";
#endif
	/// <summary> Tutorial currently being played. Null when none. </summary>
	private GameObject _currentTutorial = null;
	/// <summary> When tutorial is being played, show prompt first or dive into the tutorial. </summary>
	private bool _showPromptFirst = true;
	/// <summary> Tutorial type currently being played. </summary>
	private TutorialType _currentTutorialType;
	/// <summary> Prefabs' path. Adding to it TutorialType.ToString () to load a particular tutorial. </summary>
	private string _prefabsPath = "Tutorial_"; // + TutorialType.ToString ()
	/// <summary> System timer </summary>
	private TutorialsTimer _timer;
	/// <summary> Timeout value for system timer </summary>
	private float _timerTimeout = 3; //sec

	//-Public Methods-------------------------------------------------------------------------

	/// <summary> Load tutorial type. Returns the tutorial's instance. </summary>
	public void ShowTutorial(TutorialType type, bool showPromptFirst = true)
	{
        // ** Series of events:
        // Start timer. 
        // Local: if tutorial not seen, launch at end of timer.
        // Try and get response from server
        // Server: if response from server before timer's end, kill timer and its events. Override local with server data. If unseen, launch tutorial. 
        // On tutorial being played, store locally and send to server

        if (type == TutorialType.None)
        {
            return;
        }

        // If Carnegie Points, check subscription, etc., and, if it's disabled, try to load next tutorial (if any)
        if (type == TutorialType.CarnegiePoints)
        {
#if CLIENT_BUILD
            if (PlayerProfile.Instance.SubscriptionType == PlayerProfile.Subscription.Premium || !PlayerProfile.SpeechAnalysisIsEnabled)
            {
#endif
                ShowTutorial(Resources.Load<Tutorial_Static>(_prefabsPath + type.ToString())._nextTutorial);
#if CLIENT_BUILD
            }
#endif
        }

		_currentTutorialType = type;
		_showPromptFirst = showPromptFirst;

		if (!_alwaysPlay)
		{
			bool playTutorialOnTimerEnd;
			if (PlayerPrefs.GetString(type.ToString()+_nameTag, _unseen) != _seen)
			{
				playTutorialOnTimerEnd = true;
			}
			else
			{
				playTutorialOnTimerEnd = false;
				Debug.Log ("TutorialsManager : Local : Tutorial <" + _currentTutorialType.ToString () + "> already seen.");
			}
			_timer = TutorialsTimer.StartTimer (_timerTimeout, OnTimerEnd, playTutorialOnTimerEnd);
			
#if CLIENT_BUILD
			SocketConnector.Instance.AddListener(VelaNetworkMessageTypes.USERDATA_STORE, ServerResponse);
			SocketConnector.Instance.SendMessage (VelaNetworkMessageTypes.USERDATA_GET, "{}", true);
#endif
		}
		else
		{
			PlayTutorial ();
		}
	}
	
	/// <summary> End currently playing tutorial (if any). </summary>
	public void EndTutorial ()
	{
		if (_currentTutorial != null)
        {
			_currentTutorial.GetComponent<Tutorial_Base> ().Exit ();
			CoreEventSystem.Instance.SendEvent(CoreEventTypes.TUTORIAL_HIDDEN);
        }
		else
        {
			Debug.Log ("Tutorial Manager : No tutorials currently in scene.");
		}
	}
	
	public void DestroyTutorial (TutorialType type)
	{
        if (_currentTutorial != null && _currentTutorial.GetComponent<Tutorial_Base>()._myType == type)
        {
            _currentTutorial.GetComponent<Tutorial_Base>().DestroyMe();
        }
	}

	public bool IsTutorialActive( TutorialType type ) {
		Tutorial_Base[] tutorials = GameObject.Find (_attachToLayer).GetComponentsInChildren<Tutorial_Base> ();
		foreach (Tutorial_Base tut in tutorials) {
			if ( tut._myType == type ) {
				return true;
			}
		}
		return false;
	}
	
	
//-Privates------------------------------------------------------------------------------

	private void PlayTutorial ()
	{
        // Catch if the tutorial is already active.
        if (!IsTutorialActive(_currentTutorialType))
        {
            string prefabPath = _prefabsPath + _currentTutorialType.ToString();
            _currentTutorial = Resources.Load<GameObject>(prefabPath);

            if (_currentTutorial != null)
            {
                _currentTutorial = GameObject.Instantiate(_currentTutorial);
                LayerSystem.Instance.AttachToLayer(_attachToLayer, _currentTutorial);

                //Debug.Log("Tutorial Manager : Initialising tutorial <" + _currentTutorialType.ToString() + ">");

                _currentTutorial.GetComponent<Tutorial_Base>()._myType = _currentTutorialType;
                _currentTutorial.GetComponent<Tutorial_Base>().Enter(_showPromptFirst);
            }
            else
            {
                Debug.LogWarning("Tutorial Manager : Prefab <" + prefabPath + "> not found!");
            }
        }
        else
        {
            // Debug log to help catch the reason why tutorials are being played more than once.
            UnityEngine.Debug.LogWarningFormat("TutorialManager::PlayTutorial() - {0} tutorial is trying to be played while it is already playing.\nStack trace:\n{1}", _currentTutorialType, StackTraceUtility.ExtractStackTrace());
        }
	}
	
	private void OnTutorialSeen (object parameter)
	{
		TutorialType tutorialType = (TutorialType) parameter;
	
		if (_currentTutorial == null) {
			Debug.LogWarning ("TutorialsManager : Ooops, something went wrong!");
			return;
		}
		if (PlayerPrefs.HasKey (tutorialType.ToString ()) == false) {
			Debug.LogWarning ("TutorialsManager : Ooops, something went wrong!");
			return;
		}
		
		#if CLIENT_BUILD
		SocketConnector.Instance.SendMessage (VelaNetworkMessageTypes.USERDATA_SAVE, ServerMessage (tutorialType.ToString() + _nameTag, _seen));
		#endif
		// Set tutorial in PlayerPrefs to _seen
		PlayerPrefs.SetString (tutorialType.ToString ()+_nameTag, _seen);
	}
	
	private void OnTimerEnd (bool playTutorial)
	{
        if (playTutorial)
        {
            PlayTutorial();
        }
		else
		{
			// Load next
			ShowTutorial (Resources.Load<Tutorial_Static> (_prefabsPath + _currentTutorialType.ToString())._nextTutorial);
		}
	
		//Stop listening for tutorial check from server
		#if CLIENT_BUILD
		SocketConnector.Instance.RemoveListener(VelaNetworkMessageTypes.USERDATA_STORE, ServerResponse);
		#endif
	}
	
	private void ServerResponse (object payload)
	{
      #if CLIENT_BUILD
      SocketConnector.Instance.RemoveListener(VelaNetworkMessageTypes.USERDATA_STORE, ServerResponse);
      #endif

        // Do nothing if timer finished counting
        if (_timer == null) return;
		_timer.StopTimer ();
	
		if (((JSONObject)payload).Count > 0)
		{
			int index = 0;
			foreach (string key in ((JSONObject)payload).keys)
			{
				if (key == _currentTutorialType.ToString().ToLower() + _nameTag)
				{
					string value = ((JSONObject)payload).list[index].ToString();
					if (value != "\"" + _seen + "\"")
					{
						// Override local and play
						PlayerPrefs.SetString (_currentTutorialType.ToString ()+_nameTag, _unseen);
						PlayTutorial ();
					}
					else
					{
						// Override local
						PlayerPrefs.SetString (_currentTutorialType.ToString ()+_nameTag, _seen);
						// Load next
						ShowTutorial (Resources.Load<Tutorial_Static> (_prefabsPath + _currentTutorialType.ToString())._nextTutorial);
					}
					return;
				}
				++index;
			}
		}
		// Override local and play if not existent in server message
		PlayerPrefs.SetString (_currentTutorialType.ToString ()+_nameTag, _unseen);
		PlayTutorial ();
	}
	
	private string ServerMessage (string field, string value)
	{
		return @"""" + field.ToLower () + @""":" + @"""" + value + @"""";
	}
	

//-Singleton------------------------------------------------------------------------------

	protected override void Init ()
	{
		base.Init ();
	
		// If tutorial types not stored in PlayerPrefs, store them as _unseen
		TutorialType types = new TutorialType ();
		for (int i=0; i<Enum.GetNames (typeof(TutorialType)).Length; i++) {
			if (PlayerPrefs.HasKey (types.ToString ()) == false) {
				PlayerPrefs.SetString (types.ToString (), _unseen);
			}
			types++;
		}

#if CLIENT_BUILD
        CoreEventSystem.Instance.AddListener (Messages.TUTORIAL_STARTED_PLAYING, OnTutorialSeen);
#endif
    }
	
	protected override void Dispose ()
	{
		base.Dispose ();

#if CLIENT_BUILD
        CoreEventSystem.Instance.RemoveListener (Messages.TUTORIAL_STARTED_PLAYING, OnTutorialSeen);
#endif
    }

    public void SyncWithServer()
    {
#if CLIENT_BUILD
        SocketConnector.Instance.AddListener(VelaNetworkMessageTypes.USERDATA_STORE, OnTutorialServerResponse);
        SocketConnector.Instance.SendMessage(VelaNetworkMessageTypes.USERDATA_GET);
#endif
    }

    private void OnTutorialServerResponse(object param = null)
    {
#if CLIENT_BUILD
        SocketConnector.Instance.RemoveListener(VelaNetworkMessageTypes.USERDATA_STORE, OnTutorialServerResponse);
        ReceivedUserData userData = JSONDataConverter.JSONToUserData(param);

        foreach (string tutorialType in Enum.GetNames(typeof(TutorialType)))
        {
            string keyName = (tutorialType + _nameTag);
            if (userData.entry.ContainsKey(keyName.ToLower()))
            {
                string value = userData.entry[keyName.ToLower()];
                value = value.Replace("\\", "").Replace("\"", "");

                PlayerPrefs.SetString(keyName, value);
            }
            else
            {
                PlayerPrefs.DeleteKey(keyName);
            }
        }
#endif
    }
}