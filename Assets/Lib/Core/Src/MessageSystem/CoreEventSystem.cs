//#define CORE_EVENT_LOG

using CoreLib;
using System;
using System.Collections.Generic;
using UnityEngine;

public class CoreEventSystem : MonoSingleton<CoreEventSystem>
{
	public delegate void EventCallback(object obj);
	private Dictionary<string, List<EventCallback>> _events;

	protected override void Init ()
	{
		_events = new Dictionary<string, List<EventCallback>>();

		Log("EventSystem: Init()");
	}

	public bool SendEvent(string eventType)
	{
		return SendEvent (eventType, null);
	}

	public bool SendEvent(string eventType, object parameters)
	{
		bool success = false;
		List<int> toRemove = new List<int> ();

		Log("EventSystem: Sending Event " + eventType);

		if (_events != null)
		{
			if (_events.ContainsKey(eventType))
			{
				//Get the count at the start in case any events are added while firing.
				int eventCount = _events[eventType].Count;
				for(int i = 0; i < eventCount; i++)
				{
					EventCallback cb = _events[eventType][i];
					if (cb != null)
					{
						cb(parameters);
						success = true;
					}
					else
					{
						toRemove.Add(i);
					}
				}

				//Reverse sort so last elements are removed first
				toRemove.Sort(delegate(int a, int b) { return b.CompareTo(a); });
				//Remove any callbacks from this event that are no longer valid
				foreach (int remove in toRemove)
				{
					_events[eventType].RemoveAt(remove);
				}
			}
		}

		if(success)
			Log("EventSystem: Sending Event was successful");
		else
			Log("EventSystem: Sending Event was not successful");

		return success;
	}
	
	public void AddListener(string eventType, EventCallback callback)
	{
		if (_events == null)
			_events = new Dictionary<string, List<EventCallback>>();
		
		if (!_events.ContainsKey (eventType)) {
			_events [eventType] = new List<EventCallback> ();
		}

		if (!_events [eventType].Contains (callback)) {
			_events [eventType].Add (callback);

			Log("EventSystem: Adding Listener for event type " + eventType);
		}
	}

	public void RemoveListener(string eventType, EventCallback callback)
	{
		if (_events.ContainsKey (eventType)) 
		{
			if(_events[eventType].Contains(callback))
			{
				int indexToRemove = _events[eventType].IndexOf(callback);
				//Null out listener. It will be removed safely next time an event is sent
				_events[eventType][indexToRemove] = null;

				Log("EventSystem: Removing Listener for event type " + eventType);
			}
		}
	}
	
	public void RemoveEvent(string eventName)
	{
		if (_events != null)
		{
			if (_events.ContainsKey(eventName))
			{
				_events.Remove(eventName);

				Log("EventSystem: Removing Event " + eventName);
			}
		}
	}

	public void RemoveAllListeners(string eventName)
	{
		if (_events != null)
		{
			if (_events.ContainsKey(eventName))
			{
				_events[eventName].Clear();

				Log("EventSystem: Removing all Listeners for event type " + eventName);
			}
		}
	}

	public void PrintEvents()
	{
		Log("EventSystem: Printing Events");
		
		if(_events != null)
		{
			foreach(string event_name in _events.Keys)
			{
				Log("EventSystem: Event Name - " + event_name);
			}
		}
		
		Log("EventSystem: Finished Printing Events");
	}

	public void PrintListenersCount(string event_name)
    {
        if (_events != null)
		{
			if(_events.ContainsKey(event_name))
			{
				Log("EventSystem: Listener Count - " + _events[event_name].Count);
			}
			else
			{
				Log("EventSystem: No events with the name " + event_name);
			}
		}
    }

    public void Log(string log)
    {
#if CORE_EVENT_LOG
        Debug.Log(log);
#endif
    }

    public void ForcedListenerClear()
    {
        foreach (List<EventCallback> eventEntry in _events.Values)
        {
            for (int callbackIndex = eventEntry.Count - 1; callbackIndex >= 0 ; --callbackIndex)
            {
                if (eventEntry[callbackIndex] == null)
                {
                    eventEntry.RemoveAt(callbackIndex);
                }
            }
        }
    }
}
