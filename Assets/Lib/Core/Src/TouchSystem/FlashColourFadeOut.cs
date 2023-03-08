using UnityEngine;
using System.Collections;
using CoreLib;
using HighlightingSystem;

public class FlashColourFadeOut : TouchSystemBaseCommand {

	private TouchComponent touchComponent;
	private CustomFadeComponent fadeComponent;
	private FlashComponent flashComponent;

	private enum State
	{
		Flashing,
		Fading
	}
	private State state;
	
	void Awake ()
	{
		state = State.Flashing;
		touchComponent = gameObject.AddComponent<TouchComponent>();
		touchComponent.OnTouch += OnTouch;
		FlashComponent flashScript = flashComponent = gameObject.AddComponent<FlashComponent> ();
		flashScript.Init (new Color(0.762f, 0, 0.082f), false);
	}

	private void OnTouch(GameObject touched)
	{
		switch(state)
		{
			case State.Flashing:
				state = State.Fading;
				touchComponent.Reset();
				flashComponent.Reset();
				fadeComponent = gameObject.AddComponent<CustomFadeComponent>();
				fadeComponent.FadeObject(1.0f, 0, 1, 0);
				fadeComponent.OnFadeComplete += OnFadeComplete;
				break;
			default:
				break;
		}
	}

	private void OnFadeComplete()
	{
		ObjectTouched (gameObject);
	}
	
	public void AddListener(System.Action<GameObject> callback)
	{
		touchComponent.OnTouch += callback;
	}
	
	public void OnDestroy()
	{
		if(touchComponent) {
			touchComponent.Reset();
			Destroy (touchComponent);
		}
		if(fadeComponent) {
			Destroy (fadeComponent);
		}
		if(flashComponent) {
			flashComponent.Reset();
			Destroy (flashComponent);
		}
	}
	
	void OnDisable()
	{
		touchComponent.EnableTouch(false);
	}
	
	void OnEnable()
	{
		touchComponent.EnableTouch(true);
	}

	public override void DoStateChange()
	{
		//Game object disabled on skip
		gameObject.SetActive(false);
		//Safe destroy compatible with editor or runtime
		CoreHelper.SafeDestroy (this);
	}
}
