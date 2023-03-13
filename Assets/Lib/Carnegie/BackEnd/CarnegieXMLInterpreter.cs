using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

using CoreSystem;

[RequireComponent (typeof (CarnegieXMLSender))]
public class CarnegieXMLInterpreter : CarnegieXMLParser {

//-Public parameters
	
	//[Header ("Colour tags")]
	private Color _okColor   = new Color (10.0f/255.0f, 161.0f/255.0f, 5.0f/255.0f);
	private Color _sosoColor = new Color (255.0f/255.0f, 168.0f/255.0f, 0);
	private Color _badColor  = new Color (195.0f/255.0f, 0.0f/255.0f, 21.0f/255.0f);
	private Color _substitutedColor = new Color (195.0f/255.0f, 0.0f/255.0f, 21.0f/255.0f);
	private Color _deletedColor     = new Color (195.0f/255.0f, 0.0f/255.0f, 21.0f/255.0f);
	
	//[Header ("Focus")]
	private CheckStatus _checkStatus = CheckStatus.Words;
	public enum CheckStatus { Phonemes, Words, String }
	
	//[Header ("Extra Settings")]
	private bool _ignoreInserts = true;
	
	//[Header ("Thresholds")]
	private float    _masterThreshold = 0.76f;
	private PpStatus _statusThreshold = PpStatus.Soso;
	private float _maxPauseLimit = 1000; //ms
	
	
	public struct CarnegieFeedback
	{
		/// <summary> Index of the sentence with the highest score inside the 'sentences' list of this struct. </summary>
		public int winningSentenceIndex;
		/// <summary> Feedback details on each sentence in the order of the carnegie input. </summary>
		public List<CarnegieFeedbackPerSentence> sentences; 
		/// <summary> Warning related to errors or audio quality problems. </summary>
		public string warningText;
		/// <summary> Error flag. Check this first and if true, the error is stored in warningText and there is no per-sentence data. </summary>
		public bool isError;
		/// <summary> Bad connection flag. Switches isError as well and stores a warningText. </summary>
		public bool internetConnection;
		/// <summary> AudioClip as recorded by the microphone (not encoded) </summary>
		public AudioClip micClip;
	}
	public struct CarnegieFeedbackPerSentence
	{
		/// <summary> Overall score of the sentence. </summary>
		public float score;
		/// <summary> Carnegie-generated score. Used for differentiating between sentences with the same 'score' value. </summary>
		public float pinpointScore;
		/// <summary> Does the sentence exceed the Carnegie master threshold? </summary>
		public bool success;
		/// <summary> Is the sentence considered a correct answer? </summary>
		public bool isCorrect;
		/// <summary> Colour tagged version of the sentence. </summary>
		public string richText;
	}	
	
//-Private parameters
	
	private string _richText;
	private int    _letterIndex;
	
	private string[] _okTag   = new string[] {"<>", "</color>"};
	private string[] _sosoTag = new string[] {"<>", "</color>"};
	private string[] _badTag  = new string[] {"<><u>", "</u></color>"};
	
	private string[] _substitutedTag = new string[] {"<><u>", "</u></color>"};
	private string[] _deletedTag     = new string[] {"<><u>", "</u></color>"};
	
	
	public void Start ()
	{
		setUpTags ();
	}
	
//-Messaging--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
	
	/// <summary> This is to let any script attached to this object know that the script has finished interpreting the XML data 
	/// and also sends out the results with the message. 
	/// Results come as: bool isSuccessful, string richText, string warningText; </summary>
	private void OnComplete (CarnegieFeedback carnegieFeedback)
	{	
		DebugLogFeedback (carnegieFeedback);	
		SendMessage ("OnCarnegieComplete", new object[] {carnegieFeedback}); 
	}

	
//-Interpreters and parsers----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
	
