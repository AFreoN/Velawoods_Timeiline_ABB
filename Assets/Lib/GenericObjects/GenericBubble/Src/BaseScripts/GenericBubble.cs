using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using CoreLib;
using TMPro;

public class GenericBubble : GenericBubble_BaseData {
	
	[Header ("Dialogue Text")] [Space (10)]
	public List<DialogueEventData.DialogueText> _textData = new List<DialogueEventData.DialogueText> ();
	
//-Interface-----------------------------------------------------------------------------------------------------
	
	public virtual void Show (float lerpTime = 0.0f)
	{

		object[] paramList = new object[] {lerpTime};
		
		if (gameObject.activeSelf == false)
		{
			gameObject.SetActive (true);
			Hide ();
		}
		
		SendMessageToComponents ("Show", paramList);

		if(_showAudio != "")
		{
			PlayAudio (_showAudio);
		}
		
		_carnegieFails = 0;
	}


	
	public virtual void Hide (bool destroyOnEnd = false, float lerpTime = 0.0f)
	{
		object[] paramList = new object[] {lerpTime, destroyOnEnd};
		
		if (gameObject.activeSelf == false)
		{
			gameObject.SetActive (true);
		}
		
		SendMessageToComponents ("Hide", paramList);
		
		if (destroyOnEnd)
		{
			StartCoroutine (DestroyAfter (lerpTime));
		}

		if (_hideAudio != "")
		{
			PlayAudio (_hideAudio);
		}
	}
	
	public virtual void Slide (Vector2 fromOffset, Vector2 toOffset, float slideTime, bool destroyOnEnd = false)
	{
		if (gameObject.activeSelf == false)
		{
			gameObject.SetActive (true);
		}
		
		StopCoroutines (false);
		StartCoroutine ("SlideRoutine", new object[] {fromOffset, toOffset, slideTime, destroyOnEnd});
	}
	
	public virtual void SlideTo (Vector2 toOffset, float slideTime, bool destroyOnEnd = false)
	{
		Slide (Vector2.zero, toOffset, slideTime, destroyOnEnd);
	}
	
	public virtual void SlideFrom (Vector2 fromOffset, float slideTime, bool destroyOnEnd = false)
	{
		Slide (fromOffset, Vector2.zero, slideTime, destroyOnEnd);
	}
	
	public virtual void SetCharacter (GameObject character = null, int bubbleIndex = 0)
	{
		SendMessageToBubble (bubbleIndex, "SetCharacter", new object[] {character});
	}


	public virtual void AddText (string newDialogueText, string[] carnegieText = null, bool isCorrect = true)
	{
		DialogueEventData.DialogueText newTextData = new DialogueEventData.DialogueText ();
		newTextData.text = newDialogueText;
		newTextData.isCorrect = isCorrect;
		newTextData.carnegieText = carnegieText;
				
		NewText (newTextData);
		RefreshBubbles ();
	}

	
	public virtual void ChangeText (string newDialogueText, int index = 0, string[] carnegieText = null, bool isCorrect = true)
	{
		DialogueEventData.DialogueText newTextData = new DialogueEventData.DialogueText ();
		newTextData.text = newDialogueText;
		newTextData.carnegieOriginalText = newDialogueText;
		newTextData.isCorrect = isCorrect;
		newTextData.carnegieText = carnegieText;
		
		NewText (newTextData, index);
		RefreshBubbles ();
	}

	public virtual void SetTextData (List<DialogueEventData.DialogueText> dialogueTextData)
	{
		// Deep copy
		_textData = new List<DialogueEventData.DialogueText> ();
		for (int i=0; i<dialogueTextData.Count; i++)
		{
			_textData.Add (dialogueTextData [i]);
		}
		RefreshBubbles ();
	}
	
	public virtual void ClearTextData ()
	{
		_textData.Clear ();
		RefreshBubbles ();
	}
	
