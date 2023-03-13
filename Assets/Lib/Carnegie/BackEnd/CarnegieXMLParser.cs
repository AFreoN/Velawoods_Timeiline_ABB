/// <summary>
/// Carnegie Widget script, this script handles the initalizing of carnegie as well as parsing the data recieved and checking what we should do when 
/// we have the final score of how the learner did. This is the BASE class and so you should derive from it and override the virtual methods to make
/// this script usable from any of your own. (For example, I have a CarnegieWidget_AC script for use in my active conversation activity that overrides 
/// the 'CheckScoreThenRunLogic' function and executes its own logic. 
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System;

using TMPro;
using CoreSystem;

//--------------------------------------------------------------------------------------------------------//
//----------------------------------------MAIN STRUCTS----------------------------------------------------//

/// <summary> Contains all parsed data from the XML received from Carnegie. </summary>
public struct CarnegieParsedData {
	/// <summary> Info on each analysis field. </summary>
	public List<AnalysisParsedData> analysesData;
	/// <summary> To be checked first for potential Carnegie errors. </summary>
	public ErrorState errorState;
	/// <summary> Overall audio quality. </summary>
	public AudioQuality audioQuality;
}

/// <summary> Carnegie analysis reading fluency struct. </summary>
public struct AnalysisParsedData {
	/// <summary> Analysis ID </summary>
	public string ID;
	/// <summary> The overall score of how the user did [0, 1] </summary>
	public float overallScore;  
	/// <summary> The overall pinpoint Score of how the user did [0, 1] </summary>
	public float pinpointScore; 
	/// <summary> The pinpoint status of the string  </summary>
	public PpStatus pinpointStatus;
	/// <summary> Output text containing tags for words that are missing, inserted, etc. (see reading summary) </summary>
	public string hypothesisText; 
	/// <summary> Summary on the status of the words inside the spoken sentence compared to the original one </summary>
	public ReadingSummary summary;
	/// <summary> List of details on each spoken word. </summary>
	public List<ReadingFluencyWord> words;	
}

//----------------------------------------------------------------------------------------------------------------------------------//
//----------------------STRUCTS HOLDING THE CARNEGIE XML PARSED DATA, HELP BUILD THE MAIN STRUCTS ABOVE-----------------------------//

/// <summary>
/// If Carnegie returns an error, this struct stores it. To be checked first. </summary>
public struct ErrorState {
	/// <summary> Is there an error? </summary>
	public bool isError;
	/// <summary> Is there a bad internet/server connection? </summary>
	public bool internetConnection;
	/// <summary> The error message. </summary>
	public string errorMessage;
}

/// <summary>
/// Info on each word: overall score and a list of pinpointInfo 
/// (graphemes, phonemes and their scores)
/// </summary>
public enum AlignType { 
	/// <summary> Free access to all variables. </summary>
	Correct, 
	/// <summary> Please access only: Target Text, Hypothesis Text. </summary>
	Substituted, 
	/// <summary> Please access only: Target Text </summary>
	Deleted, 
	/// <summary> Please access only: Hypothesis text </summary>
	Inserted,
	/// <summary> Please access only: PinPoint Score, which here stores the length of the pause in miliseconds </summary>
	Pause
};
/// <summary>
/// Different states of the ppStatus used for generally evaluating the success of phonemes, words and full strings </summary>
public enum PpStatus {
	Bad = 0,
	Soso,
	Ok
}
/// <summary> Details on each word for Reading Fluency analysis type. 
/// See AlignType HTML Comments for succcessful retrieval of data. </summary>
public struct ReadingFluencyWord {
	/// <summary> The type of the word. See this enum states' html tags for accesibility. </summary>
	public AlignType alignType;
	/// <summary> Original text </summary>
	public string targetText;
	/// <summary> Number of phonemes the word contains, provided regardless of the word's AlignType </summary>
	public int phonemeCount;
	/// <summary> Recorded text as interpreted by Carnegie </summary>
	public string hypothesisText;
	/// <summary> Whole word's score. </summary>
	public float  pinpointScore;
	/// <summary> The pinpoint status of the word  </summary>
	public PpStatus pinpointStatus;
	/// <summary> Per-word details. </summary>
	public List<PinpointInfo> pinpointInfo; //Info on graphemes, phonemes and their score
}
/// <summary> Info on each grapheme, phoneme and their score </summary>
public struct PinpointInfo {
	/// <summary> original letter(s) </summary>
	public string grapheme; 
	/// <summary> spoken letter(s) </summary>
	public string phoneme;  
	/// <summary> 0 to 1 score </summary>
	public float  pinpointScore;
	/// <summary> The pinpoint status of the sound  </summary>
	public PpStatus pinpointStatus;
}
/// <summary> Info on audio quality, usually placed at the end of the carnegie output xml. </summary>
public struct AudioQuality {
	/// <summary> String containing pointers to potential Audio Quality problems. 
	/// Contains: "0" for no Audio Quality problems, "1" for AudioStartTruncated, "2" for AudioTooLoud, "3" for AudioEndTruncated, "4" for AudioTooSoft, "5" for AudioTooNoisy
	/// Extracted raw as binary string, converted appropriately at that time. </summary>
	public string status;
	/// <summary> overall sound volume </summary>
	public float volume;         
}
/// <summary> The info on text and words usually placed at the beginning of the carnegie output xml. Reading Fluency analysis type. </summary>
public struct ReadingSummary {
	/// <summary> original text </summary>
	public string targetText; 
	/// <summary> text with regions of disfluency marked by HMTL tags (colours: yellow, red) </summary>
	public string markedText; 

