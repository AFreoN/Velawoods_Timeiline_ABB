using System;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Security.Cryptography;
using Carnegie;
using UnityEngine;
using System.Collections;

using TMPro;
using CoreLib;

public class CarnegieXMLSender : MonoBehaviour
{
	// Updated at EncodingComplete ()
	private List<DialogueEventData.DialogueText> _carnegieInput;
	private string _audioFileName;
	
	private bool _threadSuccessful = false;
	private bool _threadCompleted = false;
	private string _result;
	
	private Thread _thread;
	
	private const float _connectionTimedOutAt = 5.0f; // seconds
	public static string _badConnectionTag = "badconnection";

//-Interface-------------------------------------------------------------------------------------------------------------------

	// Received from CarnegieMicrophone.cs when the mic's AudioClip has been converted to Ogg
	public void EncodingComplete (object[] paramList)
	{
		_audioFileName = (string) paramList [0];
		_carnegieInput = (List<DialogueEventData.DialogueText>) paramList [1];
		
		InternetConnection.CheckConnection (this.gameObject, InternetConnectionCheck);
	}
	
	
//-Internet check-------------------------------------------------------------------------------------------------------------------------------------------------

	public void InternetConnectionCheck (bool isConnected)
	{
		if (isConnected)
		{
			string Xml = GenerateAnalysisRequestXML (_carnegieInput);
			
			if(Xml.Length > 0) {
				Debug.Log ("Sending XML... \n \n" + Xml);
				SendCarnegieData(Xml, "/" + _audioFileName);
			}
			else {
				OnCarnegieResultsReceived (GenerateErrorXML (GenericButton_Record_Notifications.Warnings.noServer));
				Debug.LogWarning ("Carnegie: GenerateAnalysisRequestXML was blank, ignoring");
			}
		}
		else
		{
			OnCarnegieResultsReceived (GenerateErrorXML (GenericButton_Record_Notifications.Warnings.noInternet, _badConnectionTag));
		}
	}
	
	
//-Messaging------------------------------------------------------------------------------------------------------------------------------------------------------
	
	private void OnCarnegieResultsReceived (string result)
	{
		// Intended to be received by CarnegieXMLParser. Processed by CarnegieXMLInterpreter which inherits from CarnegieXMLParser.
		if (Debug.isDebugBuild)
		{
		/* For results exceeding 15000 characters (Debug.Log would truncate the string)
			if (result.Length > 15000)
			{
				Debug.Log("* Carnegie Raw Data * \n \n" + result.Substring (0, 15000));
				Debug.Log("* Carnegie Raw Data * \n \n" + result.Substring (15000));
			}
			else
		*/
			Debug.Log("* Carnegie Raw Data * \n \n" + result);
		}
		SendMessage ("OnCarnegieResults", new object[] {result, _carnegieInput});
	}
	
	
//-Carnegie Thread---------------------------------------------------------------------------------------------------------------------------------------------------------