	public virtual void SwitchBubbleText (DialogueEventData.DialogueText dialogueText, bool replaceTextData = true, float time = 0.0f, int index = 0)
	{
        dialogueText = CoreHelper.ParseDialogueText(dialogueText);

        if (replaceTextData)
        {
            NewText(dialogueText, index);
        }
        SendMessageToBubble(index, "SwitchText", new object[] { dialogueText.text, time });
    }
	
	public virtual void SwitchBubbleText (string dialogueText, bool replaceTextData = false, float time = 0.0f, int index = 0)
	{
        DialogueEventData.DialogueText newDialogueText = _textData[index];
        newDialogueText.text = dialogueText;
        newDialogueText = CoreHelper.ParseDialogueText(newDialogueText);

        if (replaceTextData)
		{
			NewText (newDialogueText, index);
		}
        SendMessageToBubble(index, "SwitchText", new object[] { newDialogueText.text, time, true });
    }

	public virtual void FadePointer (bool fadeIn, float time = 0.1f, int index = 0)
	{
		SendMessageToBubble (index, (fadeIn) ? "ShowPointer" : "HidePointer", new object[] { time });
	}
	
	public virtual void PointToTarget (GameObject newTarget, float time = 0.0f, int index = 0)
	{
		SendMessageToBubble (index, "PointToTarget", new object[] {newTarget, time});
	}
	
	public virtual void FirstAttemptDropdowns ()
	{
		Dropdowns.ShowCarnegieFeedback (_dropdownsSlideTime);
		UpdateFontSizes ();
	}
	
	public virtual void SecondAttemptDropdowns ()
	{
		Dropdowns.ShowCarnegieOffline (_dropdownsSlideTime);
		UpdateFontSizes ();
	}
	
	public virtual void HideAllDropdowns ()
	{
		if (Dropdowns != null) {
			Dropdowns.HideAll (_dropdownsSlideTime);
		}
	}
	
	/// <summary>
	/// Returns the bounds of the bubble in the format Vector4 (Xmin, Xmax, Ymin, Ymax), 
	/// where min is the bottom-left corner and max is the right-top corner; 
	/// Alternatively returns the bounds of the components typed in the argument. </summary>
	public virtual Vector4 GetBounds (string componentName = null, string subComponentName = null)
	{
		Vector4 bounds = new Vector4 (9999, -9999, 9999, -9999);
		
		GameObject[] components = new GameObject[] {Body, Buttons.gameObject};
		foreach (GameObject component in components)
		{
			if (componentName != null && component.name != componentName) continue;

			for (int i=0; i<component.transform.childCount; i++)
			{
				Transform subComponent = component.transform.GetChild (i);
				Vector3 subComponentPos = subComponent.localPosition;

				if (subComponentName != null && subComponent.name != subComponentName) continue;

				for (int j=0; j<subComponent.childCount; j++)
				{
					Transform element = subComponent.GetChild (j);
					
					if (element.gameObject.activeSelf == false || element.GetComponent<GenericObject> () == null) continue;
					
					Vector3 elementPos = element.localPosition;
					
					Rect elementRect = element.GetComponent<RectTransform> ().rect;
					Vector2 elementPivot = element.GetComponent<RectTransform> ().pivot;
					
					float leftBound = elementPos.x + subComponentPos.x - elementRect.width * elementPivot.x;
					if (leftBound < bounds.x) 
						bounds.x = leftBound;

					float rightBound = elementPos.x + subComponentPos.x + elementRect.width * (1.0f - elementPivot.x);
					if (rightBound > bounds.y) 
						bounds.y = rightBound;
						
					float bottomBound = elementPos.y + subComponentPos.y - elementRect.height * elementPivot.y;
					if (bottomBound < bounds.z) 
						bounds.z = bottomBound;
						
					float upperBound = elementPos.y + subComponentPos.y + elementRect.height * (1.0f - elementPivot.y);
					if (upperBound > bounds.w) 
						bounds.w = upperBound;
				}
			}
		}
		
		if (bounds == new Vector4 (9999, -9999, 9999, -9999)) bounds = Vector4.zero;
		
		bounds.x += transform.localPosition.x;
		bounds.y += transform.localPosition.x;
		bounds.z += transform.localPosition.y;
		bounds.w += transform.localPosition.y;
		
		return bounds;
	}
	

//-Record Button Interface-------------------------------------------------------------------------------------------------
	