	/// <summary> Takes in the data stored in the base class CarnegieXMLParser and sends a CarnegieFeedback struct through OnComplete(...). </summary>
	protected override void RunLogic(CarnegieParsedData carnegieParsedData, List<DialogueEventData.DialogueText> carnegieInput) 
	{
		// Struct to be sent to listeners and recButton
		CarnegieFeedback carnegieFeedback = new CarnegieFeedback ();
		carnegieFeedback.sentences = new List<CarnegieFeedbackPerSentence> ();
	
		try {
			// Set this to be negative, record the highest score for each sentence
			float winningSentenceScore = -1;
			float winningSentencePPScore = -1; // winning sentence pinpoint score. to be used for differentiating between sentences with the same 'score' value
			
			// If error, return and send error message
			carnegieFeedback.internetConnection = carnegieParsedData.errorState.internetConnection;
			carnegieFeedback.isError = carnegieParsedData.errorState.isError;
			if (carnegieParsedData.errorState.isError)
			{
				carnegieFeedback.winningSentenceIndex = -1;
				carnegieFeedback.warningText = carnegieParsedData.errorState.errorMessage;
				
				OnComplete (carnegieFeedback);
				return;
			}
			
			int analysisParsedDataIndex = 0;
			// Go through each struct of dialogueEventData.DialogueText and fill in the feedback struct
			for (int sentenceIndex=0; sentenceIndex<carnegieInput.Count; sentenceIndex++)
			{
				CarnegieFeedbackPerSentence feedbackPerSentece = new CarnegieFeedbackPerSentence (); // Feedback per sentence to be added to the main feedback struct at the end of next loop
				feedbackPerSentece.score = -1; // Set the score to be negative and record the highest score in the next loop
				feedbackPerSentece.pinpointScore = -1; // Set the score to be negative and record the highest score in the next loop
				
				// Go through each carnegie string of the dialogueEventData.DialogueText and choose the best fit (highest score) for its main text 
				for (int carnegieSenteceIndex=0; carnegieSenteceIndex<carnegieInput[sentenceIndex].carnegieText.Length; carnegieSenteceIndex++)
				{
					// Create new per-sentence struct to be assigned to the feedbackPerSentence (above) if the final score is greater
					CarnegieFeedbackPerSentence feedbackPerCarnegieSentence = new CarnegieFeedbackPerSentence ();
					feedbackPerCarnegieSentence.isCorrect = carnegieInput[sentenceIndex].isCorrect;
					feedbackPerCarnegieSentence.success = true; // Set initial success to true and change to false if any errors occur
				
					// Per-sentence analysis struct
					AnalysisParsedData analysisParsedData = carnegieParsedData.analysesData [analysisParsedDataIndex];
					
					float checkCount = 0.0f; //Keeping track of the elements that need to be checked (words or phonemes). 
					float passCount = 0.0f; // Keeping track of the number of checked elements that are above the statusThreshold
					
					_richText = carnegieInput[sentenceIndex].carnegieOriginalText; // Copy original text into string which will be html-tagged
					if (_richText == null || _richText.Length == 0)
						_richText = carnegieInput[sentenceIndex].text;
					
					_letterIndex = 0; // Position in sentence (for tagging)
					
					// If the full string is being checked, do it and raise warning flag if the status is below the _statusThreshold
					if (_checkStatus == CheckStatus.String && analysisParsedData.pinpointStatus < _statusThreshold)
					{
						feedbackPerCarnegieSentence.success = false;
					}
					
					// Loop through all words, place colour tags and check thresholds if necessary (words and phonemes). WordIndex keeps track of the word we're at.
					for (int wordIndex=0; wordIndex<analysisParsedData.words.Count; wordIndex++) 
					{
						// Check alignment (word status)
						switch (analysisParsedData.words [wordIndex].alignType)
						{
						case AlignType.Correct:
							//If words are being checked
							if (_checkStatus == CheckStatus.Words)
							{  
								checkCount ++; // Add to word count
								if (analysisParsedData.words [wordIndex].pinpointStatus >= _statusThreshold)
								{
									passCount ++; //Add to elements that have passed the check 
								}
							}
							
							//If sounds are being checked
							if (_checkStatus == CheckStatus.Phonemes)
							{
								checkCount += analysisParsedData.words [wordIndex].phonemeCount;
								//Go through each sound
								for (int i=0; i<analysisParsedData.words [wordIndex].pinpointInfo.Count; i++)
								{
									if (analysisParsedData.words [wordIndex].pinpointInfo [i].pinpointStatus >= _statusThreshold)
									{ 
										passCount ++; //Add to elements that have passed the check 
									}
								}
							}
							
							// Insert colour tags.
							string word = goToWord (analysisParsedData, wordIndex);
							
							if (word == null || word.Length == 0)
								break;
								
							// If the word in the displayed string differs from the one sent to Carnegie (ex: 0 -> zero)
							if (word != analysisParsedData.words [wordIndex].targetText)
							{
                                PpStatus statusToCheck = analysisParsedData.words[wordIndex].pinpointStatus;

                                if (_stringIsTime)
                                {
                                    int Score = (int)statusToCheck;
                                    ++wordIndex;
                                    Score += (int)analysisParsedData.words[wordIndex].pinpointStatus;

                                    int result = 0;

                                    if(_numberOfWordsInTime == 3)
                                    {
                                        ++wordIndex;
                                        Score += (int)analysisParsedData.words[wordIndex].pinpointStatus;

                                        result = Score / 3;
                                    }
                                    else
                                    {
                                        result = Score / 2;
                                    }

                                    statusToCheck = (PpStatus)(result);
                                }

								if (_isFourLetterNumber)
								{
									statusToCheck = GetFourLetterNumberStatus (analysisParsedData, ref wordIndex, ref checkCount, ref passCount);
									_isFourLetterNumber = false;
								}

                                switch (statusToCheck)
                                {
                                    case PpStatus.Ok: _letterIndex += insertTags(_letterIndex, word.Length, _okTag); break;
                                    case PpStatus.Soso: _letterIndex += insertTags(_letterIndex, word.Length, _sosoTag); break;
                                    case PpStatus.Bad: _letterIndex += insertTags(_letterIndex, word.Length, _badTag); break;
                                }
							}
							else //Colour each grapheme
							{
								for (int i=0; i<analysisParsedData.words [wordIndex].pinpointInfo.Count; i++)
								{
                                    int wordLength = CheckForpunctuation(analysisParsedData.words[wordIndex].pinpointInfo[i].grapheme.Length);

									PpStatus check = (_checkStatus == CheckStatus.Phonemes) ? analysisParsedData.words [wordIndex].pinpointInfo [i].pinpointStatus : analysisParsedData.words [wordIndex].pinpointStatus;
									switch (check)
									{
                                        case PpStatus.Ok: _letterIndex += insertTags(_letterIndex, wordLength, _okTag); break;
                                        case PpStatus.Soso: _letterIndex += insertTags(_letterIndex, wordLength, _sosoTag); break;
                                        case PpStatus.Bad: _letterIndex += insertTags(_letterIndex, wordLength, _badTag); _hasGotUnderlinesInString = true; break;
									}
								}
							}
							break;
							
						case AlignType.Inserted:
							if (_checkStatus == CheckStatus.String) break;
							if (_ignoreInserts) break;
							
							//If word is not first or last (sometimes Carnegie adds junk at the beginning or end), raise warning
							//if (! (wordIndex == 0 || wordIndex == _analysisReadingFluency.words.Count - 1))
							feedbackPerCarnegieSentence.success = false;
							carnegieFeedback.warningText = GenericButton_Record_Notifications.Warnings.badRecording;		
							break;
							
						case AlignType.Deleted:
							if (_checkStatus == CheckStatus.String) break;
							
							//Colour tags
							string deletedWord = goToWord (analysisParsedData, wordIndex);

                            if (_stringIsTime)
                            {
                                ++wordIndex;
                                if (_numberOfWordsInTime == 3)
                                {
                                    ++wordIndex;
                                }
                            }

                            if (deletedWord != null && deletedWord.Length > 0)
                            {
                                int wordLength = CheckForpunctuation(deletedWord.Length);

								if (_isFourLetterNumber)
								{
									_isFourLetterNumber = false;
									switch (GetFourLetterNumberStatus (analysisParsedData, ref wordIndex, ref checkCount, ref passCount))
									{
										case PpStatus.Ok: _letterIndex += insertTags(_letterIndex, deletedWord.Length, _okTag); break;
										case PpStatus.Soso: _letterIndex += insertTags(_letterIndex, deletedWord.Length, _sosoTag); break;
										case PpStatus.Bad: _letterIndex += insertTags(_letterIndex, deletedWord.Length, _badTag); break;
									}
								}
								else
								{
									_letterIndex += insertTags(_letterIndex, wordLength, _deletedTag);
								}
                                _hasGotUnderlinesInString = true;
                            }
							
							//If checking words, treat it as 'bad'
							if (_checkStatus == CheckStatus.Words)
							{
								checkCount ++;
								
								if (_statusThreshold == PpStatus.Bad)
									passCount ++;
							}
							//If checking phonemes, treat it as 'bad'
							if (_checkStatus == CheckStatus.Phonemes)
							{
								checkCount += analysisParsedData.words [wordIndex].phonemeCount;
								
								if (_statusThreshold == PpStatus.Bad)
									passCount += analysisParsedData.words [wordIndex].phonemeCount;
							}
							break;
							
						case AlignType.Substituted:
							if (_checkStatus == CheckStatus.String) break;
							
							//Colour tags
							string substitutedWord = goToWord (analysisParsedData, wordIndex);

                            if (_stringIsTime)
                            {
                                ++wordIndex;
                                if (_numberOfWordsInTime == 3)
                                {
                                    ++wordIndex;
                                }
                            }

                            if (substitutedWord != null && substitutedWord.Length > 0)
                            {
                                int wordLength = CheckForpunctuation(substitutedWord.Length);

								if (_isFourLetterNumber)
								{
									_isFourLetterNumber = false;
									switch (GetFourLetterNumberStatus (analysisParsedData, ref wordIndex, ref checkCount, ref passCount))
									{
										case PpStatus.Ok: _letterIndex += insertTags(_letterIndex, substitutedWord.Length, _okTag); break;
										case PpStatus.Soso: _letterIndex += insertTags(_letterIndex, substitutedWord.Length, _sosoTag); break;
										case PpStatus.Bad: _letterIndex += insertTags(_letterIndex, substitutedWord.Length, _badTag); break;
									}
								}
								else
								{
									_letterIndex += insertTags(_letterIndex, wordLength, _substitutedTag);
								}
                               
                                _hasGotUnderlinesInString = true;
                            }
							
							//If checking words, treat it as 'bad'
							if (_checkStatus == CheckStatus.Words)
							{
								checkCount ++;
								
								if (_statusThreshold == PpStatus.Bad)
									passCount ++;
							}
							//If checking phonemes, treat it as 'bad'
							if (_checkStatus == CheckStatus.Phonemes)
							{
								checkCount += analysisParsedData.words [wordIndex].phonemeCount;
								
								if (_statusThreshold == PpStatus.Bad)
									passCount += analysisParsedData.words [wordIndex].phonemeCount;
							}
							break;
							
						case AlignType.Pause:
							//If pause longer than allowed limit, display message
							if (analysisParsedData.words [wordIndex].pinpointScore > _maxPauseLimit)
								feedbackPerCarnegieSentence.success = false;
							break;
						}
					}
				
					// Assign score and rich text 
					feedbackPerCarnegieSentence.score = passCount / checkCount;
					// Get pinpoint score
					feedbackPerCarnegieSentence.pinpointScore = analysisParsedData.pinpointScore;
					// Set tagged text
					feedbackPerCarnegieSentence.richText = _richText;
					
					// Set success
					if (passCount / checkCount < _masterThreshold)
					{
						feedbackPerCarnegieSentence.success = false;
					}
					// Record highest score
					if (feedbackPerCarnegieSentence.score > feedbackPerSentece.score)
					{
						feedbackPerSentece = feedbackPerCarnegieSentence;
					}
					else
						// If scores are equal, check pinpoint score for more accuracy
						if (feedbackPerCarnegieSentence.score == feedbackPerSentece.score)
						{
							if (feedbackPerCarnegieSentence.pinpointScore > feedbackPerSentece.pinpointScore)
							{
								feedbackPerSentece = feedbackPerCarnegieSentence;
							}
						}
					// Jump to next analysis struct
					analysisParsedDataIndex ++;
				}
				
				// Add highest score carnegieText analysis to the feedback's sentence list
				carnegieFeedback.sentences.Add (feedbackPerSentece);
				
				// Set highest score index if applicable 
				if (feedbackPerSentece.score > winningSentenceScore)
				{
					carnegieFeedback.winningSentenceIndex = sentenceIndex; 
					winningSentenceScore = feedbackPerSentece.score;
					winningSentencePPScore = feedbackPerSentece.pinpointScore;
				}
				else 
					// If scores are equal, check PinPoint scores for more accuracy.
					if (feedbackPerSentece.score == winningSentenceScore)
					{
						if (feedbackPerSentece.pinpointScore > winningSentencePPScore)
						{
							carnegieFeedback.winningSentenceIndex = sentenceIndex; 
							winningSentenceScore = feedbackPerSentece.score;
							winningSentencePPScore = feedbackPerSentece.pinpointScore;
						}
					}
			}
	
			//If the audioQualityField returned errors, we now show them directly. Truncated errors are not shown
			carnegieFeedback.warningText = interpretAudioQualityField (carnegieParsedData.audioQuality);
		}
		
		catch (System.Exception e)
		{
			Debug.LogWarning ("CarnegieXMLInterpreter : " + e);
			
			carnegieFeedback.isError = true;
			carnegieFeedback.winningSentenceIndex = -1;
			carnegieFeedback.warningText = "";
			
			OnComplete (carnegieFeedback);
			return;
		}
		
		//Send out results
		OnComplete (carnegieFeedback);
	}
	
