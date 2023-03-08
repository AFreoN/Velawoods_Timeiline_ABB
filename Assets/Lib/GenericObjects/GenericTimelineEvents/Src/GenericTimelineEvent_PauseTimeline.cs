using UnityEngine;
using System.Collections;

using CoreLib;

public class GenericTimelineEvent_PauseTimeline : TimelineBehaviour {

	[Header ("Pause Timeline Event Attributes")]
	public bool _isActive = true;
	public float _fadeTime = 0.5f;

    public RectTransform minigameZone = null;

	// Continue button prefab
	private GameObject _continueButton;
	private GenericTimelineEvent_ContinueButton ContinueButton {
		get {
			if (_continueButton == null)
			{
				// load prefab
				_continueButton = Resources.Load<GameObject> ("GenericTimelineEvent_ContinueButton");
				if (_continueButton == null)
				{
					Debug.LogWarning ("Pause Timeline Event : Continue Button prefab not found!");
					return null;
				}
				// instantiate
				_continueButton = Instantiate (_continueButton);
				LayerSystem.Instance.AttachToLayer ("MiniGames", _continueButton);
				// add button listener
				_continueButton.GetComponent<GenericTimelineEvent_ContinueButton> ().AddButtonListener (delegate {OnContinueClicked();});
			}
			return _continueButton.GetComponent<GenericTimelineEvent_ContinueButton> ();
		}
	}

    public override void OnClipStart(object o)
    {
		FireEvent();
    }

    // Pause on fire
    public void FireEvent ()
	{
		//if (Sequence.IsPlaying == false || Sequence.RunningTime > FireTime + Duration) return;
		//if (!_isActive) return;

		TimelineController.instance.PauseTimeline();
        LayerSystem.Instance.SetDimensionsOnMinigameLayer(0.0f, 1.0f, 0.0f, 1.0f, minigameZone);
		ContinueButton.Show (_fadeTime);
	}

    public override void OnClipEnd(object o)
    {
		EndEvent();
    }

    // Remove on end if still there
    public void EndEvent ()
	{
		if (_continueButton!=null) 
			Destroy (_continueButton);
	}
	
	// Release on click
	private void OnContinueClicked ()
	{
		ContinueButton.Hide (_fadeTime, true);
		TimelineController.instance.PlayTimeline();
	}
	
	// Update
	public void Update ()
	{
		// Keep duration larger than _fadeTime - removing at EndEvent
		//if (Duration < _fadeTime)
		//	Duration = _fadeTime + 0.1f;
	}

    public override void OnSkip()
    {
		Skip();
    }
    // Remove object on skip (if there)
    public void Skip () 
	{
		if (_continueButton!=null) 
			Destroy (_continueButton);
	}

    public override void OnReset()
    {
		Reset();
    }

    public void Reset()
    {
        if (_continueButton != null)
            Destroy(_continueButton);
    }
}




























































