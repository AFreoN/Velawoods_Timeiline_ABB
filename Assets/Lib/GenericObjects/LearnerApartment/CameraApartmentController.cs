using UnityEngine;
using System.Collections;

public class CameraApartmentController : MonoBehaviour {
	
	public const string MOVE_OBJECT_TOUCHED = "MoveObjectTouched";
	public const string FREE_SPIN_ACTIVE = "FreeSpinActive";

	private ApartmentCameraComponent _currentCameraAction;
	private bool _menuActive;
	private Stack _actions;
	private bool _exitingApartment;

	GameObject mainCamera;

	void Awake()
	{
		CoreEventSystem.Instance.AddListener (MOVE_OBJECT_TOUCHED, ObjectTouched);
		CoreEventSystem.Instance.AddListener (FREE_SPIN_ACTIVE, FreeSpinActive);
#if CLIENT_BUILD
        CoreEventSystem.Instance.AddListener (MainMenu.Messages.MENU_SHOWING, MenuActive);	
		CoreEventSystem.Instance.AddListener (MainMenu.Messages.MENU_HIDING, MenuInactive);
#endif
		CoreEventSystem.Instance.AddListener (CoreEventTypes.LEVEL_CHANGE, SceneChanged);

		mainCamera = Camera.main.gameObject;
		//Snap to position of camera for first frame.
		ChangeState (new SnapToCameraComponent());
		//After that keep up with any camera animations that are going on.
		ChangeState (new BlendToAnimComponent ());
	}

	void OnDestroy()
	{
		CoreEventSystem.Instance.RemoveListener (MOVE_OBJECT_TOUCHED, ObjectTouched);
		CoreEventSystem.Instance.RemoveListener (FREE_SPIN_ACTIVE, FreeSpinActive);
#if CLIENT_BUILD
        CoreEventSystem.Instance.RemoveListener (MainMenu.Messages.MENU_SHOWING, MenuActive);	
		CoreEventSystem.Instance.RemoveListener (MainMenu.Messages.MENU_HIDING, MenuInactive);
#endif
        CoreEventSystem.Instance.RemoveListener (CoreEventTypes.LEVEL_CHANGE, SceneChanged);
	}

	void Start () 
	{
		_actions = new Stack ();
		_menuActive = false;
		_exitingApartment = false;
		ChangeState (new BlendToAnimComponent ());
		//ChangeState (new FreeLookComponent ());
	}

	private void SceneChanged(object sceneChangedTo)
	{
        MissionStateData newScene = (MissionStateData)sceneChangedTo;

        //If we are switching scene and not moving to the apartment scene then lock out command changes and just pan until new scene is loaded
        if (newScene.sceneName != MissionStateData.APARTMENT_SCENE)
        {
            ChangeState(new CameraPanComponent());
            _exitingApartment = true;
        }
    }

	public void ObjectTouched(object touchParams)
	{
		ChangeState (new BlendToAnimComponent ());
	}

	public void FreeSpinActive(object parameters)
	{
		if(_menuActive)
		{
			ChangeState (new CameraPanComponent ());
		}
		else
		{
			ChangeState (new FreeLookComponent ());
		}
	}

	public void MenuActive(object parameters)
	{
		_menuActive = true;
		SaveCurrentState ();
		ChangeState (new CameraPanComponent ());
	}

	public void MenuInactive(object parameters)
	{
		if(RestoreState() == false)
		{
			ChangeState(new FreeLookComponent());
		}
		_menuActive = false;
	}

	void Update() 
	{
		_currentCameraAction.Update ();
	}

	private void ChangeState(ApartmentCameraComponent newState)
	{
		//Unable to change state now as we are going into a mission.
		if(_exitingApartment)
		{
			return;
		}

		if (_currentCameraAction != null && _currentCameraAction.GetType() == newState.GetType()) return;

		_currentCameraAction = newState;
		_currentCameraAction.Init (mainCamera);
	}

	public void SaveCurrentState ()
	{
		_actions.Push (_currentCameraAction);
		//Pause sequence if menu becomes active half way through moving
		if(_currentCameraAction is BlendToAnimComponent)
		{
			TimelineController.instance.PauseTimeline();
		}
	}

	private bool RestoreState()
	{
		//Unable to change state now as we are going into a mission.
		if(_exitingApartment)
		{
			return false;
		}

		if (_actions.Peek() != null)
		{
			_currentCameraAction = (ApartmentCameraComponent)_actions.Pop();
			//Resume sequence if the previous state was a camera anim
			if (_currentCameraAction is BlendToAnimComponent)
			{
				TimelineController.instance.PlayTimeline();
			}
			//On restore make sure free look is recreated so we start rotating from the current pos(which may have been changed by pan)
			else if (_currentCameraAction is FreeLookComponent)
			{
				ChangeState(new FreeLookComponent());
			}
			return true;
		}
		
		return false;
	}
}
