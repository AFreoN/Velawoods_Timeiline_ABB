using UnityEngine;
using System.Collections;

public class GenericBubble_Buttons : MonoBehaviour {

	// Show warnings?
	private bool _debugLog = false;
	
	private GenericButton_Record _recordButton;
	public GenericButton_Record RecordButton {
		get {
			if (_recordButton == null) {
				GameObject buttonObject = GetButton ("record");
				if (buttonObject)
					_recordButton = buttonObject.GetComponent<GenericButton_Record> ();
			}
			if (_debugLog && _recordButton == null) Debug.LogWarning ("GenericBubble : Record Button Src not found!");
			return _recordButton;
		}
	}
	
	private GenericButton_Replay _replayButton;
	public GenericButton_Replay ReplayButton {
		get {
			if (_replayButton == null) {
				GameObject buttonObject = GetButton ("replay");
				if (buttonObject)
					_replayButton = buttonObject.GetComponent<GenericButton_Replay> ();
			}
			if (_debugLog && _replayButton == null) Debug.LogWarning ("GenericBubble : Replay Button Src not found!");
			return _replayButton;
		}
	}
	
	private GenericButton_Forward _forwardButton;
	public GenericButton_Forward ForwardButton {
		get {
			if (_forwardButton == null) {
				GameObject buttonObject = GetButton ("forward");
				if (buttonObject)
					_forwardButton = buttonObject.GetComponent<GenericButton_Forward> ();
			}
			if (_debugLog && _forwardButton == null) Debug.LogWarning ("GenericBubble : Forward Button Src not found!");
			return _forwardButton;
		}
	}
	
	private GenericButton_Back _backButton;
	public GenericButton_Back BackButton {
		get {
			if (_backButton == null) {
				GameObject buttonObject = GetButton ("back");
				if (buttonObject)
					_backButton = buttonObject.GetComponent<GenericButton_Back> ();
			}
			if (_debugLog && _backButton == null) Debug.LogWarning ("GenericBubble : Back Button Src not found!");
			return _backButton;
		}
	}
	
	private GameObject _rightSideButton;
	public GameObject RightSideButtonObj {
		get {
			if (_rightSideButton == null) {
				Transform container = transform.Find ("RightSide");
				if (container.childCount > 0)
					_rightSideButton = container.GetChild (0).gameObject;
			}
			if (_debugLog && _rightSideButton == null) Debug.LogWarning ("GenericBubble : RightSide Button not found!");
			return _rightSideButton;
		}
	}
	
	private GameObject _leftSideButton;
	public GameObject LeftSideButtonObj {
		get {
			if (_leftSideButton == null) {
				Transform container = transform.Find ("LeftSide");
				if (container.childCount > 0)
					_leftSideButton = container.GetChild (0).gameObject;
			}
			if (_debugLog && _leftSideButton == null) Debug.LogWarning ("GenericBubble : LeftSide Button not found!");
			return _leftSideButton;
		}
	}
	
	
//-Interface--------------------------------------------------------------------------------------------------------------------------------	

	public GameObject GetButton (string partName)
	{
		for (int i=0; i<transform.childCount; i++)
		{
			Transform container = transform.GetChild (i);
			for (int j=0; j<container.childCount; j++)
			{
				if (container.GetChild (j).name.ToLower ().Contains (partName.ToLower ()))
				{
					return container.GetChild (j).gameObject;
				}
			}
		}
		return null;
	}
}












































