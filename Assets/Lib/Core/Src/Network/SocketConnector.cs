#define SOCKET_DEBUG_LOGS

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System;
using System.Threading;
using System.Text;
using WebSocketSharp;


namespace CoreSystem {
	public class SocketConnector : MonoSingleton<SocketConnector>
	{
        public EventHandler OnConnected = delegate { };
        public EventHandler OnNetworkOpen = delegate { };
        public EventHandler<CloseEventArgs> OnNetworkClose = delegate { };

        public delegate void SocketCallback(object obj);
		private Dictionary<string, List<SocketCallback>> callbacks;

		List<string> events;
		WebSocket client;

		private InternetConnection.Callback _connectionCallback;
		private GameObject _connectionChecker;
		private bool _useEncryption = false;

		public bool websocketOpen { get { return (client.ReadyState == WebSocketState.Open);} }

		protected override void Init ()
		{
			base.Init ();
			callbacks = new Dictionary<string, List<SocketCallback>> ();
			events = new List<string> ();
		}

		public void Connect( string wsUrl ) {

			AddListener ("client.hello", OnClientHello);

			client = new WebSocket ( wsUrl );

			client.OnMessage += OnMessage;
            client.OnClose += OnNetworkCloseHandler;
            client.OnOpen += OnNetworkOpen;
            client.OnConnected += OnConnected;

#if SOCKET_DEBUG_LOGS && !RELEASE_BUILD
			Debug.Log ("Connecting to " + wsUrl );
#endif
			client.Connect ();			
#if SOCKET_DEBUG_LOGS && !RELEASE_BUILD
			Debug.Log ("Connected " + client);
#endif

		}

        private void OnNetworkCloseHandler(object sender, CloseEventArgs e)
        {
            // Tell the application to attempt to reconnect on main thread.
            OnNetworkClose(sender, e);
        }

        public void LoginUsingAccessToken(string accTok, string clientVersion, string platform)
		{
			Hashtable message = new Hashtable();
			message["name"] = "player.login";
			Hashtable payload = new Hashtable();
			payload["accessToken"] = accTok;
			payload["version"] = clientVersion;
			payload["platform"] = platform;
			message["payload"] = payload;
			
#if SOCKET_DEBUG_LOGS && !RELEASE_BUILD
			Debug.Log("Login access token: " + JSON.JsonEncode(message));
#endif
			SendMessage(message);
		}

		public void ReconnectUsingAccessToken(string accTok, string clientVersion, string platform)
		{
			Hashtable message = new Hashtable();
			message["name"] = "player.reconnect";
			Hashtable payload = new Hashtable();
			payload["accessToken"] = accTok;
			payload["version"] = clientVersion;
			payload["platform"] = platform;
			message["payload"] = payload;
			
#if SOCKET_DEBUG_LOGS && !RELEASE_BUILD
			Debug.Log("Reconnect access token: " + JSON.JsonEncode(message));
#endif
			SendMessage(message);
		}

		public void Login( string username, string password, string clientVersion, string platform, bool rememberMeBox = false)
		{
			Hashtable message = new Hashtable();
			message["name"] = "player.login";
			Hashtable payload = new Hashtable();
			payload["email"] = username;
			payload["password"] = password;
			payload["version"] = clientVersion;
			payload["platform"] = platform;
			message["payload"] = payload;
			
#if SOCKET_DEBUG_LOGS && !RELEASE_BUILD
			Debug.Log("Login token: " + JSON.JsonEncode(message));
#endif
			SendMessage(message);
		}

		public void CreateGuestAccount( string firstname, string clientVersion, string platform, string language )
		{
			Hashtable message = new Hashtable();
			message["name"] = "player.createGuestAccount";
			Hashtable payload = new Hashtable();
			payload["forename"] = firstname;
			payload["platform"] = platform;
			payload["version"] = clientVersion;
			payload["language"] = language;
			message["payload"] = payload;
			
			#if SOCKET_DEBUG_LOGS && !RELEASE_BUILD
			Debug.Log("Create guest: " + JSON.JsonEncode(message));
			#endif
			SendMessage(message);
		}

		public void CreatePartialAccount( string firstname, string surname, string email, string password, string clientVersion, string platform, string language )
		{
			Hashtable message = new Hashtable();
			message["name"] = "player.createAccount";
			Hashtable payload = new Hashtable();
			payload["forename"] = firstname;
			payload["surname"] = surname;
			payload["email"] = email;
			payload["password"] = password;
			payload["platform"] = platform;
			payload["version"] = clientVersion;
			payload["language"] = language;
			message["payload"] = payload;
			
			#if SOCKET_DEBUG_LOGS && !RELEASE_BUILD
			Debug.Log("Create guest: " + JSON.JsonEncode(message));
			#endif
			SendMessage(message);
		}

