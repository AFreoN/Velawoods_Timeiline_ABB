using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Tutorial_Static_TutorialWindow : MonoBehaviour {

	// Components

	private RectTransform _content;
	private RectTransform Content {
		get {
			if (_content == null)
				_content = transform.Find ("Mask/Content").GetComponent<RectTransform> ();
			return _content;
		}
	}
	
	private float[] _contentHeights;
	private float[] ContentHeights {
		get {
			if (_contentHeights == null)
			{
				_contentHeights = new float[Content.transform.childCount];
				for (int i=0; i<Content.transform.childCount; i++)
					_contentHeights [i] = Content.transform.GetChild (i).GetComponent<RectTransform> ().rect.height;
			}
			return _contentHeights;
		}
	}
	
	private Button _continueButton;
	private Button ContinueButton {
		get {
			if (_continueButton == null)
				_continueButton = transform.Find ("Mask/ContinueButton").GetComponent<Button> ();
			return _continueButton;
		}
	}
	
	// private values
	private float _continueButtonOffset = 20f;
	private float _screenHeight = 2048.0f;
	private int   _entryCount = 0;


//-Interface---------------------------------------------------------------------------------------------------------------

	// Called when prompt button is clicked. Hide all content behind header and push below screen.
	public void Init ()
	{
		// Hide content behind header
		Vector3 entryPos = Vector3.zero;
		for (int i=0; i<Content.transform.childCount; i++)
		{
			RectTransform entryRect = Content.transform.GetChild (i).GetComponent<RectTransform> ();
			
			entryPos = entryRect.transform.localPosition;
			entryPos.y = Content.rect.height/2.0f + entryRect.rect.height/2.0f;
			entryRect.transform.localPosition = entryPos;
			
			entryRect.transform.SetAsFirstSibling ();
		}
		ContinueButton.transform.localPosition = entryPos;
		
		// Push below screen
		Vector3 windowPos = transform.localPosition;
		windowPos.y -= _screenHeight;
		transform.localPosition = windowPos;
		
		// Set active
		gameObject.SetActive (true);
	}
	
	// Show window and show first content entry
	public void Enter (float lerpTime)
	{
		iTween.Stop (gameObject);
		iTween.MoveTo (gameObject, iTween.Hash ("y", 0,  "time" , lerpTime, "islocal" , true, "easetype", "easeOutQuart"));
	}
	
	// Hide window
	public void Exit (float lerpTime)
	{
		iTween.Stop (gameObject);
		iTween.MoveTo (gameObject, iTween.Hash ("y", -1 * _screenHeight,  "time" , lerpTime, "islocal" , true, "easetype", "easeInQuart"));
	}
	
	// Jump to next content entry
	public void Next (float lerpTime, float waitTime = 0)
	{
		if (_entryCount >= Content.transform.childCount) return;
		StartCoroutine (ShowEntry (new object[] {_entryCount, lerpTime, waitTime}));
		_entryCount += 1;
	}
	

//-Coroutines----------------------------------------------------------------------------------------------------------------------------

	// Show particular entry. To be used sequentially 
	private IEnumerator ShowEntry (object[] param)
	{
		// Get
		int entryNo = (int)param[0]; // Content child index
		float lerpTime = (float)param[1]; // Sliding time
		float waitTime = (param.Length>2) ? (float)param[2] : 0; // Wait?
		
		// Wait if any
		if (waitTime > 0)
			yield return new WaitForSeconds (waitTime);
		
		// Get destination of target content entry
		float yDestination = Content.rect.height/2.0f - ContentHeights [ContentHeights.Length-1-entryNo]/2.0f;
		for (int i=0; i<entryNo; i++)
			yDestination -= ContentHeights [Content.transform.childCount - 1 - i];

		// Push all hidden content to that destination (including target content entry)
		for (int i=0; i<Content.transform.childCount-entryNo; i++)
		{
			float destination = yDestination - ContentHeights [ContentHeights.Length-1-entryNo]/2.0f + ContentHeights [i]/2.0f;
			
			iTween.Stop (Content.transform.GetChild (i).gameObject);
			iTween.MoveTo (Content.transform.GetChild (i).gameObject, iTween.Hash ("y", destination,  "time" , lerpTime, "islocal" , true, "easetype", "easeOutQuart"));
		}
		
		// Push Continue button below the above lerped content
		iTween.Stop (ContinueButton.gameObject);
		iTween.MoveTo (ContinueButton.gameObject, iTween.Hash ("y", yDestination - ContentHeights [ContentHeights.Length-1-entryNo] - _continueButtonOffset,  "time" , lerpTime, "islocal" , true, "easetype", "easeOutQuart"));
	}
}



















































