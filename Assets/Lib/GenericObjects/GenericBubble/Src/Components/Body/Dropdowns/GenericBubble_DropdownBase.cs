using UnityEngine;
using System.Collections;

public class GenericBubble_DropdownBase : GenericObject {

	public enum State { Hidden, Sliding, Showing }
	private State _state = State.Hidden;
	public State GetState { get { return _state; } }

	private GameObject _slider;
	public GameObject Slider {
		get {
			if (_slider == null)
			{
				_slider = gameObject.transform.GetChild (0).gameObject;
			}
			return _slider;
		}
	}
	
	private float _sliderHeight = -1;
	public float Height {
		get {
			if (_sliderHeight == -1)
			{
				_sliderHeight = Slider.GetComponent<RectTransform> ().rect.height;
			}
			return _sliderHeight;
		}
	}
	
	private float _slideDownLength = -1;
	public float SlideDownLength {
		get {
			if (_slideDownLength == -1)
			{
				float bubbleImgHeight = transform.parent.parent.Find ("BubbleImg").GetComponent<RectTransform> ().rect.height;
				RectTransform parentTransform = transform.parent.GetComponent<RectTransform> ();
				float offset = -1 * parentTransform.offsetMax.y + parentTransform.offsetMin.y / 2.0f;
				_slideDownLength = (bubbleImgHeight + GetComponent<RectTransform> ().rect.height) / 2.0f - offset;
				
			}
			return _slideDownLength;
		}
	}
	

//-Interface--------------------------------------------------------------------------------------------	
	
	// Generic Object Interface
	public override void Show (object[] paramList)
	{
		base.Show (paramList);
	}
	
	public override void Hide (object[] paramList)
	{
		base.Hide (paramList);
	}
	
	
	// Slider Interface
	public virtual void SlideDown (float slideTime, float waitTime = 0.0f)
	{
		if (GetState == State.Showing) return;
	
		if (gameObject.activeInHierarchy)
		{
			StopCoroutine ("SlideRoutine");
			StartCoroutine ("SlideRoutine", new object [] {true, slideTime, waitTime});
		}
	}
	
	public virtual void SlideUp (float slideTime, float waitTime = 0.0f)
	{
		if (GetState == State.Hidden) return;
	
		if (gameObject.activeInHierarchy)
		{
			StopCoroutine ("SlideRoutine");
			StartCoroutine ("SlideRoutine", new object [] {false, slideTime, waitTime});
		}
	}	
	
	public virtual void DropdownClicked ()
	{
		if (GetState != State.Showing) return;
	}
	
	
	// Destructor
	public void OnDestroy ()
	{
		iTween.Stop (gameObject, true);
		StopAllCoroutines ();
	}
	

//-Coroutines--------------------------------------------------------------------------------------
	
	protected IEnumerator SlideRoutine (object[] paramList)
	{
		bool down = (bool) paramList [0];
		float slideTime = (float) paramList [1];
		float waitTime = (float) paramList [2];
		
		if (waitTime > 0)
			yield return new WaitForSeconds (waitTime);
	
		float destination = (down) ? -1 * SlideDownLength : 0.0f;
		string easeType;
		
		if (down)
		{
			Slider.SetActive (true);
			easeType = "easeOutSine";
		}
		else
		{
			easeType = "easeInSine";
		}
		
		_state = State.Sliding;
		
		iTween.Stop (Slider);
		iTween.MoveTo (Slider, iTween.Hash ("y" , destination,  "time" , slideTime, "islocal" , true, "easetype", easeType));
		
		yield return new WaitForSeconds (slideTime + 0.001f);
		
		if (Slider.GetComponent<iTween> ())
			DestroyImmediate (Slider.GetComponent<iTween> ());
		
		if (down)
		{
			_state = State.Showing;
		}
		else
		{
			_state = State.Hidden;
			
			Slider.SetActive (false);
		}
	}
}






















