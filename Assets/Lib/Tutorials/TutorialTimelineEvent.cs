using UnityEngine;
using System.Collections;
using WellFired;
using CoreLib;
using TMPro;

public class TutorialTimelineEvent : TimelineBehaviour {
	[Header ("Tutorial Event")]
	
	// Set tutrorial type
	public TutorialsManager.TutorialType _tutorialType;

    public override void OnClipStart(object o)
    {
		FireEvent();
    }
    // Call TutorialsManager to play the above selected tutorial type
    public void FireEvent ()
	{
		if (TimelineController.isPlaying)
		{
			if (_tutorialType == TutorialsManager.TutorialType.Carnegie || _tutorialType == TutorialsManager.TutorialType.CarnegiePoints)
			{
				Debug.LogWarning ("Tutorial Timeline Event : Carnegie & CarnegiePoints tutorials have been disabled in the timeline. It is now recommended to remove them from the timeline.");
				return;
			}
			TutorialsManager.Instance.ShowTutorial (_tutorialType);
		}
	}	
	
	// Set up event
	/*public void Update ()
	{
		if (Comment != _tutorialType.ToString ())
			Comment = _tutorialType.ToString ();
		if (Duration < 1.0f)
			Duration = 2.0f;
	}*/


    //-----------------------------------------------------------------------------------------------------------------

    public override void OnSkip()
    {
		Skip();
    }
    public void Skip () 
	{
		TutorialsManager.Instance.DestroyTutorial (_tutorialType);
	}

    public override void OnReset()
    {
		Reset();
    }
    public void Reset () 
	{
		TutorialsManager.Instance.DestroyTutorial (_tutorialType);
	}

    public override void OnClipEnd(object o)
    {
		EndEvent();
    }
    public void EndEvent ()
	{
		//base.EndEvent ();
	}
}