	/// <summary> no of words inside original text </summary>
	public int nReferenceWordsCount; 
	/// <summary> no of words detected in audio by the analysis </summary>
	public int nHypothesisWordsCount;
	/// <summary> no of words correctly spoken </summary>
	public int nCorrectWordsCount;    
	/// <summary> errors as per minimum edit-distance alignment </summary>
	public int nErrorsCount;          
	/// <summary> no of words in the target text replaced by different words in the output </summary>
	public int nSubstitutionsCount;   
	/// <summary> no of words in the target text completely missing from the output </summary>
	public int nDeletionsCount;       
	/// <summary> no of extraneous words present in the output </summary>
	public int nInsertionsCount;      
}


public class CarnegieXMLParser : MonoBehaviour {

	/// <summary> Structs containing parsed data from the carnegie xml </summary>
	//protected List<CarnegieParsedData> _carnegieParsedData = new List<CarnegieParsedData> ();

	
	//-Interface---------------------------------------------------------------------------------------------------------------------------------------------------------
	
	/// <summary> Called by the 'SendMessage' of CarnegieXMLSender when we get the Carnegie XML from the server. </summary>
	public void OnCarnegieResults (object[] paramList)
	{
		// Get 
		string Xml = (string) paramList [0];
		List<DialogueEventData.DialogueText> carnegieInput = (List<DialogueEventData.DialogueText>) paramList [1];
		
		// Populate structs with parsed data from the xml
		CarnegieParsedData carnegieParsedData = ParseCarnegieResultForReadingFluency (Xml);
		
		// Neatly print extracted xml data
		DebugLogResults (carnegieParsedData);
		// Interpret data for final results (overriden by CarnegieXMLInterpreter)
		RunLogic (carnegieParsedData, carnegieInput);
	}
	
	/// <summary> Method called after the data has been parsed into this class' structs. 
	/// Override this to interpret the data in any way you like. :) </summary>
	protected virtual void RunLogic(CarnegieParsedData carnegieParsedData, List<DialogueEventData.DialogueText> carnegieInput) { 
		//Override me.
	}

	
	//-Parsers----------------------------------------------------------------------------------------------------------------------------------------------------------