	/// <summary>  Returns a string containing the quality issue if any audio quality problems occured. Now ignoring truncated errors. Blank string means there are no quality issues to worry about </summary>
	private string interpretAudioQualityField (AudioQuality audioQuality)
	{
		//if (audioQuality.volume < _minVolumeLimit) { return true; }	// HUG: removed
		if (audioQuality.status != "0") {
			switch (audioQuality.status [0]) {
			case '1': return "";			// audio too short
			case '2': return GenericButton_Record_Notifications.Warnings.tooLoud;	// audio too loud
			case '3': return "";			// audio truncated at the end
			case '4': return GenericButton_Record_Notifications.Warnings.tooSoft;		// audio too soft
			case '5': return GenericButton_Record_Notifications.Warnings.tooNoisy;		// audio too noisy
			}
		}
		return "";
	}
	
	
//-Privates-------------------------------------------------------------------------------------------------------------------------------------------
	
	/// <summary> Initialise rich text tags according to the user-driven params. </summary>
	private void setUpTags ()
	{
		_okTag [0] = _okTag [0].Insert (1, "#" + CoreHelper.rgbToHex (_okColor));
		_sosoTag [0] = _sosoTag [0].Insert (1, "#" + CoreHelper.rgbToHex (_sosoColor));
		_badTag [0] = _badTag [0].Insert (1, "#" + CoreHelper.rgbToHex (_badColor));
		
		_substitutedTag [0] = _substitutedTag [0].Insert (1, "#" + CoreHelper.rgbToHex (_substitutedColor));
		_deletedTag [0] = _deletedTag [0].Insert (1, "#" + CoreHelper.rgbToHex (_deletedColor));
	}
	
