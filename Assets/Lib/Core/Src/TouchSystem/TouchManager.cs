using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace CoreLib
{
	public enum TouchAndFlashStyle
	{
		Base,
		FlashColour,
        FlashColourFadeOut,
	}

    public class TouchManager : MonoSingleton<TouchManager>
	{
        private Vector3 _touchPos;
		private Vector2 _touchPosVec2;
        private List<GameObject> _touchObjects;
		private GameObject _heldObject; //Are we dragging or holding some content;

        protected override void Init()
        {
            base.Init();

            _touchObjects = new List<GameObject>();
            _touchPos = Vector3.zero;
        }

		public void ObjectPickedUp(GameObject heldObj)
		{
			_heldObject = heldObj;
		}

		public void ObjectDropped(GameObject droppedObj)
		{
			if(_heldObject == droppedObj)
			{
				_heldObject = null;
			}
		}

		public GameObject GetHeldObject()
		{
			return _heldObject;
		}

        public bool IsTouching()
        {
            if(Input.touchCount == 1)
            {
                _touchPos.x = Input.touches[0].position.x;
                _touchPos.y = Input.touches[0].position.y;
                return true;
            }
            else if(Input.GetMouseButtonDown(0))
            {
                _touchPos = Input.mousePosition;
                return true;
            }

            return false;
        }

        public Vector3 TouchPos_Vec3
        {
            get { return _touchPos; }
        }

        public Vector2 TouchPos_Vec2
        {
			get {
				_touchPosVec2.x = _touchPos.x;
				_touchPosVec2.y = _touchPos.y;
				return _touchPosVec2;
			}
        }

        private void OnObjectTouched(object touch_obj)
        {
		    if (touch_obj == null)
            {
                Debug.Log("TouchManager: TouchObject was null");
                return;
            }

            if(_touchObjects.Contains((GameObject)touch_obj))
            {
                _touchObjects.Remove((GameObject)touch_obj);

				CoreEventSystem.Instance.SendEvent(CoreEventTypes.SCENE_OBJECT_TOUCHED, touch_obj);

                if(_touchObjects.Count == 0 && !SequenceManager.Instance.IsPlaying)
                {
                    SequenceManager.Instance.Play();
                }
            }
        }

        public void ClearTouchObjects()
        {
        	// TODO: reset touch object's material if it has been modified by the flash
            foreach(GameObject touch_obj in _touchObjects)
            {
				if(touch_obj != null && touch_obj.GetComponent<TouchSystemBaseCommand>() != null)
				{
					TouchSystemBaseCommand touch_script = touch_obj.GetComponent<TouchSystemBaseCommand>();

					CoreHelper.SafeDestroy(touch_script);
				}
            }

            _touchObjects.Clear();
        }

		public void TouchNextObject()
		{
			if (Application.isEditor)
			{
				if(_touchObjects.Count > 0)
				{
					_touchObjects[0].GetComponent<TouchSystemBaseCommand>().ObjectTouched(_touchObjects[0]);
				}
			}
		}

		public void AddStyle(GameObject touch_obj, TouchAndFlashStyle style)
		{
			switch (style) 
			{
				case TouchAndFlashStyle.Base:
				{
					touch_obj.AddComponent<TouchSystemBaseCommand>();
					break;
				}

				case TouchAndFlashStyle.FlashColour:
				{
					touch_obj.AddComponent<FlashColour>();
					break;
				}

                case TouchAndFlashStyle.FlashColourFadeOut:
                {
                    touch_obj.AddComponent<FlashColourFadeOut>();
                    break;
                }
			}
		}

		public void AddTouchObject(GameObject touch_obj, bool pauseTimeline=true)
		{
			TouchSystemBaseCommand listener = touch_obj.GetComponent<TouchSystemBaseCommand>();

			if (listener != null)
			{
				listener.ObjectTouched += OnObjectTouched;
				_touchObjects.Add(touch_obj);
				
				if (pauseTimeline)
				{
					SequenceManager.Instance.Pause();
				}
			}
			else
			{
				Debug.Log("TouchManager: TouchObject did not have a base listner class");
			}
		}

        public void AddTouchObject(GameObject touch_obj, TouchAndFlashStyle style, bool pauseTimeline=true)
        {
            if (touch_obj == null)
            {
                Debug.Log("TouchManager: TouchObject was null");
                return;
            }

			AddStyle (touch_obj, style);

			AddTouchObject(touch_obj, pauseTimeline);
        }
    }
}