	/// <summary> Parses the carnegie result for reading fluency. </summary>
	protected virtual CarnegieParsedData ParseCarnegieResultForReadingFluency(string Xml){
        Debug.LogError(Xml);
		CarnegieParsedData carnegieParsedData = new CarnegieParsedData ();

		XmlDocument xmlDoc = new XmlDocument ();
        try
        {
            xmlDoc.LoadXml(Xml);
        }
        catch(Exception e)
        {
            Debug.LogError("CarnegieWidgetScript: Exception caught when parsin the XML for Reading Fluency. Exception as follows: " + e);
            carnegieParsedData.errorState.isError = true;
            carnegieParsedData.errorState.errorMessage = "SErver Error.";
            return carnegieParsedData;
        }
		try { // Catch any errors
		
			//If error occurs, store it and return
			carnegieParsedData.errorState.internetConnection = true;
			carnegieParsedData.errorState.isError = false;
			if (xmlDoc.SelectSingleNode ("message/AAResult/error") != null) {
				Debug.LogWarning ("CARNEGIE - " + xmlDoc.SelectSingleNode ("message/AAResult/error").InnerText);
				// Record error and return
				carnegieParsedData.errorState.isError = true;
				carnegieParsedData.errorState.errorMessage = xmlDoc.SelectSingleNode ("message/AAResult/error").InnerText;
				// Is there a bad internet connection?
				if (xmlDoc.SelectSingleNode ("message/AAResult/error").Attributes ["tag"] != null) {
					if (xmlDoc.SelectSingleNode ("message/AAResult/error").Attributes ["tag"].Value == CarnegieXMLSender._badConnectionTag) {
						carnegieParsedData.errorState.internetConnection = false;
					}
				}
				
				return carnegieParsedData;
			}
			carnegieParsedData.errorState.isError = false;
			carnegieParsedData.analysesData = new List<AnalysisParsedData> ();
			
			XmlNodeList analysisFields = xmlDoc.SelectNodes ("message/AAResult/analysis");
			foreach (XmlNode analysisField in analysisFields)
			{
				AnalysisParsedData analysisParsedData = new AnalysisParsedData ();
				
				//Get main values: overall score, ppScore, HypText
				analysisParsedData.ID = analysisField.Attributes ["analysisID"].Value;
				analysisParsedData.overallScore = float.Parse(analysisField.SelectSingleNode("score").InnerText);
				if (analysisField.SelectSingleNode ("SRHyp").Attributes.Count > 0) {
					analysisParsedData.pinpointScore = float.Parse (analysisField.SelectSingleNode ("SRHyp").Attributes ["ppScore"].Value);
					switch (analysisField.SelectSingleNode ("SRHyp").Attributes ["ppStatus"].Value)
					{
					case "ok": analysisParsedData.pinpointStatus = PpStatus.Ok; break;
					case "soso": analysisParsedData.pinpointStatus = PpStatus.Soso; break;
					case "bad": analysisParsedData.pinpointStatus = PpStatus.Bad; break;
					default: 
						Debug.LogError ("CarnegieWidgetScript: ppStatus not recognised! Please check the received Carnegie xml");
						analysisParsedData.pinpointStatus = PpStatus.Bad;
						break; 
					}
				}
				else {
					analysisParsedData.pinpointScore = 0;
					analysisParsedData.pinpointStatus = PpStatus.Bad;
				}
				analysisParsedData.hypothesisText = analysisField.SelectSingleNode ("SRHyp").InnerText;
	
				//Reading Summary parser
				XmlNode readingSummaryNode = analysisField.SelectSingleNode ("readingSummary");
				foreach (XmlNode readingSummaryChildNode in readingSummaryNode) {
		
					string xmlValue = readingSummaryChildNode.InnerText;
					switch (readingSummaryChildNode.Name) {
		
						case "targetText": analysisParsedData.summary.targetText = xmlValue; break;
						case "markedText": //Here, the marked text is given the tags <i> ... </i>
						//Get main text
						analysisParsedData.summary.markedText = analysisParsedData.summary.targetText;
						//Get all marked nodes
						XmlNodeList tagNodes = readingSummaryChildNode.SelectNodes ("font");
						foreach (XmlNode tagNode in tagNodes) {
							//Get position and length of text inside target text
							int at = analysisParsedData.summary.markedText.IndexOf (tagNode.InnerText);
							int len = tagNode.InnerText.Length;
							//Add tags
							analysisParsedData.summary.markedText = analysisParsedData.summary.markedText.Insert (at + len, "</i>");
							analysisParsedData.summary.markedText = analysisParsedData.summary.markedText.Insert (at,       "<i>");
						}
						break;
		
						case "nRef" : analysisParsedData.summary.nReferenceWordsCount  = int.Parse(xmlValue); break;
						case "nHyp" : analysisParsedData.summary.nHypothesisWordsCount = int.Parse(xmlValue); break;
						case "nCorr": analysisParsedData.summary.nCorrectWordsCount    = int.Parse(xmlValue); break;
						case "nErr" : analysisParsedData.summary.nErrorsCount          = int.Parse(xmlValue); break;
						case "nSub" : analysisParsedData.summary.nSubstitutionsCount   = int.Parse(xmlValue); break;
						case "nDel" : analysisParsedData.summary.nDeletionsCount       = int.Parse(xmlValue); break;
						case "nIns" : analysisParsedData.summary.nInsertionsCount      = int.Parse(xmlValue); break;
					}
				}
	
				//Init words list
				analysisParsedData.words = new List<ReadingFluencyWord>();
	
				//Word details parser
				XmlNodeList wordNodes = analysisField.SelectNodes ("alignmentDetails/alignEntry");
				foreach (XmlNode wordNode in wordNodes) {
					//Init word
					ReadingFluencyWord newWord = new ReadingFluencyWord();
					newWord.pinpointInfo = new List<PinpointInfo>();
		
					//Parse data according to the alignmentType
					switch (wordNode.Attributes["alignType"].Value) {
						case "OK": //Correct word
							newWord.alignType      = AlignType.Correct;
							newWord.targetText     = wordNode.SelectSingleNode("target").InnerText;
							newWord.phonemeCount = int.Parse (wordNode.SelectSingleNode("target").Attributes["numPh"].Value);
							newWord.hypothesisText = wordNode.SelectSingleNode("wordSeg/word").InnerText;
							newWord.pinpointScore  = float.Parse (wordNode.SelectSingleNode("wordSeg/word").Attributes["ppScore"].Value);
		
							switch (wordNode.SelectSingleNode("wordSeg/word").Attributes["ppStatus"].Value)
							{
							case "ok": newWord.pinpointStatus = PpStatus.Ok; break;
							case "soso": newWord.pinpointStatus = PpStatus.Soso; break;
							case "bad": newWord.pinpointStatus = PpStatus.Bad; break;
							default: 
								Debug.LogError ("CarnegieWidgetScript: ppStatus not recognised! Please check the received Carnegie xml");
								newWord.pinpointStatus = PpStatus.Bad;
								break; 
							}
		
							//Details on each sound (grapheme, phoneme, sound score)
							XmlNodeList wordInfo = wordNode.SelectNodes ("wordSeg/pinpointInfo/grapheme");
							foreach (XmlNode grapheme in wordInfo) {
								PinpointInfo newPinpointInfo  = new PinpointInfo();
								newPinpointInfo.grapheme      = grapheme.Attributes["str"].Value;
								
								if (grapheme.SelectSingleNode("phoneme") == null)
								{
									Debug.Log ("ERROR DETECTED AT GRAPHEME: " + newPinpointInfo.grapheme);
									newPinpointInfo.phoneme = newPinpointInfo.grapheme;
									newPinpointInfo.pinpointScore = newWord.pinpointInfo [newWord.pinpointInfo.Count - 1].pinpointScore;
									newPinpointInfo.pinpointStatus = newWord.pinpointInfo [newWord.pinpointInfo.Count - 1].pinpointStatus;
								}
								else
								{
									newPinpointInfo.phoneme = grapheme.SelectSingleNode ("phoneme").Attributes ["name"].Value;
									newPinpointInfo.pinpointScore = float.Parse (grapheme.SelectSingleNode ("phoneme").Attributes ["score"].Value);
									switch (grapheme.SelectSingleNode ("phoneme").Attributes ["status"].Value)
									{
									case "ok": newPinpointInfo.pinpointStatus = PpStatus.Ok; break;
									case "soso": newPinpointInfo.pinpointStatus = PpStatus.Soso; break;
									case "bad": newPinpointInfo.pinpointStatus = PpStatus.Bad; break;
									default: 
										Debug.LogError ("CarnegieWidgetScript: ppStatus not recognised! Please check the received Carnegie xml");
										newPinpointInfo.pinpointStatus = PpStatus.Bad;
										break; 
									}
								}
								
								newWord.pinpointInfo.Add (newPinpointInfo);
							}
							break;
						case "SUB": //Substituted word
							newWord.alignType  = AlignType.Substituted; 
							newWord.targetText = wordNode.SelectSingleNode("target").InnerText;
							newWord.phonemeCount = int.Parse (wordNode.SelectSingleNode("target").Attributes["numPh"].Value);
							newWord.hypothesisText = wordNode.SelectSingleNode("wordSeg/word").InnerText;
							break;
						case "DEL": //Deleted word
							newWord.alignType  = AlignType.Deleted;  
							newWord.targetText = wordNode.SelectSingleNode("target").InnerText;
							newWord.phonemeCount = int.Parse (wordNode.SelectSingleNode("target").Attributes["numPh"].Value);
							break;
						case "INS": //Inserted word
							newWord.alignType      = AlignType.Inserted;  
							newWord.hypothesisText = wordNode.SelectSingleNode("wordSeg/word").InnerText;
							break;
						case "pause": //Comma/full stop/silence in user's speech
							newWord.alignType 	  = AlignType.Pause;
							newWord.pinpointScore = float.Parse (wordNode.Attributes["value"].Value);
							break;
					}
					//Add values to main structure
					analysisParsedData.words.Add (newWord);
				}
				
			carnegieParsedData.analysesData.Add (analysisParsedData);
			}
			
			//--Audio Quality Parser------------------------------------------------------------------------------
			XmlNode audioQualityNode = xmlDoc.SelectSingleNode ("message/AAResult/audioQuality");
			carnegieParsedData.audioQuality.volume = float.Parse (audioQualityNode.Attributes ["vol"].Value);
			carnegieParsedData.audioQuality.status = parseStatusValue (audioQualityNode.Attributes ["status"].Value);
			//------------------------------------------------------------------------------------------------------
			
			return carnegieParsedData;
		
		//**
		} 
		catch (Exception e) {
			Debug.LogError ("CarnegieWidgetScript: Exception caught when parsin the XML for Reading Fluency. Exception as follows: " + e); 
			carnegieParsedData.errorState.isError = true;
			carnegieParsedData.errorState.errorMessage = "Parsing Error.";
			return carnegieParsedData;
		}
		//**

	}
	