		public void LogOut() {
			client.Close ();
			client.Connect ();
		}
		
		
		public void SendMessage(Hashtable message)
		{
			if(message == null) return;

			if (_useEncryption && message.ContainsKey ("payload")) {

				string json_string = JSON.JsonEncode(message["payload"]);
				string[] encryption = EncryptionManager.Instance.Encrypt( json_string );

				message["payload"] = encryption[0];
				message["iv"] = encryption[1];

			}

			string data = JSON.JsonEncode(message);

			#if SOCKET_DEBUG_LOGS && !RELEASE_BUILD
			Debug.Log("Sending to server: " + JSON.JsonEncode(message));
			#endif

			client.Send (data);
		}
		
		
		public void SendMessage(string messageType, string payload = "", bool payloadIsArray = false)
		{
			string data;

			if(string.IsNullOrEmpty(payload) == false)
			{
				if ( !payloadIsArray ) payload = "{" + payload + "}";

				string iv = "";

				if ( _useEncryption ) {

					string[] encryption = EncryptionManager.Instance.Encrypt( payload );
					
					payload = "\"" + encryption[0] + "\"";
					iv = ", \"iv\":\"" + encryption[1] + "\"";
					
				}


				data = "{\"name\":\"" + messageType + "\", \"payload\": " + payload + iv + "}";


				/*if(payloadIsArray)
				{
					data = "{\"name\":\"" + messageType + "\", \"payload\": " + payload + "}";
				}
				else
				{
					data = "{\"name\":\"" + messageType + "\", \"payload\": {" + payload + "}}";
				}*/
			}
			else
			{
				data = "{\"name\":\"" + messageType + "\"}";
			}
#if SOCKET_DEBUG_LOGS && !RELEASE_BUILD
			Debug.Log ("Sending to server: " + data);
#endif
			client.Send( data);
		}

		void OnMessage( object sender, MessageEventArgs e ) {

			// NOTE: THIS GET CALLED ON A SEPARATE THREAD 
			// You cannot use unity items on anything but the main thread
			// Push the events onto a queue to be run on the main thread
			
#if SOCKET_DEBUG_LOGS && !RELEASE_BUILD
			Debug.Log ("Data received " + e.Data);
#endif
			events.Add (e.Data);

		}
		
		
		void OnClose( object sender, CloseEventArgs e ) {
			CoreEventSystem.Instance.SendEvent (CoreEventTypes.NETWORK_CLOSED);
		}

		void OnOpen( object sender, EventArgs e ) {
			CoreEventSystem.Instance.SendEvent (CoreEventTypes.NETWORK_CONNECTED);
		}


		public void AddListener(string messageType, SocketCallback callback)
		{
			messageType = messageType.ToLower(); // Make name lowercase to remove server/client inconsistencies
			if (callbacks.ContainsKey (messageType) == false)
			{
				callbacks [messageType] = new List<SocketCallback> ();
			}
            
            callbacks[messageType].Add(callback);
		}

		public void RemoveListener(string eventType, SocketCallback callback)
		{
			eventType = eventType.ToLower(); // Make name lowercase to remove server/client inconsistencies
			if (callbacks.ContainsKey (eventType)) 
			{
				if(callbacks[eventType].Contains(callback))
				{
					int indexToRemove = callbacks[eventType].IndexOf(callback);
					//Null out listener. It will be removed safely next time an event is sent
					callbacks[eventType][indexToRemove] = null;
				}
			}
		}

		void RunQueue() {

			
			lock (events) 
			{
				while(events.Count > 0)
				{
					string evt = events[0];
					string messageName = "";
					JSONObject jsonData = new JSONObject (evt);
					Dictionary<string, string> data = new JSONObject (evt).ToDictionary ();
					data.TryGetValue ("name", out messageName);
					messageName = messageName.ToLower(); // Make name lowercase to remove server/client inconsistencies
					
					List<int> toRemove = new List<int> ();
					if(callbacks.ContainsKey(messageName))
					{
						for(int i = 0; i < callbacks[messageName].Count; i++)
						{
							SocketCallback callback = callbacks[messageName][i];
							if(callback == null)
							{
								toRemove.Add(i);
							}
							else
							{
								JSONObject payload = null;
								//Check if event has payload
								if(jsonData.HasField("payload"))//list.Count > 1)
								{
									payload = jsonData.GetField("payload");//list[1];

									if ( _useEncryption && jsonData.HasField("iv") ) {
										// payload will be an encrptyed string.
										// We need to decrypt this and make it into a json object
										string IV = jsonData.GetField("iv").str;
										string payload_string = payload.str;
										payload_string = EncryptionManager.Instance.Decrypt( payload_string, IV );

										payload = new JSONObject( payload_string );

									}
								}
								
								//Send back json object of payload
								callback.Invoke(payload);
							}
						}
						
						//Reverse sort so last elements are removed first
						toRemove.Sort(delegate(int a, int b) { return b.CompareTo(a); });
						//Safely remove any callbacks from this event that are no longer valid
						foreach (int remove in toRemove)
						{
							callbacks[messageName].RemoveAt(remove);
						}
					}
					events.RemoveAt(0);
				}
			}
			events.Clear ();
		}

		public void Update() 
		{
			RunQueue ();
            if (client != null)
            {
                client.Update();
            }
        }

		protected override void Dispose ()
		{
            client.OnMessage -= OnMessage;
            client.OnClose -= OnNetworkCloseHandler;
            client.OnOpen -= OnNetworkOpen;
            client.OnConnected -= OnConnected;

            client.Close ();

			base.Dispose ();
		}

		private void OnClientHello( object obj ) {

			Debug.Log ("Received hello");

			if (obj != null) {

				JSONObject json = (JSONObject) obj;
				if  (json.HasField("useEncryption")) {
					_useEncryption = json.GetField("useEncryption").b;
				}
			}

			//_useEncryption = true;
			//string[] encryption = EncryptionManager.Instance.Encrypt ("Hello, World!", "UrLq5fundvD8Ip5RGZxsEw==");
			//Debug.Log ("Use encrption set to " + _useEncryption + " " + encryption[0] + " " + encryption[1] );
			//Debug.Log (EncryptionManager.Instance.Decrypt ( "bqBgyTZXDgXjFdRRu5KilA==", "UrLq5fundvD8Ip5RGZxsEw==" ));
		}
	}
}

