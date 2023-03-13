using UnityEngine;
using System.Collections;

using CoreSystem;


public abstract class Tutorial_Base : MonoBehaviour {

	/// <summary> Type of tutorial. Set by TutorialsManager. If you're not using the Manager, set this by yourself! </summary>
	public TutorialsManager.TutorialType _myType;

	public abstract void Enter (bool showPromptFirst);
	public abstract void Exit ();
	
	/// <summary> USE THIS! Call this when tutorial starts playing. Lets TutorialsManager and timeline events know. </summary>
	public virtual void OnTutorialSeen ()
	{
		// Let TutorialsManager know that the tutorial is being played so that it can set the PlayerPrefs appropriately. Also the timeline events will pause the sequence on this message.
		CoreEventSystem.Instance.SendEvent (TutorialsManager.Messages.TUTORIAL_STARTED_PLAYING, _myType);

        // Pause the game
        RouteGames.PauseManager.Instance.MenuPause();
    }
	
	public void DestroyMe ()
	{
		Destroy (gameObject);
	}

    public void OnDestroy()
    {
        // Unpause game
        RouteGames.PauseManager.Instance.MenuResume();
    }

    public void OnDisable()
    {
        // Unpause game
        RouteGames.PauseManager.Instance.MenuResume();
    }
}
















































