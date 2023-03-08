using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GenericBubble_BaseData : MonoBehaviour {

	// Carnegie parameters
	protected const float _carnegieFeedbackResponseTime = 0.2f;
	protected const float _bubblesCollapseTime = 0.3f;
	protected float _dropdownsSlideTime = 0.35f;
	protected float _legendFontSize = 36;
	
	// Components
	private GameObject _body;
	public GameObject Body {
		get {
			if (_body == null)
			{
				_body = transform.Find ("Body").gameObject;
			}
			return _body;
		}
	}
	
	private GenericBubble_Buttons _buttons;
	public GenericBubble_Buttons Buttons {
		get {
			if (_buttons == null)
			{
				_buttons = transform.Find ("Buttons").GetComponent<GenericBubble_Buttons> ();
			}
			return _buttons;
		}
	}
	
	private GenericBubble_Dropdowns _dropdowns;
	public GenericBubble_Dropdowns Dropdowns {
		get {
			if (_dropdowns == null || GenericBubble_Dropdowns._hideAllCalled) {
				GenericBubble_Dropdowns._hideAllCalled = false;
				List<DialogueEventData.DialogueText> tempTextData = GetComponent<GenericBubble> ()._textData;
				
				for (int i=tempTextData.Count - 1; i>-1; i--) {
					if (tempTextData [i].isCorrect) {
						_dropdowns = Body.transform.GetChild (i).Find ("Dropdowns").GetComponent<GenericBubble_Dropdowns> ();
						break;
					}
				}
				if (_dropdowns == null) _dropdowns = transform.Find ("Body/Box/Dropdowns").GetComponent<GenericBubble_Dropdowns> ();
			}
			return _dropdowns;
		}
	}
	
	// Audio
	protected string _showAudio = "";//"Audio/Speech_Bubble_On";
	protected string _hideAudio = "Audio/UI_Slide_2";
}



























