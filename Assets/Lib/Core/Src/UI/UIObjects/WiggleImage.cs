using UnityEngine;
using System.Collections;

namespace CoreSystem{
	public class WiggleImage : BaseListener {

	    public float _honzStrength = 1f;
	    public float _vertStrength = 5.0f;
	    private float _vertCounterTime = 1.0f;
		public float _honzSpeed = 1.0f;
		public bool _randomizeWobble = false;
		public int _honzDirection = 0;
		public float _VertSpeed = 0.8f;
        public bool ShouldWiggleOnStart = true;

	    private float _theta;
	    private float _distance;
	    private float _up_down_timer = 0;
	    private Vector3 _wobble_position;
	    private Vector3 _new_position;
		private bool _wobbleActive;
		private Vector3 _startPos;
		private Transform _childToWiggle; 
		/*Always wiggle a child container rather than the object itself
		 * as the object may be draggable so can't
		 * rely on objects initial pos as a constant */

		private const float WOBBLE_SWITCH_TIME = 0.5f;

	    // Use this for initialization
	    void Start()
	    {
            if (ShouldWiggleOnStart)
            {
                StartWiggle();
            }
	    }

	    public void StartWiggle()
	    {
			if (_randomizeWobble) {
				_honzSpeed = Random.Range(4.0f, 10.0f);
				_honzDirection = Random.Range(0, 1);
			}

			if(transform.childCount == 0)
			{
				Debug.LogError("Attached a WiggleImage component with no child container to wiggle.");
				Destroy(this);
				return;
			}
			_childToWiggle = transform.GetChild (0);

			//Start the wiggle off at child local coordinates 0,0.
	        _startPos = Vector2.zero;
	        _wobble_position = _startPos;

	        _wobble_position.y = _startPos.y +
	            Random.Range(-(_vertStrength), _vertStrength);

	        _theta = Random.Range(0, 100);
	        _wobbleActive = true;

	        _distance = _honzStrength * Mathf.Sin(_theta * Mathf.PI);

			_childToWiggle.localEulerAngles = Vector3.forward * _distance;
	    }

	    public void StopWiggle()
	    {
	        _wobbleActive = false;
	        ResetObjectsRotation();
			_childToWiggle.localPosition = _startPos;
	    }

	    public void ResetObjectsRotation()
	    {
			_childToWiggle.localEulerAngles = new Vector3(0, 0, 0);
	    }

		void Update()
		{
			if(_wobbleActive) //are we wobbling/wiggling
			{
				if(_honzDirection == 0)
					_theta = _theta + Time.deltaTime; 
				else
					_theta = _theta - Time.deltaTime;
				
				_distance = _honzStrength * Mathf.Sin(_theta * _honzSpeed);
				_childToWiggle.localEulerAngles = Vector3.forward * _distance;

				if (_up_down_timer >= WOBBLE_SWITCH_TIME) //do we need to switch direction up or down
				{
					_up_down_timer = 0;
					
					_wobble_position = _startPos;
					
					if (_childToWiggle.localPosition.y > _startPos.y)
					{
						_wobble_position.y = _startPos.y - _vertStrength;
					}
					else
					{
						_wobble_position.y = _startPos.y + _vertStrength;
					}
				}
				else
				{
					_up_down_timer += _vertCounterTime * Time.deltaTime;
					
					_new_position = _childToWiggle.localPosition;
					_new_position.y = Mathf.Lerp(_childToWiggle.localPosition.y,
					                             _wobble_position.y,
					                             _VertSpeed * Time.deltaTime);
					
					_childToWiggle.localPosition = _new_position;
				}
			}
		}
	}
}
