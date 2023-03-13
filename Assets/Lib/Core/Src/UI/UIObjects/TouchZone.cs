using UnityEngine.EventSystems;
using UnityEngine;
using System.Collections;
using CoreSystem;

public class TouchZone : BaseListener
{
	/// <summary> Touch Zone global switch. </summary>
	public static bool _isEnabled = true;

	public bool _InitOnStart = true; //does the start method init this object
	protected Vector2 _touchPos; //where is the touch
	protected RectTransform _rect_trans; //the rect transform for the UI object
	protected BoxCollider2D _collider; //the box collider
	protected bool touchDown;
	protected Vector2 _startTouch; 
	protected bool _touchBegin;
	
	public static string TOUCHZONE_EVENT_HIT = "TouchZoneHit";
	public static string TOUCHZONE_EVENT_MISS = "TouchZoneMiss";
	public static string TOUCHZONE_EVENT_DOWN = "TouchZoneDown";
	public static string TOUCHZONE_EVENT_UP = "TouchZoneUp";
	public static string TOUCHZONE_EVENT_UP_LEFT_OBJECT = "TouchZoneUpLeftObject"; //Touch down on object but touch up off it
	public static string TOUCHZONE_EVENT_START = "TouchZoneStart";
	public static string TOUCHZONE_EVENT_DOWN_AND_UP = "TouchZoneDownUp";
	
	public bool _autoResizeCollider = false;

    public bool Blocks2DInput = false;
	
	void Start()
	{
		if (_InitOnStart)
			Init();
	}
	
	public virtual void Init()
	{
		_rect_trans = GetComponent<RectTransform>();
		_collider = GetComponent<BoxCollider2D>();
		_touchBegin = false;
	}
	
	protected virtual void Update()
	{
		if (!_isEnabled || RouteGames.PauseManager.Instance.IsMenuPaused) return;
	
		if(_autoResizeCollider) ResizeCollider(Vector2.zero);
		
		if (GetTouchPos())
		{
			if (IsTouchInZone())
			{
				if(IsTouchDown())
				{
					SendStringEvent(TOUCHZONE_EVENT_DOWN, gameObject);
					touchDown = true;
					
					if(!_touchBegin)
					{
						_touchBegin = true;
						_startTouch = _touchPos;
						SendStringEvent(TOUCHZONE_EVENT_START, gameObject);
					}
                }
				
				SendStringEvent(TOUCHZONE_EVENT_HIT, gameObject);
				
				if(IsTouchUp())
				{
					if(_touchBegin)
					{
						SendStringEvent(TOUCHZONE_EVENT_DOWN_AND_UP, gameObject);
					}
					
					touchDown = false;
					_touchBegin = false;
					SendStringEvent(TOUCHZONE_EVENT_UP, gameObject);
                }
			}
			else
			{
				//Would normally want touch up event even if the touch was up outside this object if the touch was originally down on this object.
				//E.g. A button. Touch down inside it then move before touch up. Button would want to know to release itself from down state.
				if(IsTouchUp() && touchDown)
				{
					touchDown = false;
					SendStringEvent(TOUCHZONE_EVENT_UP_LEFT_OBJECT, gameObject);
					SendStringEvent(TOUCHZONE_EVENT_DOWN_AND_UP, gameObject);
				}
				SendStringEvent(TOUCHZONE_EVENT_MISS, gameObject);
            }
		}
		else if(_touchBegin)
		{
			_touchBegin = false;
			SendStringEvent(TOUCHZONE_EVENT_DOWN_AND_UP, gameObject);
		}
	}
	
	/// <summary>
	/// Sets the size and position of the touch zone
	/// </summary>
	/// <param name="x"></param>
	/// <param name="y"></param>
	/// <param name="width"></param>
	/// <param name="height"></param>
	public void SetZone(float x, float y, float width, float height)
	{
		transform.position = new Vector3(x, y, 1);
		ResizeCollider(new Vector2(width, height));
	}
	
