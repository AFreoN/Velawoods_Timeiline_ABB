using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace CoreSystem
{
	public class Core : MonoBehaviour
	{
		//Add Scripts
        public ActivityTracker _activityTracker; //tracks progress in an activity
        public Database _database; //has the data needed
        public MiniGameManager _minigameManager; //handles creating/ending minigames
        public SequenceManager _sequenceManager; //handles pausing/playing the main timeline
        public LayerSystem _layerSystem; //handles canvases and 2d ui elements
        public ComponentManager _componentManager; //handles predefined UI Elements
        public TouchManager _touchManager; //handles where the user is touching and what objects are waiting to be touched
		public CoreEventSystem _coreEventSystem; //handles events sent globally across objects
		public AudioManager _audioManager; //plays audio
		public SocketConnector _socketConnector; // Connects to the server
		public ContentManager _contentManager; // Manages content and localisation - get your strings from here
        public RouteGames.PauseManager _pauseManager; // Manages the games global pause state

		public GameObject _entryPointObject; //used for game specific init stuff
		public GameObject _sequence; //do you wish to have a sequence that you choose
		public bool _fakeLogin = true;

        // Bool to determine as to whether the user is playing the mission as a try
        public bool userTryMode = false;

		//Core Stuff
		private static Core _instance;

		void Awake()
		{
			#if DEBUG
			Debug.Log("Core: Init()");
			Debug.Log("Core: Game is in Debug Mode");
			#endif

			_instance = Core.Instance;

#if UNITY_IOS && CLIENT_BUILD
			// If iOS, add the Memory Manager component if not alread present.
			if(GetComponent<iOSMemoryManager>() == null)
			{
				Debug.Log ("Core::Awake() - iOS Memory Manager component added.");
				gameObject.AddComponent<iOSMemoryManager>();
			}
#endif
		}

		public static Core Instance
		{
			get
			{
				if(_instance == null)
				{
					_instance = GameObject.FindObjectOfType<Core>();

                    //this stops warnings from unity doing it this way
                    _instance._activityTracker = ActivityTracker.Instance; 
                    _instance._database = Database.Instance;
                    _instance._minigameManager = MiniGameManager.Instance;
                    _instance._sequenceManager = SequenceManager.Instance;
                    _instance._layerSystem = LayerSystem.Instance;
					_instance._componentManager = ComponentManager.Instance;
                    _instance._touchManager = TouchManager.Instance;
					_instance._coreEventSystem = CoreEventSystem.Instance;
					_instance._audioManager = AudioManager.Instance;
					_instance._socketConnector = SocketConnector.Instance;
					_instance._contentManager = ContentManager.Instance;
                    _instance._pauseManager = RouteGames.PauseManager.Instance;

                    CoreEventSystem.Instance.AddListener(CoreEventTypes.MISSION_SETUP, SequenceManager.Instance.MissionSetUpCallback);

					if(_instance._entryPointObject != null &&
					   _instance._entryPointObject.GetComponent<BaseEntryPoint>() != null)
					{
						_instance._entryPointObject.GetComponent<BaseEntryPoint>().Init( );
						if ( _instance._fakeLogin ) _instance._coreEventSystem.SendEvent( CoreEventTypes.LOGIN_SUCCESS );
					}

					DontDestroyOnLoad(_instance.gameObject);
                    DontDestroyOnLoad(_instance._layerSystem.MainCanvas);
				}
				else
				{
					Core[] core_objects = GameObject.FindObjectsOfType<Core>();

					foreach(Core core_obj in core_objects)
					{
						if(core_obj.gameObject != _instance.gameObject)
						{
							CoreHelper.SafeDestroy(core_obj.gameObject);
						}
					}
				}

				return _instance;
			}
		}

        int FramesToWait = 0;
        bool DeferredGarbageCollect = false;

        public void TriggerDeferredGarbageCollect(int framesToWait)
        {
            DeferredGarbageCollect = true;
            
            if (framesToWait > FramesToWait)
            {
                FramesToWait = framesToWait;
            }
        }

		void Update() 
        {
#if DEBUG_SINGLETON
            if(Input.GetKeyDown(KeyCode.A))
            {
                foreach (string classname in SingletonDebug.ClassNames)
                {
                    Debug.Log(classname);
                }
            }
#endif
            if(DeferredGarbageCollect)
            {
                if (FramesToWait <= 0)
                {
                    Resources.UnloadUnusedAssets();
                    System.GC.Collect();
                    DeferredGarbageCollect = false;
                }

                --FramesToWait;
            }

			if (_instance._socketConnector != null) {
				_instance._socketConnector.Update ();
			}

			if(_instance._entryPointObject != null &&
			   _instance._entryPointObject.GetComponent<BaseEntryPoint>() != null)
			{
				_instance._entryPointObject.GetComponent<BaseEntryPoint>().Update();
			}

            //Some devices don't clear stencil buffer correctly meaning masks may not function properly. We manually clear it here.
            GL.Clear(true, false, Color.black);
        }
		
		/// <summary>  Exit the game. Listen for 'CoreEventTypes.ON_APPLICATION_QUIT (bool isStandalone)' for confirmation right before the app is killed.  </summary>
		public static void QuitApplication ()
		{
			// Standalone
			if (!Application.isEditor) 
			{
#if UNITY_STANDALONE_WIN
				// Windows
				Application.Quit();
#else
				// Mobile
				Application.Quit();
#endif
			}
			// Editor
			else 
			{
#if UNITY_EDITOR
				UnityEditor.EditorApplication.isPlaying = false;
#endif
			}
		}
		
		void OnApplicationPause (bool pause)
		{
			bool isStandalone = (Application.isEditor) ? false : true;
			
			string messageType = (pause) ? CoreEventTypes.ON_APPLICATION_PAUSE : CoreEventTypes.ON_APPLICATION_RESUME;
			CoreEventSystem.Instance.SendEvent (messageType, isStandalone);
		}
		
		/// <summary> 'Exiting the app' confirmation message. </summary>		
		void OnApplicationQuit ()
		{
			// Standalone
			if (!Application.isEditor) 
			{
#if UNITY_STANDALONE_WIN
				// This gets rid of windows error messages resulting from quitting in high resolutions
				PlayerPrefs.Save();
				System.Diagnostics.ProcessThreadCollection pt = System.Diagnostics.Process.GetCurrentProcess().Threads;
				foreach (System.Diagnostics.ProcessThread p in pt)
				{
					p.Dispose();
				}
#endif
			}

			bool isStandalone = (Application.isEditor) ? false : true;
			CoreEventSystem.Instance.SendEvent (CoreEventTypes.ON_APPLICATION_QUIT, isStandalone);
		}

		/// <summary> Reset() all singleton instances, kill Core. </summary>
		public static void Reset() 
		{
			// Reset all subcomponents
#if CLIENT_BUILD
			// non-core singletons
            NetworkScoreManager.Instance.Reset();
            SU_MyNotes_FileManager.Instance.Reset();
            Shop.Instance.Reset();
            UserWarning.Instance.Reset();
			ScoringSystemCreator.Instance.Reset();
			PracticeActivityManager.Instance.Reset();
			AnalyticsManager.Instance.Reset();
			DeviceCapabilities.Instance.Reset();
			ConnectivityManagerLocator.Instance.Reset();
			CourseTest_SpriteInserter.Instance.Reset();
			EncryptedFileManager.Instance.Reset();
			NetworkCertificatesManager.Instance.Reset();
			NetworkTestCompletionManager.Instance.Reset();
			NetworkBookmarkManager.Instance.Reset();
			UnsentNetworkMessages.Instance.Reset();
#endif
			// core singletons
			HTMLTextEntities.Instance.Reset();
      
            ConversationManager.Instance.Reset();
            
            ComponentManager.Instance.Reset();
            TutorialsManager.Instance.Reset();
            StorageManager.Instance.Reset();
            
            TouchManager.Instance.Reset();
            
            ContentManager.Instance.Reset();
            
            Database.Instance.Reset();
            LayerSystem.Instance.Reset();
            AudioManager.Instance.Reset();
            EncryptionManager.Instance.Reset();
            ObjectStateManager.Instance.Reset();

            // CoreEventSystem
            TutorialsManager.Instance.Reset();
            // CoreEventSystem
            ActivityTracker.Instance.Reset();
            // CoreEventSystem
            SequenceManager.Instance.Reset();
            // CoreEventSystem
            MiniGameManager.Instance.Reset();

            RouteGames.PauseManager.Instance.Reset();

            // SocketConnector CoreEventSystem
#if CLIENT_BUILD
            PlayerProfile.Instance.Reset();
#endif

            // Core
            AssetBundler_WWWHandler.Instance.Reset();

            SocketConnector.Instance.Reset();
            CoreEventSystem.Instance.Reset();

			// Destroy core
			Destroy (_instance);
			_instance = null;
		}
	}
}
