using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace CoreLib
{
    public class Tracker : BaseListener {

        private float _currentTime; //the time the user has taken while the minigame is active
        private float _setEndTime; //the time assigned as the end of this minigame
        private bool _signalledEnd; //have we reached the end already

        private Dictionary<string, string> _tracked_data;

        private bool _timerActive;

        public static string TRIGGER_TIMER_END = "TriggerEnd";

	   public void Init()
	{
		Init (-1);
	}

	    public void Init(float end_time)
        {
            _currentTime = 0.0f;
            _setEndTime = end_time;
            _signalledEnd = false;

            _timerActive = true;

            _tracked_data = new Dictionary<string, string>();
        }
	
	    public void UpdateTracker()
        {
            if (_timerActive)
            {
                _currentTime += 1.0f * Time.deltaTime;

                if (_setEndTime > -1)
                {
                    if (_currentTime >= _setEndTime && !_signalledEnd)
                    {
                        _signalledEnd = true;
                        Debug.Log("Hit timer end");
                        TriggerEvent(TRIGGER_TIMER_END, gameObject);
                    }
                }
            }
        }

        public void ResetTimer()
        {
            _currentTime = 0.0f;
            _signalledEnd = false;
        }

        public void SetTimerActive(bool timer_active)
        {
            _timerActive = timer_active;
        }

        public void SetEndTime(float value)
        {
            _setEndTime = value;
        }

        public float CurrentTime
        {
            get { return _currentTime; }
        }

        public string GetValue(string value_name)
        {
            if(_tracked_data.ContainsKey(value_name))
            {
                return _tracked_data[value_name];
            }
            else
            {
                return "";
            }
        }

        public void SetValue(string value_name, string value)
        {
            if(_tracked_data.ContainsKey(value_name))
            {
                _tracked_data[value_name] = value;
            }
            else
            {
                _tracked_data.Add(value_name, value);
            }
        }
    }
}
