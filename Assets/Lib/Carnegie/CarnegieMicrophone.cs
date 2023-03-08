using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using r1g.plugins;

[RequireComponent (typeof (CarnegieXMLInterpreter))]
[RequireComponent (typeof (CarnegieXMLSender))]
public class CarnegieMicrophone : MonoBehaviour {

	// Mic components
	private string _micName;
	private AudioClip _micClip;
	
	private const int _frequency = 22050;
	private const int _lengthSamples = 22050;
	private const int _channels = 1;
	private int _maxRecordingLength = 30;
	private float _recordingStartedAt = 0;

	// Read/Write properties
	public int MaxRecordingLength {
		get { return _maxRecordingLength; }
		set {_maxRecordingLength = value; }
	}
	
	// Encoder
	private OggVorbisEnc ove;
	private const float _encodeTimeout = 10; // seconds
	
	// Carnegie strings kept to be sent to the server when encoder is finished
	private List<DialogueEventData.DialogueText> _carnegieInput = new List<DialogueEventData.DialogueText> ();
	
	private bool _micActive = false;
	private bool _sendToCarnegie;
	

	void OnDestroy ()
	{
		ClearMemory ();
	}
	
//-Interface------------------------------------------------------------------------------------------------------------------------------------------------
	
	public void StartRecording (List<DialogueEventData.DialogueText> carnegieInput, bool sendToCarnegie = true)
	{
		_carnegieInput = carnegieInput;
		_sendToCarnegie = sendToCarnegie;

		if(Microphone.devices.Length > 0)
		{
			_micName = Microphone.devices [0];
			_micActive = true;
		}
		else
		{ 
			//if (Debug.isDebugBuild) 
			//	Debug.LogWarning("Carnegie Microphone: You don't have a microphone plugged in!"); 
			_micActive = false; 
			return;
		}

		ClearMemory ();
		
		_micClip = Microphone.Start (_micName, false, _maxRecordingLength, _frequency);

		if(_micClip == null)
		{
			//Error in starting microphone. 
			_micActive = false;
		}

		_recordingStartedAt = Time.time;
	}
	
	public AudioClip StopRecording ()
	{
		if (_micActive)
		{
			// End
			Microphone.End(_micName);
			// Get recording length
			float recordingLength = Time.time - _recordingStartedAt + 0.01f;
			// Trim
			if (recordingLength < _maxRecordingLength)
				_micClip = TrimAudioClip (_micClip, (int)recordingLength + 1);

			bool availableCarnegiePoints = true;
#if CLIENT_BUILD
			if (PlayerProfile.Instance.SubscriptionType != PlayerProfile.Subscription.Premium && PlayerProfile.SpeechAnalysisIsEnabled == true)
				availableCarnegiePoints = (PlayerProfile.Instance.SpeechAnalysis.Balance > 0);
#endif
			// Send
			if (_sendToCarnegie && availableCarnegiePoints)
				StartCoroutine (OggEncoder ()); // Also sends to carnegie when complete, at OnEncodeComplete (bool)
			if (!availableCarnegiePoints)
			{
#if CLIENT_BUILD
				if (PlayerProfile.Instance.SpeechAnalysis.IsSelfAssessment == false)
				{
                    SpeechAnalysisScreen speechAnalysisScreen = SpeechAnalysisScreen.Create();
                    if (speechAnalysisScreen != null)
                    {
                        speechAnalysisScreen.Show();
                    }
					StartCoroutine (SendErrorXMLAfter (0.3f, "OnCarnegieResults" , CarnegieXMLSender.GenerateErrorXML (""), _carnegieInput));
				}
				else
#endif
				{
					StartCoroutine (SendErrorXMLAfter (0.3f, "OnCarnegieResults" , CarnegieXMLSender.GenerateErrorXML (GenericButton_Record_Notifications.Warnings.selfAssessment), _carnegieInput));
				}	
			}
				
			// Return (for self-assessment)
			return _micClip;
		}
		else
		{
			StartCoroutine (SendErrorXMLAfter (0.2f, "OnCarnegieResults" , CarnegieXMLSender.GenerateErrorXML (GenericButton_Record_Notifications.Warnings.noMicrophone), _carnegieInput));
			return null;
		}
	}
	
	
//-Messaging--------------------------------------------------------------------------------------------------------------------------------------------
	
