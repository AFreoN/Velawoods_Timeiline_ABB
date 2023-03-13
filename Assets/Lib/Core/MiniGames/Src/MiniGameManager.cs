using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CoreSystem;

public class MiniGameManager : MonoSingleton<MiniGameManager> 
{
	private MiniGameCreator _creator; //handles creating objects and loading originals from resources
	private GameObject _currentMiniGame_Object; //which minigame is on screen
	private MiniGameBase _currentMiniGame_Script; //the current minigame script
    private string _currentMiniGame_Type; //the type of minigame currently active
    private string _currentMiniGame_WidgetType; //the widget type of minigame currently active
    private bool _enabled; //can we display minigames
	private Color _color1;
	private Color _color2;
	private Color _color3;
	private Color _UIcolor1;
    public bool UsingAnimatedBackground = false;
    public bool keepAnimatedBackgroundNext = false;
    public bool keepAnimatedBackgroundPrevious = false;
    public bool hasMinigameZoneDefined = false;


	public Color Color1{
		get{	return _color1;
		}
		set{_color1 = value;}
	}	

	public Color Color2
	{
		get{return _color2;}
		set{	_color2 = value;}
	}

	public Color Color3
	{
		get{return _color3;}
		set{_color3 = value;}
	}
	public Color UIColor1
	{
		get{return _UIcolor1;}
		set{_UIcolor1 = value;}
	}



	public MiniGameCreator Creator {
		get {
			return _creator;
		}
	}


	protected override void Init ()
	{
		base.Init ();

		_creator = Core.Instance.gameObject.GetComponent<MiniGameCreator> ();
        if (_creator == null) _creator = Core.Instance.gameObject.AddComponent<MiniGameCreator>();
        _creator.Init();

		_currentMiniGame_Object = null;

        _currentMiniGame_Type = "";

		CoreEventSystem.Instance.AddListener (CoreEventTypes.LEVEL_CHANGE, LevelChangedEvent);

		_enabled = true;
	}

	protected override void Dispose ()
	{
		CoreEventSystem.Instance.RemoveListener (CoreEventTypes.LEVEL_CHANGE, LevelChangedEvent);
		base.Dispose ();
	}

	private void LevelChangedEvent(object parameters)
	{
#if CLIENT_BUILD
        GameObject background = GameObject.Find("AnimatedBackground(Clone)");
        if (background != null)
        {
            background.GetComponent<AnimatedBackground>().Hide();
        }
#endif
        ClearMinigame();
	}