	public virtual void CarnegieFeedback (object[] paramList)
	{
	}
	
	public virtual void RecordButtonOnComplete ()
	{
		// Carnegie has been successful and record button has finished all its animations
	}
	
	public virtual void CarnegieError (object[] paramList)
	{	
		CarnegieXMLInterpreter.CarnegieFeedback carnegieFeedback = (CarnegieXMLInterpreter.CarnegieFeedback) paramList [0];
		
		Dropdowns.SelfAssessment.SetAudioClip (carnegieFeedback.micClip);
		
		// No internet
		if (carnegieFeedback.internetConnection == false)
		{
			CollapseBubbles (_bubblesCollapseTime);
			SecondAttemptDropdowns ();
		}
		// Other errors.
		else
		{
			CollapseBubbles (_bubblesCollapseTime);
			SecondAttemptDropdowns ();
		}
	}
	
	public virtual void CarnegieSuccess (object[] paramList) 
	{
		CarnegieXMLInterpreter.CarnegieFeedback carnegieFeedback = (CarnegieXMLInterpreter.CarnegieFeedback) paramList [0];
		
		int affectedBubbleIndex = 0;
		for (int i=0; i<_textData.Count; i++) {
			if (_textData [i].isReferenceAnswer == false) {
				if (carnegieFeedback.winningSentenceIndex == affectedBubbleIndex)
				{
					SwitchBubbleText (carnegieFeedback.sentences [carnegieFeedback.winningSentenceIndex].richText, false, _carnegieFeedbackResponseTime, i);
					break;
				}
				affectedBubbleIndex++;
			}
		}
		
	
		CollapseBubbles (_bubblesCollapseTime);
		HideAllDropdowns ();
	}
	
	private float _carnegieFails = 0;
	public virtual void CarnegieFail (object[] paramList) 
	{
		_carnegieFails ++;
	
		Debug.Log ("CARNEGIE FAIL");
	
		CarnegieXMLInterpreter.CarnegieFeedback carnegieFeedback = (CarnegieXMLInterpreter.CarnegieFeedback) paramList [0];
		
		if (carnegieFeedback.sentences == null) return;
		
		int affectedBubbleIndex = 0;
		for (int i=0; i<_textData.Count; i++) {
			if (_textData [i].isReferenceAnswer == false) {
				if (carnegieFeedback.winningSentenceIndex == affectedBubbleIndex)
				{
					SwitchBubbleText (carnegieFeedback.sentences [carnegieFeedback.winningSentenceIndex].richText, false, _carnegieFeedbackResponseTime, i);
                    break;
				}
				affectedBubbleIndex++;
			}
		}
			
		// Set self assessment (self recording)
		Dropdowns.SelfAssessment.SetAudioClip (carnegieFeedback.micClip);
		
		// Get tutor gender
		bool _tutorIsMale = false;
#if CLIENT_BUILD
		_tutorIsMale = (PlayerProfile.Instance.Tutor == PlayerProfile.Gender.Male);
#endif

		// Set tutor audio according to the answer spoken by the user (if any audio clip exists, else the default one remains)
		if (carnegieFeedback.sentences [carnegieFeedback.winningSentenceIndex].isCorrect)
		{
			if (_textData[carnegieFeedback.winningSentenceIndex].tutorAudioClips.female != null || _textData[carnegieFeedback.winningSentenceIndex].tutorAudioClips.male != null) 
					Dropdowns.Help.SetAudioClip((_tutorIsMale) ? _textData[carnegieFeedback.winningSentenceIndex].tutorAudioClips.male : _textData[carnegieFeedback.winningSentenceIndex].tutorAudioClips.female);
		}
		else // Spoken sentence is incorrect, doesn't have tutor audio. Grab the first tutor audio available.
		{
			for (int i=carnegieFeedback.sentences.Count-1; i>-1; i--) {
				if (_textData[i].tutorAudioClips.female != null || _textData[i].tutorAudioClips.male != null) {
					Dropdowns.Help.SetAudioClip((_tutorIsMale) ? _textData[i].tutorAudioClips.male : _textData[i].tutorAudioClips.female);
					break;
				}
			}
		}
		
		// Display legend and help button if it's the first try and the spoken sentence is correct
		if (_carnegieFails == 1 && carnegieFeedback.sentences [carnegieFeedback.winningSentenceIndex].isCorrect)
		{
			StartCoroutine ("WaitForSecondAttemptDropdowns", 3.0f);
			FirstAttemptDropdowns ();
		}
		// Display help/replay/next buttons otherwise
		else
		{	
			StopCoroutine ("WaitForSecondAttemptDropdowns");
			SecondAttemptDropdowns ();
		}	
		
		CollapseBubbles (_bubblesCollapseTime);
	}