	/// <summary>
	/// .status comes as a 5-letter binary string. Converting so that it matches the status' description in the Carnegie Documentation.
	/// Please go to status' declaration inside the AudioQuality struct for insight.
	/// </summary>
	string parseStatusValue (string status) {
		if (status != "0") {
			//Get string, convert to 4*n binary string
			status = CoreHelper.hexToBinary (status);
			//Convert to 5-letter binary so it fits the Carnegie documentation requirements
			if      (status.Length == 4) { status = status.Insert (0, "0"); }
			else if (status.Length > 4)  { status = status.Substring (status.Length - (5), 5); }
			else                         { Debug.LogError ("CarnegieWidgetScript: Unexpected status value!"); }
			//Convert from binary to fit .status' description. See declaration for insight.
			string result = "";
			for (int i=status.Length-1; i>-1; i--)
				if (status [i] == '1')
					result += (status.Length - i).ToString ();
			return result;
		}
		else { return status; }
	}


	//-Printing------------------------------------------------------------------------------------------------------------------------------------------------------

	/// <summary> Nicely Debug.Log the parsed data. (ReadingFluency only atm) </summary>
	protected void DebugLogResults (CarnegieParsedData carnegieParsedData) {
		string output = "* Carnegie Parsed Data * \n \n";
		string status = "";

		output += "<General>" + "\n";
		output += "-----------" + "\n";
		
		if (carnegieParsedData.errorState.isError)
		{
			output += carnegieParsedData.errorState.errorMessage + "\n";
			output += "Internet connection: " + carnegieParsedData.errorState.internetConnection;
			output += "No further data available." + "\n";
			output += "-----------" + "\n";
			
			Debug.Log (output);
			return;
		}
		
		foreach (AnalysisParsedData analysisResults in carnegieParsedData.analysesData)
		{
			output += "\n";
			output += "Analysis ID: " + analysisResults.ID + "\n";
			output += "Target Text: "        + analysisResults.summary.targetText + "\n";
			output += "-------------------" + "\n";
			output += "PinPoint Score: "  + analysisResults.pinpointScore  + "\n";
			output += "PinPoint Status: "  + analysisResults.pinpointStatus  + "\n";
			output += "Overall Score: "   + analysisResults.overallScore   + "\n";
			output += "Hypothesis Text: " + analysisResults.hypothesisText + "\n";
			output += "\n";
			output += "<Reading Summary>" + "\n";
			output += "-------------------" + "\n";
			output += "Marked Text: "        + analysisResults.summary.markedText            + "\n";
			output += "nReference Words: "   + analysisResults.summary.nReferenceWordsCount  + "\n";
			output += "nHypothesis Words: "  + analysisResults.summary.nHypothesisWordsCount + "\n";
			output += "nCorrect Words: "     + analysisResults.summary.nCorrectWordsCount    + "\n";
			output += "nError Words: "       + analysisResults.summary.nErrorsCount          + "\n";
			output += "nSubstituted Words: " + analysisResults.summary.nSubstitutionsCount   + "\n";
			output += "nDeleted Words: "     + analysisResults.summary.nDeletionsCount       + "\n";
			output += "nInserted Words: "    + analysisResults.summary.nInsertionsCount      + "\n";
			output += "\n";
			output += "<Words Info>" + "\n";
			output += "--------------" + "\n";
			for (int i=0; i<analysisResults.words.Count; i++) {
	
				switch (analysisResults.words[i].alignType) {
	
				case AlignType.Correct:
					output += "Align Type: "      + "Ok/Correct" + "\n";
					output += "Target Text: "     + analysisResults.words[i].targetText     + "\n";
					output += "Phoneme Count: " + analysisResults.words[i].phonemeCount + "\n";
					output += "Hypothesis Text: " + analysisResults.words[i].hypothesisText + "\n";
					output += "PinPoint Score: "  + analysisResults.words[i].pinpointScore  + "\n";
					output += "PinPoint Status: "  + analysisResults.words[i].pinpointStatus  + "\n";
					output += "<details>" + "\n";
	
					int count = analysisResults.words[i].pinpointInfo.Count;
					for (int j=0; j < count; j++) {
						output += "    Grapheme: " + analysisResults.words[i].pinpointInfo[j].grapheme      + "\n";
						output += "    Phoneme: "  + analysisResults.words[i].pinpointInfo[j].phoneme       + "\n";
						output += "    Score: "    + analysisResults.words[i].pinpointInfo[j].pinpointScore + "\n";
						output += "    Status: "    + analysisResults.words[i].pinpointInfo[j].pinpointStatus + "\n";
						if (j!=count - 1) 
							output += "***" + "\n";
					}
					output += "</details>" + "\n";
					output += "\n";
					break;
				case AlignType.Deleted:
					output += "Align Type: "  + "Deleted" + "\n";
					output += "Target Text: " + analysisResults.words[i].targetText + "\n";
					output += "Phoneme Count: " + analysisResults.words[i].phonemeCount + "\n";
					output += "\n";
					break;
				case AlignType.Inserted:
					output += "Align Type: "      + "Inserted" + "\n";
					output += "Hypothesis Text: " + analysisResults.words[i].hypothesisText + "\n";
					output += "Phoneme Count: " + analysisResults.words[i].phonemeCount + "\n";
					output += "\n";
					break;
				case AlignType.Substituted:
					output += "Align Type: "      + "Substituted" + "\n";
					output += "Target Text: "     + analysisResults.words[i].targetText     + "\n";
					output += "Hypothesis Text: " + analysisResults.words[i].hypothesisText + "\n";
					output += "Phoneme Count: " + analysisResults.words[i].phonemeCount + "\n";
					output += "\n";
					break;
				case AlignType.Pause:
					output += "Align Type: " + "Pause" + "\n";
					output += "Duration: "   + analysisResults.words[i].pinpointScore + "\n";
					output += "\n";
					break;
				}
			}
		}
		output += "\n";
		output += "<Audio Quality>" + "\n";
		output += "--------------" + "\n";
		output += "Volume: " + carnegieParsedData.audioQuality.volume + "\n";
		output += "Status: " + carnegieParsedData.audioQuality.status + "\n";
		output += "Audio Quality Problems: ";
		
		status = carnegieParsedData.audioQuality.status;
		if (status == "0")
		{
			output += "None";
		}
		else {
			if (status.Contains ("1"))
			{
				output += "Audio truncated at the start. ";
			}
			if (status.Contains ("2"))
			{
				output += "Audio is too loud. ";
			}
			if (status.Contains ("3"))
			{
				output += "Audio truncated at the end. ";
			}
			if (status.Contains ("4"))
			{
				output += "Audio too soft. ";
			}
			if (status.Contains ("5"))
			{
				output += "Audio too noisy. ";
			}
		}
		output += "\n";
		output += "--------------" + "\n";
		output += "\n";
		
		if (Debug.isDebugBuild)
		Debug.Log (output);
	}
}
