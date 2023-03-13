using UnityEngine;
using System.Collections;

namespace CoreSystem {
	public class InternetConnection : MonoBehaviour {
	
		public delegate void Callback(bool success);
		private Callback _callback;
	
		// Public DNS servers, will check in sequence until ping successful or all time out. {Google, Google, Level3}
		private static string[] _pingAddresses = new string[] {"8.8.8.8", "8.8.4.4", "209.244.0.3"}; 
		private static float _waitingTime = 0.64f; // seconds
		private static bool _allowCarrierDataNetwork = true;
		
		private Ping _ping;
		private float _pingStartTime;
		
		
//-Interface-------------------------------------------------------------------------------------------------------------
		
		/// <summary>
		/// Checks internet connection. Attaches itself to the gameObject obj while checking. When it's done calls the callback method (bool isConnected).
		public static void CheckConnection (GameObject obj, Callback callbackMethod)
		{
			obj.AddComponent<InternetConnection> ().StartConnectionCheck (callbackMethod);
		}
		
		/// <summary>
		/// Starts checking internet connection. To be called only when attached to a gameObject or by the static CheckConnection </summary>
		public void StartConnectionCheck (Callback callbackMethod)
		{
			_callback = callbackMethod;
				
			if (Debug.isDebugBuild)
			Debug.Log ("Internet Connection: Checking..");
			
			bool internetPossiblyAvailable;
			switch (Application.internetReachability)
			{
			case NetworkReachability.ReachableViaLocalAreaNetwork:
				internetPossiblyAvailable = true;
				break;
			case NetworkReachability.ReachableViaCarrierDataNetwork:
				internetPossiblyAvailable = _allowCarrierDataNetwork;
				break;
			default:
				internetPossiblyAvailable = false;
				break;
			}
			if (!internetPossiblyAvailable)
			{
				InternetAvailability (false);
				return;
			}
			
			StartCoroutine (PingUpdate ());
		}
		
		
//-Privates--------------------------------------------------------------------------------------------------------------
		
		private void InternetAvailability (bool isConnected)
		{
			_ping = null;
			
			if (Debug.isDebugBuild)
				Debug.Log ("Internet Connection: " + isConnected);
		
			if (_callback != null)
				_callback (isConnected);
			Destroy (GetComponent<InternetConnection> ());
		}
		
		private IEnumerator PingUpdate (int tryCount = 0)
		{
			_ping = new Ping(_pingAddresses [tryCount]);
			_pingStartTime = Time.time;
		
			if (Debug.isDebugBuild)
				Debug.Log ("Internet Connection: Ping to " + _pingAddresses [tryCount]);
		
			if (_ping == null)
				yield break;
		
			while (true)
			{
				// Done
				if (_ping.isDone)
				{
					InternetAvailability (true);
					yield break;
				}
				else 
				{
					// Timed out
					if (Time.time - _pingStartTime > _waitingTime)
					{
						if (Debug.isDebugBuild)
							Debug.Log ("Internet Connection: Ping to " + _pingAddresses [tryCount] + " timed out.");
						
						if (tryCount == _pingAddresses.Length - 1)
							InternetAvailability (false);
						else
							StartCoroutine (PingUpdate (tryCount+=1));
							
						yield break;
					}
				}
				yield return null;
			}
		}
	}
}

