	public virtual void CarnegieNext (){}
	
	public virtual void RecordButtonOnSkip ()
	{
		
	}

	public virtual string ParseToCarnegieText( string input ) {

		string output = input;

		Regex emailFormat = new Regex( @"[\w\.\-]+@[\w\-]+\.[\w\.\-]+" );
		output = emailFormat.Replace (output, delegate( Match match ) {
			// Input is an email, replace . and @
			return match.ToString().Replace ("@", " at ").Replace (".", " dot ");
		});

		Regex sterlingFormat = new Regex( @"£([0-9]+)(.([0-9]+))?" );
		output = sterlingFormat.Replace (output, delegate( Match match ) {
			// Input is an email, replace . and @
			if ( match.Groups[2].Value == "" ) {
				return match.Groups[1].Value + " pounds";
			}else {
				return match.Groups[1] + " pounds and " + match.Groups [3].Value.TrimStart("0".ToCharArray()) + " pence";
			}
		});
		
		return output;
		
	}

	
	public virtual void RecordButtonTouched ()
	{
		List<DialogueEventData.DialogueText> textData = new List<DialogueEventData.DialogueText> ();
		for (int i=0; i<_textData.Count; i++)
		{
			if (_textData [i].isReferenceAnswer == false)
				textData.Add (_textData [i]);
		}
		
		for (int i=0; i<textData.Count; i++) {
			if (textData [i].carnegieText == null || textData [i].carnegieText.Length == 0)
			{
				// If no carnegie text set use the bubble text
				DialogueEventData.DialogueText newTextData = textData [i];
				newTextData.carnegieText = new string[] { ParseToCarnegieText( textData [i].text ) };
				textData [i] = newTextData;
			}else{

				// Go through existing carnegie texts and update them to use words rather than symbols
				DialogueEventData.DialogueText newTextData = textData [i];
				for ( int j=0; j<newTextData.carnegieText.Length; j++ ) {
					newTextData.carnegieText[j] = ParseToCarnegieText( newTextData.carnegieText[j] );
				}
				textData [i] = newTextData;
			}
		}	
		
		if (Buttons.RecordButton)
			Buttons.RecordButton._carnegieInput = textData;
	}
	
	public virtual void RecordButtonReleased ()
	{
		for (int i=0; i<Body.transform.childCount; i++) {
			string replaceWith = (_textData [i].carnegieOriginalText != null && _textData [i].carnegieOriginalText.Length>0) ? _textData [i].carnegieOriginalText : _textData [i].text;
			SwitchBubbleText (replaceWith, false, _carnegieFeedbackResponseTime, i);
		}
	}
	
	
//-Replay Button Interface-----------------------------------------------------------------------------------------------
	
	public virtual void ReplayButtonPressed ()
	{
		if (Buttons.ReplayButton)
		{
			Buttons.ReplayButton.Interactable (false, true, 0.2f);	
		}
	}
	
//-Privates&Protected-----------------------------------------------------------------------------------------------------

	protected void SendMessageToComponents (string callMethod, object[] paramList = null)
	{
		Body.BroadcastMessage (callMethod, paramList, SendMessageOptions.DontRequireReceiver);
		Buttons.gameObject.BroadcastMessage (callMethod, paramList, SendMessageOptions.DontRequireReceiver);
	}
	
