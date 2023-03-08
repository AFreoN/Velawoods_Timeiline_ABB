using UnityEngine;
using System.Collections;
using TMPro;

public class GenericBubble_Text : GenericObject {
	//[Header ("Bubble Text Params")]

	public void SetTextString (object[] paramList)
	{
		string text = (string) paramList [0];
		
		SwitchText (new object[] {text, 0.0f});
	}
	
	public void SwitchText (object[] paramList)
	{
		string newDialogueText = (string) paramList [0];
		float lerpTime = (float) paramList [1];
		
		StartCoroutine ("SwitchTextCoroutine", new object[] {gameObject, newDialogueText, lerpTime});
	}
	
	public override void Hide (object[] paramList)
	{
		StopCoroutine ("SwitchTextCoroutine");
	
		base.Hide (paramList);
	}
	
	
	//-Coroutines-------------------------------------------------------------------------------------------
	
	private IEnumerator SwitchTextCoroutine (object[] paramList)
	{
		GameObject textObj = (GameObject) paramList [0];
		string newDialogueText = (string) paramList [1];
		float lerpTime = (float) paramList [2];
	
		if (lerpTime > 0) // Fade out
		{
			UITween.fadeTo (textObj, 0, lerpTime, UITween.UIFadeType.easeInSine, false);
			yield return new WaitForSeconds (lerpTime / 2.0f + 0.001f);
		}
		
		// Switch
		textObj.GetComponent<TextMeshProUGUI> ().text = newDialogueText;
		
		if (lerpTime > 0) // Fade in
		{
			UITween.fadeTo (textObj, 1, lerpTime, UITween.UIFadeType.easeInSine, false);
			yield return new WaitForSeconds (lerpTime / 2.0f + 0.001f);
		}
	}
}