	private void OnEncodeComplete (bool success)
	{	
		if (Debug.isDebugBuild)
			Debug.Log ("Ogg encoder: Complete!"); 
			
		if (success)
		{
			// Intended to be received by CarnegieXMLSender for XML wrapping and sending to Carnegie servers.
			SendMessage ("EncodingComplete", new object[] {"test.ogg", _carnegieInput}, SendMessageOptions.DontRequireReceiver);
		}
		else
		{
			// Send error result
			SendMessage ("OnCarnegieResults", new object[] {CarnegieXMLSender.GenerateErrorXML (GenericButton_Record_Notifications.Warnings.encodingError), _carnegieInput});
		}
	}

	// Received from CarnegieInterpreter.cs with 
	// the final results of the analysis
	public void OnCarnegieComplete(object[] paramList)
	{
		CarnegieXMLInterpreter.CarnegieFeedback carnegieFeedback = (CarnegieXMLInterpreter.CarnegieFeedback)paramList[0];
#if CLIENT_BUILD
		if (PlayerProfile.Instance.SubscriptionType != PlayerProfile.Subscription.Premium && PlayerProfile.SpeechAnalysisIsEnabled == true)
		{
			if (carnegieFeedback.warningText != GenericButton_Record_Notifications.Warnings.encodingError &&
				carnegieFeedback.warningText != GenericButton_Record_Notifications.Warnings.noInternet &&
				carnegieFeedback.warningText != GenericButton_Record_Notifications.Warnings.noMicrophone &&
				carnegieFeedback.warningText != GenericButton_Record_Notifications.Warnings.noServer &&
			    carnegieFeedback.warningText != GenericButton_Record_Notifications.Warnings.tooLoud &&
			    carnegieFeedback.warningText != GenericButton_Record_Notifications.Warnings.tooSoft &&
			    carnegieFeedback.warningText != GenericButton_Record_Notifications.Warnings.tooNoisy &&
			    carnegieFeedback.warningText != GenericButton_Record_Notifications.Warnings.badRecording &&
				carnegieFeedback.warningText != GenericButton_Record_Notifications.Warnings.selfAssessment)
			{
				PlayerProfile.Instance.SpeechAnalysis.Consume();
			}
		}
#endif
	}
	
//-Privates----------------------------------------------------------------------------------------------------------------------------------------------

	private AudioClip TrimAudioClip (AudioClip clip, int toLength)
	{
		AudioClip trimmedAudioClip;// = new AudioClip();
		trimmedAudioClip = Microphone.Start (_micName, false, toLength, _frequency);
		Microphone.End (_micName);
		
        float[] samples = new float [clip.samples * clip.channels];
        clip.GetData (samples, 0);
		trimmedAudioClip.SetData (samples, 0);
		
		return trimmedAudioClip;
	}

	private void ClearMemory ()
	{
		Destroy (_micClip);
		Resources.UnloadUnusedAssets ();
	}
	
	
//-Coroutines--------------------------------------------------------------------------------------------------------------------------------------------
	
	// Encode to Ogg
	IEnumerator OggEncoder () 
	{
		if (Debug.isDebugBuild)
		Debug.Log ("Ogg encoder: Starting...");
		ove = new OggVorbisEnc(Application.persistentDataPath + "/test.ogg", _micClip, 1.0f, OnEncodeComplete);
		ove.encode();
		
		if (Debug.isDebugBuild)
		Debug.Log ("Ogg encoder: Updating...");

		float currentTime = 0;
		while (currentTime < _encodeTimeout) 
		{
			currentTime += Time.deltaTime;
			if(ove != null && ove.update()) 
				yield break; // done!
			yield return null;
		}
		OnEncodeComplete (false); // timed out
		ove = null;
	}

	IEnumerator SendErrorXMLAfter (float seconds, string message, string errorXML, List<DialogueEventData.DialogueText> carnegieInput)
	{
		if (seconds > 0)
			yield return new WaitForSeconds (seconds);

		SendMessage (message, new object[] { errorXML, carnegieInput });
	}
}