	public void SendMessageToBubble (int bubbleNo, string callMethod, object[] paramList = null)
	{
		if (Body.transform.childCount <= bubbleNo) {
			Debug.LogError ("GenericBubble: Argument out of range: " + bubbleNo + ". Available bubbles: " + Body.transform.childCount);
			return;
		}
		Body.transform.GetChild (bubbleNo).BroadcastMessage (callMethod, paramList, SendMessageOptions.DontRequireReceiver);
		return;
	}

	protected void StopCoroutines (bool alertComponents = false)
	{
		StopCoroutine ("SlideRoutine");
		if (alertComponents)
			SendMessageToComponents ("StopCoroutines");
	}
	
	protected void RemoveSpeechBubble (int bubbleNo = -1)
	{
		Destroy (Body.transform.GetChild ((bubbleNo < 0) ? Body.transform.childCount - 1 : bubbleNo).gameObject);
	}
	
	protected void AddSpeechBubble (string text = "")
	{
		Vector4 bodyBounds = GetBounds ("Body");
		float bodyHeight = Mathf.Abs (bodyBounds.w - bodyBounds.z);
		
		GameObject box = Body.transform.GetChild (0).gameObject;
		
		box = Instantiate (box) as GameObject;
		box.transform.SetParent (Body.transform, false);
		box.BroadcastMessage ("SetTextString", new object[] {text}, SendMessageOptions.DontRequireReceiver);
		box.name = "Box(New)";
		
		Vector4 boxBounds = GetBounds ("Body", "Box(New)");
		float boxHeight = boxBounds.w - boxBounds.z;
		
		for (int i=0; i<Body.transform.childCount; i++) {
			if (Body.transform.GetChild (i).name == "Box")
			{
				Vector3 pos = Body.transform.GetChild (i).localPosition;
				pos.y += boxHeight / 2.0f;
				Body.transform.GetChild (i).localPosition = pos;
			}
		}
		
		box.name = "Box";
		
		Vector3 boxPos = box.transform.localPosition;
		boxPos.y = -1 * (bodyHeight / 2.0f);
		box.transform.localPosition = boxPos;
	}
	public virtual void NextDropdownPressed (){



	}
	protected void CollapseBubbles (float time)
	{
		StartCoroutine (CollapseBubblesRoutine (time));	
	}
	
	public void RefreshBubbles ()
	{
		if (_textData.Count == 0)
		{
			DialogueEventData.DialogueText newDialogueText = new DialogueEventData.DialogueText();
			newDialogueText.text = "";
			newDialogueText.isCorrect = true;
			_textData.Add (newDialogueText);
		}
		else
		{
			if (_textData.Count > 1)
			{
				bool randomize = true;
				for (int k=0; k<_textData.Count; k++)
					if (_textData [k].isReferenceAnswer)
					{
						randomize = false;
						break;
					}
				if (randomize)
					_textData = CoreHelper.RandomizeList (_textData);
			}
		}
		
		int mcqIndex = 0;
		int i=0;
		for (i=0; i<_textData.Count; i++)
		{
			_textData [i] = CoreHelper.ParseDialogueText (_textData [i]);
		
			if  (Body.transform.childCount > i)
			{
				NewText (_textData [i], i);
				SendMessageToBubble (i, "SwitchText", new object[] {_textData [i].text, 0.0f});
			}
			else
			{
				AddSpeechBubble (_textData [i].text);
			}
			
			if (_textData.Count > 1 && _textData [i].isReferenceAnswer == false)
			{
				SendMessageToBubble (i, "SetMCQTag", new object[] {true, mcqIndex});
				SendMessageToBubble (i, "TogglePointer", new object[] {true});	
				mcqIndex ++;	
			}
			else
			{
				SendMessageToBubble (i, "SetMCQTag", new object[] {false, mcqIndex});
				if (_textData [i].isReferenceAnswer)
					SendMessageToBubble (i, "TogglePointer", new object[] {false});	
			}
		}
		for (int j=i; j<Body.transform.childCount; j++)
		{
			if (Body.transform.GetChild (j))
				RemoveSpeechBubble (j);
		}
		
		// Set model answer if any in _textData
		AudioClip modelAnswer = GetModelAnswer ();
		if (modelAnswer) Dropdowns.Help.SetAudioClip (modelAnswer);
	}
	