    public void CreateAnimatedBackground(bool deleteBackgroundOnEndNext, bool deleteBackgroundOnEndPrevious, Color backgroundColor)
    {
#if CLIENT_BUILD
        keepAnimatedBackgroundNext = deleteBackgroundOnEndNext;
        keepAnimatedBackgroundPrevious = deleteBackgroundOnEndPrevious;

        // Try and find an animated background in the current scene
        GameObject animatedBackground = GameObject.Find("AnimatedBackground(Clone)");
        if (animatedBackground == null)
        {
            // Create one if one did not already exist
            animatedBackground = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("AnimatedBackground"));
            if (animatedBackground != null)
            {
                LayerSystem.Instance.AttachToLayer(UILayers.Background.ToString(), animatedBackground);
            }
        }
        // Show the animated background
        animatedBackground.GetComponent<AnimatedBackground>().Show(backgroundColor);
#endif
    }

	public void TriggerMiniGame(string minigame_type, bool pause, GameObject[] mgParams)
	{
        if(_enabled)
        { 
		    if (_creator == null) 
				Init ();

			if(_currentMiniGame_Object != null) CoreHelper.SafeDestroy(_currentMiniGame_Object);

            _currentMiniGame_Object = _creator.Create(minigame_type);

		    if (_currentMiniGame_Object != null)
		    {
			    //widget has begun
                if(pause)
				    SequenceManager.Instance.Pause ();

				_currentMiniGame_Script = _currentMiniGame_Object.GetComponent<MiniGameBase>();
				_currentMiniGame_Script.SetDesignerData(mgParams);
		    } 
		    else 
		    {
			    //no widget begun
			    //nothing happens for now
				Debug.LogWarning("MinigameManager: Could not create a minigame for " + minigame_type);
		    }

			// MattB: Send MiniGame Start event
			CoreEventSystem.Instance.SendEvent(CoreEventTypes.MINIGAME_START, minigame_type);

			ClearMemory ();
        }
	}

    public void TriggerPause()
    {
		if(SequenceManager.Instance.IsPlaying &&
		   _currentMiniGame_Object != null)
			SequenceManager.Instance.Pause();
    }

    /// <summary>
    /// Creates a mini game but finds the type for you.
    /// Using the Activity Tracker class
    /// </summary>
	public void TriggerMiniGame(bool pause = true)
	{
		TriggerMiniGame (GetMiniGameType (), pause, new GameObject[]{});
	}

	
	public void TriggerMiniGame(string minigame_type, bool pause)
	{
		TriggerMiniGame(minigame_type, pause, new GameObject[]{});
	}

	public void TriggerMiniGame(GameObject[] mgParams, bool pause = true)
	{
		TriggerMiniGame(GetMiniGameType(), pause, mgParams);
	}

	/// <summary>
	/// Event passed to minigame when minigames happen over a period of timeline time
	/// </summary>
	public void ProgressMiniGame()
	{
		if(_currentMiniGame_Script != null)
		{
			_currentMiniGame_Script.ProgressMiniGame();
			return;
		}
		Debug.Log("Error progressing current minigame");
	}

    public GameObject CurrentGame
    {
        get { return _currentMiniGame_Object; }
    }

	/// <summary>
	/// This calls the current Minigame's End function.
	/// ONLY CALL IF YOU ARE NOT A MINIGAME!!! Minigames should call their own end function.
	/// </summary>
	/// <param name="unPause">If set to <c>true</c> un pause.</param>
    public void EndMinigame(bool unPause = true)
    {

        
        if (_currentMiniGame_Script)
			_currentMiniGame_Script.End (false);
		else
			// No minigame was loaded, skip to the end of this process
			CompletedMinigameEndTransition ();

		// TODO: Why is this here? Minigames should already unpause at the end of the transition.
		// Is there a reason they need to be unpaused before the end?
		if(unPause)
        	SequenceManager.Instance.Play();
    }

	public void MinigameHasEnded() {
		MinigameEndTransition();
     
    }

	public void MinigameEndTransition()
	{
		if(_currentMiniGame_Object != null) 
			_currentMiniGame_Script.EndTransition();
		else
			CompletedMinigameEndTransition();
	}

	private void ClearMinigame()
	{
		if(_currentMiniGame_Object != null) 
		{
			_currentMiniGame_Script.Clear();
			CoreHelper.SafeDestroy(_currentMiniGame_Object);
			_currentMiniGame_Object = null;
			_currentMiniGame_Script = null;
		}
       
		CoreEventSystem.Instance.SendEvent(CoreEventTypes.MINIGAME_END);


        _currentMiniGame_Type = "";

		ClearMemory ();
	}

	public void CompletedMinigameEndTransition()
	{
		ClearMinigame();
		SequenceManager.Instance.Play();
	}

    public string CurrentType
    {
        get { return _currentMiniGame_Type; }
    }

	public void SetEnabled(bool value)
	{
#if UNITY_EDITOR
		_enabled = value;
#endif
	}

	public string GetMiniGameType()
	{
        string activityid = Core.Instance._activityTracker.ActualActivityID;
        _currentMiniGame_Type = Database.Instance.GetActivityType(activityid);
        return _currentMiniGame_Type;
	}

    public string GetMiniGameWidgetType()
    {
        string activityid = Core.Instance._activityTracker.ActualActivityID;
        _currentMiniGame_WidgetType = Database.Instance.GetActivityWidgetType(activityid);
        return _currentMiniGame_WidgetType;
    }

    private void ClearMemory ()
	{
		Resources.UnloadUnusedAssets ();
        System.GC.Collect();
	}
}