	// What we have from the Carnegie guys. (Also deleted some methods here which apparently were not being referenced anywhere)
	private void SendCarnegieData (string xml, string audioFileName)
	{
	    DateTime Jan1st1970 = new DateTime (1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		
		//CarnegieBackEnd send = new CarnegieBackEnd ("52.16.249.188", 80); // port forwarding through AWS
		//CarnegieBackEnd send = new CarnegieBackEnd ("speechservices.carnegiespeech.com", 8084); // Singapore server
		CarnegieBackEnd send = new CarnegieBackEnd("speechservices-learndirect.carnegiespeech.com", 80); // US East server
		
		string audioFile = Application.persistentDataPath + audioFileName;
	    /*Validation header info*/
	    //string requester = "CSDev";
		string requester = "LearnDirect";
		string requesterID = "Tok888yo_"+ (int)(UnityEngine.Random.Range(0,100));
		
		string userID = "User_"+ (int)(UnityEngine.Random.Range(0,10000));
		string requesterInfo = "Question_"+ (int)(UnityEngine.Random.Range(0,10000));
		
#if CLIENT_BUILD
		userID = "User_" + PlayerProfile.Instance.ScreenName.Replace (" ", ""); // Send player's screen name
		for (int i=0; i<_carnegieInput.Count; i++) { // For requesterInfo, choose first model answer name found in _carnegieInput (according to the tutor gender selected by the user)
			if (_carnegieInput[i].isCorrect) {
				AudioClip modelAnswer = (PlayerProfile.Instance.Tutor == PlayerProfile.Gender.Male) ? _carnegieInput[i].tutorAudioClips.male : _carnegieInput[i].tutorAudioClips.female;
				if (modelAnswer != null) {
					requesterInfo = "Question_" + modelAnswer.name.Trim ().Replace (" ","");
					break;	
				}
			}
		}
#endif
		
		int requesterSeqNum = (int)(UnityEngine.Random.Range(0,100000000));
		string clientSecretKey = "LD@2014";
		string transactionID = "000000000000000" + (long)(DateTime.UtcNow - Jan1st1970).TotalMilliseconds;
		string aareqString = xml;

		/*Sending single AA Transaction to speech service server*/
		StartCoroutine (checkThread());
		_thread = new Thread (() => CallSingleAATransactionThreaded (requester, requesterID, userID, requesterInfo, requesterSeqNum, aareqString, audioFile, clientSecretKey, transactionID, send));
		_thread.Start ();
	}

	private void CallSingleAATransactionThreaded (string requester, string requesterID, string userID, string requesterInfo, int requesterSeqNum, string aareqString, string audioFileName, string clientSecretKey, string transactionID, CarnegieBackEnd send)
	{
		_result = send.singleAATransaction (out _threadSuccessful, requester, requesterID, userID, requesterInfo, requesterSeqNum, aareqString, audioFileName, clientSecretKey, transactionID);
		_threadCompleted = true;
	}
	
	// Checking for thread complete in a coroutine
	// because we can only use SendMessage from the main thread
	IEnumerator checkThread ()
	{
		float startTime = Time.time;
		float timeElapsed = 0;
	
		while (true) 
		{
			timeElapsed = Time.time - startTime;
		
			if (_threadCompleted) 
			{
				if (_threadSuccessful)
				{
					OnCarnegieResultsReceived (_result);
				}
				else
				{
					OnCarnegieResultsReceived (GenerateErrorXML (GenericButton_Record_Notifications.Warnings.internalError));
				}
				_threadCompleted = false;
				yield break;
			}
			else
			{
				if ((timeElapsed > _connectionTimedOutAt))
				{
					OnCarnegieResultsReceived (GenerateErrorXML (GenericButton_Record_Notifications.Warnings.noServer));
					if (_thread.IsAlive)
						_thread.Abort ();
					_threadCompleted = false;
					yield break;
				}
			}
			yield return null;
		}
	}
	
	
//-XML Generators------------------------------------------------------------------------------------------------
	
	public static string GenerateErrorXML (string errorMsg, string errorTag = "")
	{
		string result = "";
		
		result += "<message><AAResult><error";
		
		if (errorTag != "")
		{
			result += @" tag="""; 
			result += errorTag + @""">";
		}
		else
		{
			result += ">";
		}
		
		result += errorMsg;
		result += "</error></AAResult></message>";
		
		return result;
	}
	
	/// <summary> Creates the XML for reading fluency. </summary>
	protected string GenerateAnalysisRequestXML (List<DialogueEventData.DialogueText> carnegieInput)
	{
		try {
			string readingFluency = @"<AARequest AAReqID=""201"">";
			
			for (int prefix=0; prefix<carnegieInput.Count; prefix++)
			{
				for (int suffix=0; suffix<carnegieInput [prefix].carnegieText.Length; suffix++)
				{
					readingFluency += @"<analysis analysisID=""";
					readingFluency += prefix.ToString () + suffix.ToString ();
					readingFluency += @""" analysisType=""ReadingFluency"" pinpoint=""TRUE"" postprocess=""LD:"">";
					
					readingFluency += @"<text>";
					readingFluency += CoreHelper.removeRichTextTags (carnegieInput [prefix].carnegieText [suffix]);
					readingFluency += @"</text>";
					
					readingFluency += @"</analysis>";
				}
			}
			readingFluency += @"</AARequest>";
			
			return readingFluency;
		}
		catch(NullReferenceException e) {
			Debug.LogWarning (e);
			return ""; // return a blank string on exception
		}
	}
}