	private void NewText (DialogueEventData.DialogueText dialogueText, int index = -1)
	{
		if (index == -1 || _textData.Count == 0)
		{
			_textData.Add (dialogueText);
		}
		else
		{
			_textData [Mathf.Clamp (index, 0, _textData.Count)] = dialogueText;
		}
	}
	
	protected void PlayAudio (string audioPath, float volumeScale = 0.5f, CoreLib.AudioType type = CoreLib.AudioType.SFX)
	{
		AudioManager.Instance.PlayAudio (audioPath, type, volumeScale);
	}
	
	/// <summary> Returns the AudioClip to be initially set as Model Answer. </summary>
	private AudioClip GetModelAnswer ()
	{
		bool tutorIsMale = false;
#if CLIENT_BUILD
		tutorIsMale	= (PlayerProfile.Instance.Tutor == PlayerProfile.Gender.Male);	
#endif
		AudioClip learnerAudio = null;
		// Get last correct answer's model answer
		for (int i=_textData.Count-1; i>=0; i--) {
			if (_textData [i].isCorrect) {
				learnerAudio = (tutorIsMale) ? _textData [i].tutorAudioClips.male : _textData [i].tutorAudioClips.female;
				if (learnerAudio != null)
					break;
			}
		}
		return learnerAudio;
	}


//-Coroutines---------------------------------------------------------------------------------------------------

	protected void UpdateFontSizes() {

		TextMeshProUGUI next = Dropdowns.transform.Find ("Next/Background/Text").GetComponent<TextMeshProUGUI> ();
		TextMeshProUGUI replay = Dropdowns.transform.Find ("Replay/Background/Text").GetComponent<TextMeshProUGUI> ();
		TextMeshProUGUI help = Dropdowns.transform.Find ("Help/Background/Text").GetComponent<TextMeshProUGUI> ();

		bool changeFont = false;

		// Force help, replay and next to all use the same font size
		float fontSize = 64;

		if (Dropdowns.Help.GetState != GenericBubble_DropdownBase.State.Hidden) {
			fontSize = Mathf.Min (fontSize, help.fontSize);
			changeFont = true;
		}

		if (Dropdowns.Next.GetState != GenericBubble_DropdownBase.State.Hidden) {
			fontSize = Mathf.Min (fontSize, next.fontSize);
			changeFont = true;
		}

		if (Dropdowns.SelfAssessment.GetState != GenericBubble_DropdownBase.State.Hidden) {
			fontSize = Mathf.Min (fontSize, replay.fontSize);
			changeFont = true;
		}
				
		next.enableAutoSizing = false;
		replay.enableAutoSizing = false;
		help.enableAutoSizing = false;
		
		if (changeFont)
		{
			replay.fontSize = fontSize;
			help.fontSize = fontSize;
			next.fontSize = fontSize;
		}
		
		// Force the legend fonts to size 36
		
		TextMeshProUGUI legend0 = Dropdowns.transform.Find ("Legend/Background/Key 0/Text").GetComponent<TextMeshProUGUI> ();
		legend0.enableAutoSizing = false;
		legend0.fontSize = _legendFontSize;
		
		TextMeshProUGUI legend1 = Dropdowns.transform.Find ("Legend/Background/Key 1/Text").GetComponent<TextMeshProUGUI> ();
		legend1.enableAutoSizing = false;
		legend1.fontSize = _legendFontSize;
		
		TextMeshProUGUI legend2 = Dropdowns.transform.Find ("Legend/Background/Key 2/Text").GetComponent<TextMeshProUGUI> ();
		legend2.enableAutoSizing = false;
		legend2.fontSize = _legendFontSize;
	}

	protected IEnumerator DestroyAfter (float time)
	{
		yield return new WaitForSeconds (time + 0.001f);
		Destroy (gameObject);
	}
	