	/// <summary> Inserts tags[0] at 'at' and tags[1] at 'at+length'. 
	/// Adding the return value to _index will set _index at the end of the word+its tags. </summary>
	private int insertTags (int at, int length, string[] tags) 
	{
		string tempTxt = _richText;

        //Debug.Log ("Colouring " + tempTxt + " with " + tags [0] + " " + at + " " + length);
        if (at + length < tempTxt.Length)
        {
            tempTxt = tempTxt.Insert(at + length, tags[1]);
        }
        else
        {
            // If needs to add a tag past the end of the string, need to add instead.
            tempTxt += tags[1];
        }

        if (at < tempTxt.Length)
        {
            tempTxt = tempTxt.Insert(at, tags[0]);
        }
		
		_richText = tempTxt;
		
		return (length + tags[0].Length + tags[1].Length);
	}


	private bool wordIsOneLetterNumber( string word ) {
		switch (word) {
			case "zero": case "oh": case "one": case "two": case "three":
			case "four": case "five": case "six": case "seven": case "eight": case "nine":
				return true;
			default:
				return false;
		}
	}

	private bool isOneLetterNumber (string letter)
	{
		switch (letter)
		{
			case "0": case "1": case "2": case "3": case "4":
			case "5": case "6": case "7": case "8": case "9":
				return true;
			default:
				return false;
		}
	}

