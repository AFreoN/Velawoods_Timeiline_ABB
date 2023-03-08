using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GenericBubble_Pointer : GenericObject {
	[Header ("BubblePointer")] [Space (5)]
	
	// The object in the scene the triangle points to
	private GameObject _target;
	private Vector3 _targetPosition = Vector3.zero;
	private float _lerpTime = 0.2f;
	// If the pointer moves about or not. If it is not moveable, the pointer does not require a target and 
	// will not be de-activated. It will also be centered according to its _alignment (below).
	public bool _isMoveable = true;
	// The side of the bubble the pointer sticks to
	public enum Alignment { Top, Bottom, Left, Right };
	public Alignment _alignment;

	private float _widthOffset = 16; // Left + Right
	private float _heightOffset = 16; // Top + Bottom
	
	private RectTransform _bubbleImgRect;
	private RectTransform BubbleImgRect {
		get {
			if (_bubbleImgRect == null)
				_bubbleImgRect = transform.parent.Find ("BubbleImg").GetComponent<RectTransform> ();
			return _bubbleImgRect;
		}
	}
	
	// Canvas scale, static
	private static Transform _canvas = null;
	public Transform CanvasTransform {
		get {
			if (_canvas == null)
			{
				Transform canvas = null;
				Transform canvasCheck = transform;
				while (canvas == null)
				{
					canvasCheck = canvasCheck.parent;
					if (canvasCheck.name.ToLower ().Contains ("maincanvas"))
						canvas = canvasCheck;
				}
				_canvas = canvas;
			}
			return _canvas;
		}
	}
	
	
	// Checking camera animation state for changes. Updating pointer when change occurs
	//
	int _cameraStateHash = 0;
	float _cameraTime = 0;
	float _checkCameraEvery = 1.0f;
	bool _checkCamera = false;
	
	public void OnEnable ()
	{
		if (Camera.main.GetComponent<Animator> () != null)
		{
			_cameraStateHash = Camera.main.GetComponent<Animator> ().GetCurrentAnimatorStateInfo (0).shortNameHash;
			_checkCamera = true;	
		}
	}
	
	public void LateUpdate ()
	{
		if ((_alignment == Alignment.Top || _alignment == Alignment.Bottom) && _isMoveable && transform.localPosition.y != BubbleImgRect.rect.height/2.0f - 8)
		{
			Vector3 pos = transform.localPosition;
			pos.y = BubbleImgRect.rect.height/2.0f - 8;
			transform.localPosition = pos;
		}
	
		if (_checkCamera == false) return;
		
		if (Time.time - _cameraTime > _checkCameraEvery)
		{
			_cameraTime = Time.time;
			if (_cameraStateHash != Camera.main.GetComponent<Animator> ().GetCurrentAnimatorStateInfo (0).shortNameHash)
			{
				StopCoroutine ("UpdatePosition");
				StartCoroutine ("UpdatePosition");
				
				_cameraStateHash = Camera.main.GetComponent<Animator> ().GetCurrentAnimatorStateInfo (0).shortNameHash;
			}
		}
	}
	
	// ^^^
	

	//-Interface-------------------------------------------------------------------------------------------------------------------
	
	public void Init ()
	{

	}
	
	public void OnDestroy ()
	{
		StopCoroutine ("UpdatePosition");
		iTween.Stop (gameObject, true);
	}
	
	public virtual void SetCharacter (object[] paramList)
	{
		GameObject target = (GameObject) paramList [0];
		
		_target = target;
	}
	
	public override void Show (object[] paramList)
	{ // If pointer is moveable and there is a target, move to target. 
	  // If pointer is moveable and there isn't a target, hide pointer.
   	  // If pointer is not moveable / fixed, regardless of target, show.
	
		FitToAlignment (_alignment);
		if (_isMoveable)
		{
			if (_target == null)
			{
				GetComponent<Image> ().enabled = false;
				//gameObject.SetActive (false);
				return;
			}
			else
			{
				GetComponent<Image> ().enabled = true;
				float lerpTime = (float) paramList [0];
				PointToTarget (new object[] {_target, lerpTime});
			}
		}
		base.Show (paramList);
	}
	
	public override void Hide (object[] paramList)
	{ // Hide only if pointer is active
		if (gameObject.activeSelf)
		{
			base.Hide (paramList);
		}
	}

	public void ShowPointer (object[] paramList)
	{
		Show (paramList);
	}

	public void HidePointer (object[] paramList)
	{
		Hide (paramList);
	}
	
	public virtual void PointToTarget (object[] paramList)
	{
		GameObject newTarget = (GameObject) paramList [0];
		float lerpTime = (float) paramList [1];
		
		_target = newTarget;
		_lerpTime = lerpTime;
	
		StopCoroutine ("UpdatePosition");
		StartCoroutine ("UpdatePosition");
	}
	
	public void TogglePointer (object[] paramList)
	{
		GetComponent<Image> ().enabled = ((bool)paramList[0]);
	}
	
	
//-Privates and Protected--------------------------------------------------------------------------------------------------
	
	/// <summary> Returns the target's world-to-screen position. Target can be either 2D (UI) or 3D. </summary>
	protected Vector2 GetCharacterPosition (GameObject target)
	{
		Vector2 characterPosition = Vector2.zero;
		
		if (target.GetComponent<CanvasRenderer> ()) //UI object
		{
			characterPosition.x = target.transform.position.x - Screen.width / 2.0f;
			characterPosition.y = target.transform.position.y - Screen.height / 2.0f;
		}
		else  { // 3D Object
			characterPosition = Camera.main.WorldToViewportPoint (target.transform.position);

			characterPosition.x *= 2048.0f;
			characterPosition.x -= 1024.0f;
			characterPosition.x -= (CanvasTransform.localPosition.x * CanvasTransform.localScale.x) / 2.0f;
			
			characterPosition.y *= 1536.0f;
			characterPosition.y -= 768.0f;
			characterPosition.y -= (CanvasTransform.localPosition.y * CanvasTransform.localScale.y) / 2.0f;
		}
		return characterPosition;
	}
	
	/// <summary> Gets the character's position and clamps it to the bubble's image.
	/// Use this value to set the final pointer position. </summary>
	protected Vector2 ClampCharacterPosition (Vector2 characterPosition)
	{
		float marginToPointerSpacing = 10; // Don't go right to the edge
		// Get bubble bounds
		float widthOffset = _widthOffset + marginToPointerSpacing * 2 + transform.GetComponent<RectTransform> ().rect.height;
		float heightOffset = _heightOffset + marginToPointerSpacing * 2 + transform.GetComponent<RectTransform> ().rect.height;
		// Get limits
		float widthClamp = (BubbleImgRect.rect.width - widthOffset) / 2.0f;
		float heightClamp = (BubbleImgRect.rect.height - heightOffset) / 2.0f;
		// Clamp and return
		return new Vector2 (Mathf.Clamp (characterPosition.x, -1 * widthClamp, widthClamp), Mathf.Clamp (characterPosition.y, -1 * heightClamp, heightClamp));
	}
	
	// This is public here as it's used by BubblePointerEditor.cs, but shouldn't be accessed by any other scripts.
	/// <summary> Adapts the pointer's position and rotation to the selected alignment </summary>
	public void FitToAlignment (Alignment alignment)
	{
		RectTransform pointerRect = GetComponent<RectTransform> ();
		
		Vector3 position = BubbleImgRect.transform.localPosition;
		Vector3 rotation = BubbleImgRect.transform.localEulerAngles;
		
		switch (alignment)
		{
		case Alignment.Top:
			position.y += (BubbleImgRect.rect.height - _heightOffset) / 2.0f;
			rotation.z -= 90;
			pointerRect.anchorMin = new Vector2 (0.5f, 1.0f);
			pointerRect.anchorMax = new Vector2 (0.5f, 1.0f);
			break;
		case Alignment.Bottom:
			position.y -= (BubbleImgRect.rect.height - _heightOffset) / 2.0f;
			rotation.z += 90;
			pointerRect.anchorMin = new Vector2 (0.5f, 0.0f);
			pointerRect.anchorMax = new Vector2 (0.5f, 0.0f);
			break;
		case Alignment.Left:
			position.x -= (BubbleImgRect.rect.width - _widthOffset) / 2.0f;
			rotation.z += 0;
			pointerRect.anchorMin = new Vector2 (0.0f, 0.5f);
			pointerRect.anchorMax = new Vector2 (0.0f, 0.5f);
			break;
		case Alignment.Right:
			position.x += (BubbleImgRect.rect.width - _widthOffset) / 2.0f;
			rotation.z += 180;
			pointerRect.anchorMin = new Vector2 (1.0f, 0.5f);
			pointerRect.anchorMax = new Vector2 (1.0f, 0.5f);
			break;
		}
		transform.localPosition = position;
		transform.localEulerAngles= rotation;
	}
	
	//-Coroutines-------------------------------------------------------------------------------------------------------
	
	IEnumerator UpdatePosition ()
	{
		if (gameObject == null || _target == null)
			yield break;

		Vector3 newTargetPosition = GetCharacterPosition (_target);
		_targetPosition = newTargetPosition;
		Vector2 finalPosition = ClampCharacterPosition (_targetPosition);
		Vector3 initialPosition = transform.localPosition;
		
		Vector3 pos = transform.localPosition;
		float currentTime = 0;
		while (currentTime<_lerpTime)
		{
			currentTime += Time.deltaTime;
			
			float lerpValue = Mathf.Clamp (currentTime/_lerpTime, 0, 1);
			
			if (_alignment == Alignment.Top || _alignment == Alignment.Bottom)
			{
				pos.x = initialPosition.x + (finalPosition.x - initialPosition.x)*lerpValue;
			}
			else
			{	
				pos.y = initialPosition.y + (finalPosition.y - initialPosition.y)*lerpValue;
			}
			transform.localPosition=pos;
			
			yield return null;
		}
	}
}






