	/// <summary>
	/// Resizes the objects collider.
	/// 
	/// If you put a zero vector2 in the size will be the
	/// size of the RectTransform.rect width and height
	/// </summary>
	/// <param name="new_size"></param>
	public virtual void ResizeCollider(Vector2 new_size)
	{
		if (new_size == Vector2.zero)
		{
			if(_rect_trans != null)
			{
				_collider.size = new Vector2(_rect_trans.rect.width, _rect_trans.rect.height);
			}
			else
			{
				//Do something for 3D objects
			}
		}
		else
		{
			_collider.size = new_size;
		}
	}
	
	/// <summary>
	/// Checks if a touch has happened
	/// if a touch has happened then the variable
	/// _touchPos will contain the position of the touch.
	/// 
	/// If you need a Vector3 then use the member variable
	/// TouchPos after using this method.
	/// </summary>
	/// <returns>If a touch is detected</returns>
	public bool GetTouchPos()
	{
		if (Input.GetMouseButton(0) || Input.GetMouseButtonDown(0) || Input.GetMouseButtonUp(0))
		{
			_touchPos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
			return true;
		}
		else if (Input.touchCount == 1)
		{
			_touchPos = Input.GetTouch(0).position;
			return true;
		}
		
		return false;
	}
	
	public Vector3 TouchPos
	{
		get { return new Vector3(_touchPos.x, _touchPos.y, 1); }
	}
	
	/// <summary>
	/// Is a touch in the zone
	/// </summary>
	/// <returns></returns>
	public bool IsTouchInZone()
	{
		if(!Camera.main)
		{
			return false;
		}

		if (GetTouchPos())
		{			
			//2D UI
			if (_collider != null)
			{

#if CLIENT_BUILD

                StandaloneInputModuleCustom newEventSystem = EventSystem.current.GetComponent<StandaloneInputModuleCustom>();
                if (newEventSystem != null)
                {
                    PointerEventData data = newEventSystem.GetLastPointerEventDataPublic();
                    if (data != null && data.pointerEnter != null)
                    {
                        TouchZone touchZone = data.pointerEnter.GetComponentInChildren<TouchZone>();
                        if (touchZone == null)
                        {
                            // If not found touchzone, check parent
                            touchZone = data.pointerEnter.GetComponentInParent<TouchZone>();
                        }
                        if (touchZone != null)
                        {
                            if (touchZone.Blocks2DInput)
                            {
                                return (touchZone == this);
                            }
                        }
                    }
                }
#endif

                return _collider.OverlapPoint(_touchPos);
            }

            if (EventSystem.current.IsPointerOverGameObject())
            {
                return false;
            }

            //3D objects
            Ray ray = Camera.main.ScreenPointToRay(_touchPos);
            RaycastHit[] hits;
            hits = Physics.RaycastAll(ray, 100);
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.gameObject == gameObject)
                {
                    return true;
                }
            }

		}
		return false;
	}
	
	/// <summary>
	/// Same as above method except only return true if we have had a touch down on that object this frame
	/// </summary>
	/// <returns>If mouse down is detected on object</returns>
	public bool HasTouchDownOnObject()
	{
		if (IsTouchDown())
		{
			return IsTouchInZone();
		}
		return false;
	}
	
	/// <summary>
	/// Has there been a touch down this frame. (Does not neceserrily mean on this object)
	/// </summary>
	/// <returns>If touch down happened this frame</returns>
	public bool IsTouchDown()
	{
		if (Input.GetMouseButtonDown(0))
		{
			return true;
		}
		else if (Input.touchCount == 1)
		{
			if(Input.GetTouch(0).phase == TouchPhase.Began)
			{
				return true;
			}
		}
		
		return false;
	}
	
	/// <summary>
	/// Has there been a touch up this frame. (Does not neceserrily mean on this object)
	/// </summary>
	/// <returns>If touch up happened this frame</returns>
	public bool IsTouchUp()
	{
		if (Input.GetMouseButtonUp(0))
		{
			return true;
		}
		else if (Input.touchCount == 1)
		{
			if(Input.GetTouch(0).phase == TouchPhase.Ended)
			{
				return true;
			}
		}
		
		return false;
	}
	
	/// <summary>
	/// Is this position in the zone
	/// </summary>
	/// <param name="point"></param>
	/// <returns></returns>
	public bool InZone(Vector2 point)
	{
		return _collider.OverlapPoint(point);
	}
}