    private bool CheckIfTime(string nextWord, string pastString, string toString, ref string output)
    {
        output = nextWord == "past" ? pastString : nextWord == "to" ? toString : nextWord;

        if ((output != nextWord) && goTo(output))
        {
            _stringIsTime = true;
            return true;
        }

        return false; ;
    }

    string[] SingleNumberWords = new string[] { "one", "two", "three", "four", "five", "six", "seven", "eight", "nine" };

    private int GetLengthOfTimeString(string word)
    {
        foreach(string number in SingleNumberWords)
        {
            if(word == number)
            {
                return 1;
            }
        }

        return 2;
    }


    private bool _stringIsTime = false;
    private int  _numberOfWordsInTime = 2;
    private bool _hasGotUnderlinesInString = false;
	/// <summary>
	/// Goes to the next word, parsing the data backwards so that 'zero' & 'oh' turn to '0'. Returns actual word ('0'). 
	/// If special case is not detected, it goes to the targetText (original word).
	/// Compare this to the struct's targetText, if not equal, special word is selected. </summary>
	string goToWord (AnalysisParsedData analysisParsedData, int wordIndex) {
		//Init word holder
		string word;
		word = analysisParsedData.words [wordIndex].targetText;

        if (_stringIsTime)
        {
            //|1-2|   9   | 3 |   8  | = 22
            //  12<#FFA800>.15</color>  
            // <u></u> = 7
            int newLetterIndex = _letterIndex - ((20 + GetLengthOfTimeString(word)) + (_hasGotUnderlinesInString ? 7 : 0));
            _letterIndex = Mathf.Max(newLetterIndex, 0);
            _stringIsTime = false;
            _numberOfWordsInTime = 2;
        }
        _hasGotUnderlinesInString = false;

		// Check for double followed by a number.
		// If found continue this function as if the user had said the next number
		if (word == "double" && wordIndex < analysisParsedData.words.Count - 1) {
			string next_word = analysisParsedData.words [wordIndex + 1].targetText;
			if ( wordIsOneLetterNumber ( next_word ) ) {
				word = next_word;
			}
		}

        string output = "";
		string checkNumber = null;
		string checkTwoLetterNumber = null;
		//Check for special cases, otherwise go to targetText
		switch (word.ToLower ())
		{
			case "zero"  :  { checkNumber = goToNumber ("0", analysisParsedData, wordIndex); if (checkNumber!=null) return checkNumber; break; }
			case "oh"    :  { checkNumber = goToNumber ("0", analysisParsedData, wordIndex); if (checkNumber!=null) return checkNumber; break; }
			case "one"   :  { checkNumber = goToNumber ("1", analysisParsedData, wordIndex); if (checkNumber!=null) return checkNumber; break; }
			case "two"   :  { checkNumber = goToNumber ("2", analysisParsedData, wordIndex); if (checkNumber!=null) return checkNumber; break; }
			case "three" :  { checkNumber = goToNumber ("3", analysisParsedData, wordIndex); if (checkNumber!=null) return checkNumber; break; }
			case "four"  :  { checkNumber = goToNumber ("4", analysisParsedData, wordIndex); if (checkNumber!=null) return checkNumber; break; }
			case "five"  :  {
                                if (wordIndex + 1 < analysisParsedData.words.Count && CheckIfTime(analysisParsedData.words[wordIndex + 1].targetText, ".5", ".55", ref output))
                                {
                                    return output;
                                }
                                else
                                {
                                    checkNumber = goToNumber("5", analysisParsedData, wordIndex); if (checkNumber != null) return checkNumber; break;
                                }
                            }
			case "six"   :  { checkNumber = goToNumber ("6", analysisParsedData, wordIndex); if (checkNumber!=null) return checkNumber; break; }
			case "seven" :  { checkNumber = goToNumber ("7", analysisParsedData, wordIndex); if (checkNumber!=null) return checkNumber; break; }
			case "eight" :  { checkNumber = goToNumber ("8", analysisParsedData, wordIndex); if (checkNumber!=null) return checkNumber; break; }
			case "nine"  :  { checkNumber = goToNumber ("9", analysisParsedData, wordIndex); if (checkNumber!=null) return checkNumber; break; }
			case "ten"   :  {
                                if (wordIndex + 1 < analysisParsedData.words.Count && CheckIfTime(analysisParsedData.words[wordIndex + 1].targetText, ".10", ".50", ref output))
                                {
                                    return output;
                                }
                                else
                                {
                                    checkNumber = goToNumber("10", analysisParsedData, wordIndex); if (checkNumber != null) return checkNumber; break;
                                }
                            }
			case "eleven"   :   { checkNumber = goToNumber ("11", analysisParsedData, wordIndex); if (checkNumber!=null) return checkNumber; break; }
			case "twelve"   :   { checkNumber = goToNumber ("12", analysisParsedData, wordIndex); if (checkNumber!=null) return checkNumber; break; }
			case "thirteen" :   { checkNumber = goToNumber ("13", analysisParsedData, wordIndex); if (checkNumber!=null) return checkNumber; break; }
			case "fourteen" :   { checkNumber = goToNumber ("14", analysisParsedData, wordIndex); if (checkNumber!=null) return checkNumber; break; }
			case "fifteen"  :   { checkNumber = goToNumber ("15", analysisParsedData, wordIndex); if (checkNumber!=null) return checkNumber; break; }
			case "sixteen"  :   { checkNumber = goToNumber ("16", analysisParsedData, wordIndex); if (checkNumber!=null) return checkNumber; break; }
			case "seventeen":   { checkNumber = goToNumber ("17", analysisParsedData, wordIndex); if (checkNumber!=null) return checkNumber; break; }
			case "eighteen" :   { checkNumber = goToNumber ("18", analysisParsedData, wordIndex); if (checkNumber!=null) return checkNumber; break; }
			case "nineteen" :   { checkNumber = goToNumber ("19", analysisParsedData, wordIndex); if (checkNumber!=null) return checkNumber; break; }
			case "twenty"   :   {
					if (wordIndex + 1 < analysisParsedData.words.Count && CheckIfTime(analysisParsedData.words[wordIndex + 1].targetText, ".20", ".40", ref output))
					{
						return output;
					}
					else 
					if (wordIndex + 1 < analysisParsedData.words.Count && analysisParsedData.words[wordIndex + 1].targetText == "five")
					{
						if (CheckIfTime(analysisParsedData.words[wordIndex + 1].targetText, ".25", ".35", ref output))
						{
							_numberOfWordsInTime = 3;
							return output;
						}
					}
					else
					{
						checkNumber = goToNumber("20", analysisParsedData, wordIndex); if (checkNumber != null) return checkNumber; break;
					}
					break;
				}
			case "thirty" :     { checkTwoLetterNumber = goToTwoLetterNumber ("30", analysisParsedData, wordIndex); break; } 
			case "forty" :      { checkTwoLetterNumber = goToTwoLetterNumber ("40", analysisParsedData, wordIndex); break; }
			case "fifty"  :     { checkTwoLetterNumber = goToTwoLetterNumber ("50", analysisParsedData, wordIndex); break; }
			case "sixty"  :     { checkTwoLetterNumber = goToTwoLetterNumber ("60", analysisParsedData, wordIndex); break; }
			case "seventy":     { checkTwoLetterNumber = goToTwoLetterNumber ("70", analysisParsedData, wordIndex); break; }
			case "eighty" :     { checkTwoLetterNumber = goToTwoLetterNumber ("80", analysisParsedData, wordIndex); break; }
			case "ninety" :     { checkTwoLetterNumber = goToTwoLetterNumber ("90", analysisParsedData, wordIndex); break; }
            case "quarter":     {
                                    if (wordIndex + 1 < analysisParsedData.words.Count && CheckIfTime(analysisParsedData.words[wordIndex + 1].targetText, ".15", ".45", ref output))
                                    {
                                        return output;
                                    }
                                    break;
                                }
            case "and": { if (goTo("&")) return "&"; break; }
			// Added these for email addresses.
			case "at" :     {if (goTo ("@")) return "@"; break; }
			case "dot" :    {if (goTo (".")) return "."; break; }
		
			// In the case of the GBP sign, it might go before the sum, so we start the search back from 0. 
			// TODO: WILL NOT WORK PROPERLY if there are multiple signs in the code. It will always stop at the first one.
			case "pounds": { _letterIndex = 0; if (goTo ("£")) return "£"; break; }
			case "pound" : { _letterIndex = 0; if (goTo ("£")) return "£"; break; }
		}
		if (checkTwoLetterNumber != null && checkTwoLetterNumber.Length>0) return checkTwoLetterNumber;

		if (goTo (word)) return word;
		return null;
	}

#region Four Letter Numbers ("one thousand (and)...")

