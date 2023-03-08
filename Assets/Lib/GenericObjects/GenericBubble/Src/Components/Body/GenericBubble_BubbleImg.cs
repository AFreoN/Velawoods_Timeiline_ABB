using UnityEngine;
using System.Collections;

using TMPro;

public class GenericBubble_BubbleImg : GenericObject {

	public GameObject _bubbleParent;
	public GameObject _textObject;

	private float _switchTextThreshold = 20;
	
	public void SetTextString (object[] paramList)
	{
		string text = (string) paramList [0];
		
		SwitchText (new object[] {text, 0.0f});
	}

	public void SwitchText (object[] paramList)
	{
		string dialogueText = (string) paramList [0];
		float lerpTime = (float) paramList [1];
		
		bool affectBubblePosition = false;
		if (paramList.Length > 2)
		{
			affectBubblePosition = (bool) paramList [2];
		}
		
		TextMeshProUGUI textComponent = _textObject.GetComponent<TextMeshProUGUI> ();
		textComponent.ForceMeshUpdate ();
		float initialTextHeight = textComponent.bounds.size.y;
		
		string initialText = textComponent.text;
		textComponent.text = dialogueText;
		
		textComponent.ForceMeshUpdate ();
		
		float finalTextHeight = textComponent.bounds.size.y;
		
		textComponent.text = initialText;
		
		float waitTime;
		if (lerpTime > 0)
		{
			waitTime = -1;
		}
		else
		{
			waitTime = (finalTextHeight - initialTextHeight < 0) ? lerpTime/2.0f : 0;
		}
		
		if (Mathf.Abs (finalTextHeight - initialTextHeight) > _switchTextThreshold)
			StartCoroutine (SwitchTextRoutine (lerpTime / 2.0f, waitTime, finalTextHeight - initialTextHeight, affectBubblePosition));
	}
	
	private IEnumerator SwitchTextRoutine (float lerpTime, float waitTime, float heightDifference, bool affectBubblePosition)
	{
		if (waitTime > 0)
			yield return new WaitForSeconds (waitTime);
		
		float initialSize = GetComponent<RectTransform> ().rect.height;
		float finalSize = initialSize + heightDifference;
		
		Vector3 initialPosition = _bubbleParent.transform.localPosition;
		Vector3 finalPosition = initialPosition;
		finalPosition.y += heightDifference / 2.0f;

		if (lerpTime > 0)
		{
			float currentTime = 0;
			while (currentTime / lerpTime < 1)
			{
				currentTime += Time.deltaTime;
				
				float lerpValue = Mathf.Sin ((currentTime / lerpTime) * 0.5f * Mathf.PI);
				float transformValue = (finalSize - initialSize) * lerpValue;
				GetComponent<RectTransform> ().SetSizeWithCurrentAnchors (RectTransform.Axis.Vertical, initialSize + transformValue);

				if (affectBubblePosition)
				{
					_bubbleParent.transform.localPosition = Vector3.Lerp (initialPosition, finalPosition, lerpValue);
				}
				
				yield return null;
			}
		}

		GetComponent<RectTransform> ().SetSizeWithCurrentAnchors (RectTransform.Axis.Vertical, finalSize);
		if (affectBubblePosition)
			_bubbleParent.transform.localPosition = finalPosition;
	}

}
