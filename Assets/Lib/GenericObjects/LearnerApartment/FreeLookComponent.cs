using UnityEngine;

public class FreeLookComponent : ApartmentCameraComponent
{
	private GameObject camera;

	private float _honzSensitivity = 1.0f; //how fast should the x rotation be
	private float _vertSensitivity = 1.0f; //how fast should the y rotation be

	private Vector2 _firstTouch; //where did the user touch first
	private Vector2 _nextTouch; //where is the user touching now

	private Vector2 _touchPos; //where is the touch
	private bool _hadFirstTouch; //is this touch the first one

	//used for rotations
	private float _xAngle; 
	private float _yAngle;
    private float _zAngle;
	private float _xAngTemp;
	private float _yAngTemp;
	private float _yClamp = 45; //how much can the y rotate to
	private float _xDirection = 1; //which way is the x direction moving

	public override void Init (GameObject camera)
	{
		this.camera = camera;

		//get the inital angles for the cameras
		_xAngle = camera.transform.rotation.eulerAngles.y; 
		_yAngle = camera.transform.rotation.eulerAngles.x;
		
		_hadFirstTouch = false; //the user has not touched yet
	}

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

	public override void Update () 
	{
		if(GetTouchPos() && !_hadFirstTouch) //has the user touched for one frame yet
		{
			//yes then record where that was and what angles we were at
			_hadFirstTouch = true;
			_firstTouch = _touchPos;
			
			_xAngle = camera.transform.rotation.eulerAngles.y; 
			_yAngle = camera.transform.rotation.eulerAngles.x;
            _zAngle = camera.transform.rotation.eulerAngles.z; 
			
			_xAngTemp = _xAngle;
			_yAngTemp = _yAngle;
		}
		else if(GetTouchPos())
		{
			_nextTouch = _touchPos;
			
			_xAngle = _xAngTemp - (_nextTouch.x - _firstTouch.x) * 180.0f / Screen.width;
			
			_yAngle = _yAngTemp + (_nextTouch.y - _firstTouch.y) * 90.0f / Screen.height;
			
			_xAngle *= _honzSensitivity;
			_yAngle *= _vertSensitivity;
			
			if(_yAngle > 180)
				_yAngle = Mathf.Clamp(_yAngle, 360-_yClamp, 360);
			else
				_yAngle = Mathf.Clamp(_yAngle, -_yClamp, _yClamp);

            camera.transform.rotation = Quaternion.Euler(_yAngle, (_xAngle * _xDirection), _zAngle);
		}
		else
		{
			_hadFirstTouch = false;
		}
	}
}