	private bool _isFourLetterNumber = false;  // A four letter number has been found
	private int  _wordsInFourLetterNumber = 2; // Word count for the four letter number. ex: 'one thousand (and)'

	/// <summary> Check if 'number' (1, 2, ... , 9) has 'thousand (and)' after it. If so, find appropriate number in _richText string and return it. Works in formats : '<1000>' or '<100>1' or '<10>11' </summary>
	private string GoToFourLetterNumber (string number, AnalysisParsedData analysisParsedData, int wordIndex)
	{
		// Get following words
		string nextWord		     = (analysisParsedData.words.Count > wordIndex+1) ? analysisParsedData.words [wordIndex+1].targetText : null;
		string nextAfterNextWord = (analysisParsedData.words.Count > wordIndex+2) ? analysisParsedData.words [wordIndex+2].targetText : null;

		// Check for 'thousand'
		if (nextWord == null || nextWord.ToLower () != "thousand")
			return null;

		// Found
		_isFourLetterNumber = true;
		_wordsInFourLetterNumber = (nextAfterNextWord != null && nextAfterNextWord == "and") ? 3 : 2;
		
		// Check for '<1000>'
		if (_wordsInFourLetterNumber == 2 && goTo (number+"000"))
			return number+"000";

		// Check for '<100>1' or '<10>11'
		try {
			if (goTo(number + "00") && isOneLetterNumber(_richText[_letterIndex + 1].ToString()))
				return number + "00";
			if (goTo (number+"0") && isOneLetterNumber(_richText[_letterIndex + 1].ToString()) && isOneLetterNumber(_richText[_letterIndex + 2].ToString()))
				return number+"0";
		}
		catch (System.Exception e) 
        {
            Debug.LogError(e);
        }

		// Not found
		_isFourLetterNumber = false;
		return null;
	}

