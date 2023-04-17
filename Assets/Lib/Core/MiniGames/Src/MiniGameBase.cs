using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace CoreSystem
{
    public class MiniGameBase : BaseListener, ISkippable
    {
        protected Tracker _tracker; //tracks information about how the user is doing

		protected List<Dictionary<string, string>> _mg_elementIDs;
		protected string _subType;

		//data is now broken in to element data structure so a list of elements. 
		//A element is a list of rows and a row is a dictionary
		protected List<MinigameSectionData> _data; 
		protected GameObject[] _designerAssignedData; //game objects the designer has assigned to use in the current minigame

		protected Image _bgImage;

		protected FadeCanvasGroup _fadeScript;

		private bool _ending = false;
		protected bool _skipping = false;
		protected bool _gamePaused = false; // Menu is showing

        protected virtual void Init()
        {
            
        }

        protected virtual void MiniGameLoop()
        {
            
        }

        protected virtual void TimerEnd(object obj)
        {
            Debug.Log("MiniGame: Timer Finished");

            //TODO The logic for when a timed minigame ends
        }

		/// <summary>
		/// Tells the minigame to skip. If it's already skipping it will ignore the command and return false.
		/// </summary>
		/// <param name="parameters">Parameters.</param>
		public virtual bool SendSkip(object parameters)
		{
			if(_skipping == false)
			{
				CoreEventSystem.Instance.SendEvent(CoreEventTypes.MINIGAME_SUCCESSFUL_SKIP);
				Skip (parameters);
				_skipping = true;

				if (_fadeScript == null)
				{
					_fadeScript = GetComponent<FadeCanvasGroup>();

					_fadeScript = gameObject.AddComponent<FadeCanvasGroup>();
					_fadeScript._canvasGroup = gameObject.AddComponent<CanvasGroup>();
				}
				_fadeScript.FadeTo(0.0f, 0);
				return true;
			}

			return false;
		}

        private void ForceSkip(object parameter)
        {
            SendSkip(parameter);
        }


        public virtual void Skip(object parameters)
        {
           
           
        }

#if CLIENT_BUILD
        public void Skipforward(object param)
        {

            GameObject background = GameObject.Find("AnimatedBackground(Clone)");
            if (background != null)
            {
                if (!MiniGameManager.Instance.keepAnimatedBackgroundNext)
                {
                    background.GetComponent<AnimatedBackground>().Hide();
                }
            }
        }
#endif

#if CLIENT_BUILD
        public void Skipbackwards(object param)
        {
            GameObject background = GameObject.Find("AnimatedBackground(Clone)");
            if (background != null)
            {
                if (!MiniGameManager.Instance.keepAnimatedBackgroundPrevious)
                {
                    background.GetComponent<AnimatedBackground>().Hide();
                }
            }

        }
#endif 

        private void GetTimer()
        {
            //TODO get if the activity is timed from the database
            List<Dictionary<string, string>> data = Database.Instance.Select("*", "ActivityFlag JOIN ActivityFlagType ON ActivityFlag.activityflagtypeid = ActivityFlagType.id",
                "ActivityFlagType.flagname = 'SetTime' AND ActivityFlag.activityid=" + ActivityTracker.Instance.ActualActivityID);

            if (data.Count == 1)
            {
                _tracker.SetEndTime(float.Parse(data[0]["flagvalue"]));
            }
        }

        public virtual void End(bool minigameComplete)
        {
            if (!_ending)
			{
				_ending = true;

				if (_tracker != null) 
				{
					_tracker.SetTimerActive (false);
				}

				if(minigameComplete)
				{
					CoreEventSystem.Instance.SendEvent (CoreEventTypes.MINIGAME_COMPLETE);
				}

                CoreEventSystem.Instance.SendEvent(CoreEventTypes.MINIGAME_END);

                // This block raycasts to stop buttons being clicked after end.
                GetComponent<CanvasGroup>().blocksRaycasts = false;
				MiniGameManager.Instance.MinigameHasEnded ();
			}
        }

		public virtual void EndTransition()
		{
			_fadeScript.FadeTo(0.0f);
			SendTransitionEnd(1.1f);
#if CLIENT_BUILD
            GameObject background = GameObject.Find("AnimatedBackground(Clone)");
            if (background != null)
            {
                if (!MiniGameManager.Instance.keepAnimatedBackgroundNext)
                {
                    background.GetComponent<AnimatedBackground>().Hide();
                }
            }
#endif
        }

		public virtual void SendTransitionEnd(float delay)
		{
			StartCoroutine("EndSignalDelay", delay);
		}

		private IEnumerator EndSignalDelay(object delay)
		{
			float delay_value = (float) delay;

			yield return new WaitForSeconds(delay_value);

			MiniGameManager.Instance.CompletedMinigameEndTransition();
		}

		/// <summary>
		/// Clear this instance. This minigame is one instruction away from being destroyed!
		/// </summary>
		public virtual void Clear() {

		}

		protected virtual void OnDestroy() {
			Debug.Log ("Destroying minigame, removing pause listeners");
#if CLIENT_BUILD
            CoreEventSystem.Instance.RemoveListener (MainMenu.Messages.MENU_SHOWING, PauseGame);
			CoreEventSystem.Instance.RemoveListener (MainMenu.Messages.MENU_HIDING, UnpauseGame);
            CoreEventSystem.Instance.RemoveListener(CoreEventTypes.ACTIVITY_SKIP, Skipforward);
            CoreEventSystem.Instance.RemoveListener(CoreEventTypes.ACTIVITY_REVERSE, Skipbackwards);
            CoreEventSystem.Instance.RemoveListener(CoreEventTypes.LEVEL_CHANGE, ForceSkip);

            // Close keyboard if it is still present on screen
            CoreEventSystem.Instance.SendEvent(CustomKeyboardScript.Messages.HIDE_KEYBOARD);
#endif
        }
	

		private void GetData()
		{
			_data = MinigameDataLoader.LoadMinigameData (ActivityTracker.Instance.ActualActivityID);
		}

		public virtual void SetDesignerData(GameObject[] designerAssignedData)
		{
			_designerAssignedData = designerAssignedData;
		}

		protected virtual void ParseData(List<MinigameSectionData> data)
		{

		}

		public virtual void ProgressMiniGame()
		{
			Progress ();
		}

		protected virtual void Progress()
		{

		}

		//Called by minigame every time user has failed to complete it correctly. Scoring system.
		protected void IncorrectAttempt()
		{
			CoreEventSystem.Instance.SendEvent (CoreEventTypes.MINIGAME_FAIL);
		}

		public virtual void ChangeBGColor(Color new_color)
		{
			if(LayerSystem.Instance.CheckIfForcedDimensions()) return;

            if (null != _bgImage)
            {
                _bgImage.color = new_color;
            }
		}

	    // Use this for initialization
	    void Start ()
        {
        	// HUG: remove previous background image if already set
			Image prevImage = gameObject.GetComponent<Image>();
			if(prevImage != null) 
				DestroyImmediate (prevImage);
			
			_fadeScript = GetComponent<FadeCanvasGroup>();

			if(_fadeScript == null)
			{
				_fadeScript = gameObject.AddComponent<FadeCanvasGroup>();
				_fadeScript._canvasGroup = gameObject.AddComponent<CanvasGroup>();
			}

			GetData ();

			if (GetComponent<Tracker> () == null) 
			{
				_tracker = gameObject.AddComponent<Tracker> ();
			}
			else
			{
				_tracker = GetComponent<Tracker>();
				_tracker.Init ();
				_tracker.AddEvent (Tracker.TRIGGER_TIMER_END, TimerEnd);
			}

			// SIMON: This has been moved to the Sequence Manager
			//CoreEventSystem.Instance.AddListener (CoreEventTypes.ACTIVITY_SKIP, InternalSkip);

			LayerSystem.Instance.AttachToLayer("MiniGames", gameObject);

            // TOM: Adds background image, but leaves it as clear if it has forced dimensions or an animated background
            _bgImage = gameObject.AddComponent<Image>();
            if (LayerSystem.Instance.CheckIfForcedDimensions() || MiniGameManager.Instance.UsingAnimatedBackground
                || MiniGameManager.Instance.hasMinigameZoneDefined)
            {
                // do not show the black background
                _bgImage.color = Color.clear;
            }
            else
            {
                // show the black background
                _bgImage.color = new Color(0, 0, 0, 0.7f);
            }
			
			gameObject.GetComponent<RectTransform> ().anchoredPosition = Vector2.zero;
			transform.localScale = Vector3.one;

            GetTimer();

            Init();

			ParseData (_data);

#if CLIENT_BUILD
            CoreEventSystem.Instance.AddListener (MainMenu.Messages.MENU_SHOWING, PauseGame);
			CoreEventSystem.Instance.AddListener (MainMenu.Messages.MENU_HIDING, UnpauseGame);
            CoreEventSystem.Instance.AddListener(CoreEventTypes.ACTIVITY_SKIP, Skipforward);
            CoreEventSystem.Instance.AddListener(CoreEventTypes.ACTIVITY_REVERSE, Skipbackwards);
            CoreEventSystem.Instance.AddListener(CoreEventTypes.LEVEL_CHANGE, ForceSkip);            
#endif

            CoreEventSystem.Instance.SendEvent (CoreEventTypes.MINIGAME_CREATED);
		}

		protected virtual void PauseGame( object p ) {
			_gamePaused = true;
		}
		
		protected virtual void UnpauseGame( object p ) {
			_gamePaused = false;
		}

		public void PlayAudio(AudioClip audio_clip)
		{
			AudioManager.Instance.PlayAudio(audio_clip, AudioType.Music);
		}

        public Tracker Tracker
        {
            get { return _tracker; }
        }

	    // Update is called once per frame
	    void Update () 
        {
			if(_tracker != null)
            	_tracker.UpdateTracker();

            MiniGameLoop();
	    }
    }
}