	private IEnumerator WaitForSecondAttemptDropdowns (float seconds)
	{
		float time = 0;
		while (time < seconds)
		{
			time += Time.deltaTime;
			yield return null;
		}
		SecondAttemptDropdowns ();
	}
	
	protected IEnumerator SlideRoutine (object[] paramList)
	{
		Vector2 fromOffset = (Vector2) paramList [0];
		Vector2 toOffset = (Vector2) paramList [1];
		float lerpTime = (float) paramList [2];
		bool destroyOnEnd = (bool) paramList [3];
		
		Vector2 startingOffsetMin = GetComponent<RectTransform> ().offsetMin + fromOffset;
		Vector2 startingOffsetMax = GetComponent<RectTransform> ().offsetMax + fromOffset;
		//
		Vector2 finalOffsetMin = GetComponent<RectTransform> ().offsetMin + toOffset;
		Vector2 finalOffsetMax = GetComponent<RectTransform> ().offsetMax + toOffset;
		
		GetComponent<RectTransform> ().offsetMin = startingOffsetMin;
		GetComponent<RectTransform> ().offsetMax = startingOffsetMax;
		
		float currentTime = 0.0f;
		if (lerpTime > 0.0f)
		while (currentTime / lerpTime < 1)
		{
			currentTime += Time.deltaTime;
			float lerpValue = Mathf.Sin ((currentTime / lerpTime) * 0.5f * Mathf.PI);
			
			GetComponent<RectTransform> ().offsetMin = Vector2.Lerp (startingOffsetMin, finalOffsetMin, lerpValue);
			GetComponent<RectTransform> ().offsetMax = Vector2.Lerp (startingOffsetMax, finalOffsetMax, lerpValue);//Vector2.Lerp (fromOffset, toOffset, lerpValue);
			
			yield return null;
		}
		GetComponent<RectTransform> ().offsetMin = finalOffsetMin;
		GetComponent<RectTransform> ().offsetMax = finalOffsetMax;
		
		if (destroyOnEnd)
			Destroy (gameObject);
	}
	
	protected IEnumerator CollapseBubblesRoutine (float lerpTime)
	{
		List<GameObject> correctBubbles = new List<GameObject> ();
		List<float> correctBubblesHeights = new List<float> ();
		
		float totalCorrectBubbleHeight = 0;
					
		for (int i=0; i<Body.transform.childCount; i++)
		{
			Transform box = Body.transform.GetChild (i);
			if (_textData [i].isCorrect || _textData [i].isReferenceAnswer)
			{
				box.name = "Box(correct" + i +")";
				Vector4 boxBounds = GetBounds ("Body", "Box(correct" + i +")");
				
				float height = Mathf.Abs (boxBounds.w - boxBounds.z);
				totalCorrectBubbleHeight += height;
				
				correctBubbles.Add (box.gameObject);
				correctBubblesHeights.Add (height);
			}
			else
			{
				for (int j=0; j<box.childCount; j++) {
					if (box.GetChild (j).GetComponent<GenericObject> ()) {
						box.GetChild (j).GetComponent<GenericObject> ().Hide (new object[] {lerpTime});
					}
				}
			}
		}
		
		float yDestination = -1 * totalCorrectBubbleHeight / 2.0f;
		for (int i=correctBubbles.Count - 1; i>-1; i--)
		{
			yDestination += correctBubblesHeights [i] / 2.0f;
			iTween.MoveTo (correctBubbles [i], iTween.Hash("y", yDestination, "easeType", "easeInSine", "islocal", true, "time", lerpTime));
			yDestination += correctBubblesHeights [i] / 2.0f;
		}
		
		yield return new WaitForSeconds (lerpTime + 0.01f);
		
		for (int i=Body.transform.childCount - 1; i>-1; i--) {
			if (Body.transform.GetChild (i).name == "Box") {
				Destroy (Body.transform.GetChild (i).gameObject);
				_textData.RemoveAt (i);
			}
			else {
				Body.transform.GetChild (i).name = "Box";
			}
		}
	}
}




