	/// <summary> When _isFourLetterNumber (see above) is true, this is being called to get the average of 'one thousand (and)' words so that it can be mapped onto '<1000>' or '<100>1' or '<10>11' </summary>
	private PpStatus GetFourLetterNumberStatus (AnalysisParsedData analysisParsedData, ref int wordIndex, ref float checkCount, ref float passCount)
	{
		// First word
		int score = (int)analysisParsedData.words[wordIndex].pinpointStatus; 

		// Second word
		++checkCount;
		++wordIndex;
		score += (int)analysisParsedData.words[wordIndex].pinpointStatus;
		if (analysisParsedData.words [wordIndex].pinpointStatus >= _statusThreshold)
			passCount ++;
		
		if(_wordsInFourLetterNumber == 3)
        {
			// Third word
			++checkCount;
            ++wordIndex;
            score += (int)analysisParsedData.words[wordIndex].pinpointStatus;
			if (analysisParsedData.words [wordIndex].pinpointStatus >= _statusThreshold)
				passCount ++;

            return (PpStatus)(score/3);
        }
        return (PpStatus)(score/2);
	}

#endregion

	private string goToNumber (string number, AnalysisParsedData analysisParsedData, int wordIndex)
	{ 
		// Check for four letter numbers (in the case of 'one thousand (and)...'
		if (number != "0" && isOneLetterNumber (number))
		{
			string checkFourLetterNumber = GoToFourLetterNumber (number, analysisParsedData, wordIndex);
			if (!string.IsNullOrEmpty (checkFourLetterNumber))
				return checkFourLetterNumber;
		}
		
		if (goTo (number + ".00")) return number + ".00"; // Check in the case of '£6.00', as an example
		//if (goTo (number + ".")) return number + "."; 
		if (goTo (number)) return number;
		return null;
	}
	
