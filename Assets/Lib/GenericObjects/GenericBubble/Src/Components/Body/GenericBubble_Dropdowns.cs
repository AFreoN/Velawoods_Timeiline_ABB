using UnityEngine;
using System.Collections;

public class GenericBubble_Dropdowns : MonoBehaviour {

	private GenericBubble_Help _help;
	/// <summary> Help button. Should have the audio of the tutor saying the sentence. Use .SetAudioClip () </summary>
	public GenericBubble_Help Help {
		get {
			if (_help == null) {
				_help = transform.Find ("Help").GetComponent<GenericBubble_Help> ();
			}
			return _help;
		}
	}
	
	private GenericBubble_Legend _legend;
	/// <summary> Carnegie Legend. Handled internally by the bubble. </summary>
	public GenericBubble_Legend Legend {
		get {
			if (_legend == null) {
				_legend = transform.Find ("Legend").GetComponent<GenericBubble_Legend> ();
			}
			return _legend;
		}
	}
	
	private GenericBubble_ReplaySelfAssessment _replay;
	/// <summary> Self Assessment button. Has the user's recording and shows itself when offline. Handled internally by the bubble. </summary>
	public GenericBubble_ReplaySelfAssessment SelfAssessment {
		get {
			if (_replay == null) {
				_replay = transform.Find ("Replay").GetComponent<GenericBubble_ReplaySelfAssessment> ();
			}
			return _replay;
		}
	}
	
	private GenericBubble_Next _next;
	/// <summary> Next button, jumps to recordButtonOnComplete. Handled internally by the bubble. </summary>
	public GenericBubble_Next Next {
		get {
			if (_next == null) {
				_next = transform.Find ("Next").GetComponent<GenericBubble_Next> ();
			}
			return _next;
		}
	}
	
	private float _maxSliderHeight = -1;
	/// <summary> Grabs the height of the largest dropdown. </summary>
	public float MaxSliderHeight {
		get {
			if (_maxSliderHeight == -1)
			{
				if (Help.Height > _maxSliderHeight) _maxSliderHeight = Help.Height;
				if (Next.Height > _maxSliderHeight) _maxSliderHeight = Next.Height;
				if (Legend.Height > _maxSliderHeight) _maxSliderHeight = Legend.Height;
				if (SelfAssessment.Height > _maxSliderHeight) _maxSliderHeight = SelfAssessment.Height;
			}
			return _maxSliderHeight;
		}
	}
	
	// Counting the number of times ShowCarnegieFeedback is called
	int _carnegieCount = 0;
	
	public static bool _hideAllCalled = false; // When this is true, the bubble will search again for its appropriate dropdowns and set it back to false until all are hidden again.

//-Interface---------------------------------------------------------------------------------------------

	public virtual void ShowCarnegieFeedback (float slideTime)
	{
		_carnegieCount ++;
		
		if (Legend.GetState == GenericBubble_DropdownBase.State.Hidden)
		{
			Legend.SlideDown (slideTime);
		}
		
		if (Help.GetState == GenericBubble_DropdownBase.State.Hidden)
		{
			Help.SlideDown (slideTime);
		}
	}
	
	public void ShowCarnegieOffline (float slideTime)
	{
		if (Legend.GetState == GenericBubble_DropdownBase.State.Showing)
		{
			Legend.SlideUp (slideTime);
		}
		
		if (Help.GetState == GenericBubble_DropdownBase.State.Hidden)
		{
			Help.SlideDown (slideTime);
		}
		
		if (SelfAssessment.GetState == GenericBubble_DropdownBase.State.Hidden)
		{
			SelfAssessment.SlideDown (slideTime, 0.05f);
		}
		
		if (Next.GetState == GenericBubble_DropdownBase.State.Hidden)
		{
			Next.SlideDown (slideTime, 0.1f);
		}
	}
	
	public void HideAll (float slideTime)
	{
		_hideAllCalled = true;
		_carnegieCount = 0;
	
		if (Legend.GetState == GenericBubble_DropdownBase.State.Showing)
		{
			Legend.SlideUp (slideTime);
		}
		
		if (Help.GetState == GenericBubble_DropdownBase.State.Showing)
		{
			Help.SlideUp (slideTime);
		}
		
		if (SelfAssessment.GetState == GenericBubble_DropdownBase.State.Showing)
		{
			SelfAssessment.SlideUp (slideTime);
		}
		
		if (Next.GetState == GenericBubble_DropdownBase.State.Showing)
		{
			Next.SlideUp (slideTime, 0.05f);
		}
	}
}



















