	// Check for two letter number. Example: "20" will return "20" (or "20.00") if found, "2" if found and a number follows it, null if not found-resetting _letterIndex
	private string goToTwoLetterNumber (string number, AnalysisParsedData analysisParsedData, int wordIndex)
	{	
		string initialCheck = goToNumber (number, analysisParsedData, wordIndex);
		if (initialCheck!= null)
			return initialCheck;
			
		int initialLetterIndex = _letterIndex;
		if (goTo (number[0].ToString ()))
		{
			try { 
				if (_richText [_letterIndex+1] != ' ') 
					return number[0].ToString (); 
			} 
			catch { 
				Debug.LogWarning ("Parsing error"); 
				return null;
			}
		}

		_letterIndex = initialLetterIndex; 
		return null; 
	}	
	
	/// <summary> Brings _index to tagetText. Checking against all letters. </summary>
	private bool goTo (string targetText) {
		
		//Check if target text exists (useful when dealing with numbers (0 -> 'oh'/'zero')
		if (!_richText.Contains (targetText))
			return false;
		//Tag flag
		bool isInsideTag = false;
		
		int initialLetterIndex = _letterIndex;
		
		//Reach word
		bool check = false;
		for (int j=_letterIndex; j<_richText.Length; j++) {
			
			//Jump over tags
			if (_richText [j] == '<') 
				isInsideTag = true;
			if (isInsideTag && _richText [j] == '>') 
				isInsideTag = false;
			if (isInsideTag) {
				_letterIndex += 1;
				continue;
			}
			
			//Check letters
			if (_richText [j] != targetText [0]) 
				_letterIndex += 1;
			else {
				check = true;
				for (int i=0; i<targetText.Length; i++) 
					if (_richText [j+i] != targetText [i])
						check = false;
				if (check) break;
				else _letterIndex += 1;
			}
		}

        if (_letterIndex  > 0 && _richText[_letterIndex - 1] == '\'')
        {
            --_letterIndex;
        }
		// Return to where we started if word not found
		if (!check) _letterIndex = initialLetterIndex;
		return check;
	}

	private int CheckForpunctuation(int startingLength)
    {
        if (_letterIndex < _richText.Length)
        {
            if (_richText[_letterIndex] == '\'')
                ++startingLength;
        }

		if (_letterIndex + startingLength < _richText.Length)
			if (_richText[_letterIndex + startingLength] == '\'')
			    ++startingLength;
		if (_letterIndex + startingLength < _richText.Length)
			if (_richText[_letterIndex + startingLength] == '.')
				++startingLength;

        return startingLength;
    }
	
	private void DebugLogFeedback (CarnegieFeedback carnegieFeedback)
	{
		string output = "* Carnegie Results * \n \n";
		
		output += "Winning Sentence Index: " + carnegieFeedback.winningSentenceIndex.ToString () + "\n";
		output += "Warning Text: " + carnegieFeedback.warningText + "\n";
		output += "Error: " + carnegieFeedback.isError + "\n";
		output += "Internet connection: " + carnegieFeedback.internetConnection + "\n";
		output += "\n" + "--------------------" + "\n" + "\n";
		for (int i=0; i<carnegieFeedback.sentences.Count; i++)
		{
			output += "Rich Text: " + carnegieFeedback.sentences [i].richText + "\n";
			output += "Score: " + carnegieFeedback.sentences [i].score + "\n";
			output += "PpScore: " + carnegieFeedback.sentences [i].pinpointScore + "\n";
			output += "Success: " + carnegieFeedback.sentences [i].success + "\n";
			output += "Correct: " + carnegieFeedback.sentences [i].isCorrect + "\n";
			output += "\n" + "***" + "\n";
		}
		
		if (Debug.isDebugBuild)
		Debug.Log (output);
	}
}






















