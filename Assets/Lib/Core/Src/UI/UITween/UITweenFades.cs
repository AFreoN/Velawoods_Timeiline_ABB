using UnityEngine;
using UnityEngine.UI;
using System.Collections;

using TMPro;

public class UITweenFades : MonoBehaviour {

	// Types of supported components
	public enum ComponentType
	{
		Image,
		TMPro,
		Text,
		CanvasGroup
	}
	// Types of variables supported
	public enum VariableType
	{
		Colour,
		Alpha
	}
	
	//-----------------------------------
	//-----------INTERFACE---------------
	
	/// <summary> Start fading </summary>
	public void StartFade (object[] argList)
	{
		// argList : ComponentType, (component), VariableType, (variable), time, FadeType, destroyOnComplete, isFadeTo
		StartCoroutine ("FadeRoutine", argList);
	}
	
	//-----------------------------------
	
	public void OnDestroy ()
	{
		StopAllCoroutines ();
	}

	//----------------------------------------------------------------------------------------------------------------------
	//----------------------------------------------LERPS-------------------------------------------------------------------
	
	private IEnumerator FadeRoutine (object[] argList)
	{
		// Get data
		
		ComponentType 	   componentType	 = (ComponentType) argList [0];
		VariableType  	   var				 = (VariableType) argList [2];
		float 		  	   time 			 = (float) argList [4];
		UITween.UIFadeType fadeType 		 = (UITween.UIFadeType) argList [5];
		bool 			   destroyOnComplete = (bool) argList [6];
		bool 			   isFadeTo 		 = (bool) argList [7];
		
		Image image = null;
		TextMeshProUGUI tmPro = null;
		Text text = null;
		CanvasGroup canvasGroup = null;
		
		switch (componentType)
		{
		case ComponentType.Image: image = (Image) argList [1]; break;
		case ComponentType.Text:  text =  (Text) argList [1]; break;
		case ComponentType.TMPro: tmPro = (TextMeshProUGUI) argList [1]; break;
		case ComponentType.CanvasGroup: canvasGroup = (CanvasGroup) argList [1]; break;
		}
		
		Color colour = Color.black;
		float alpha = 0;
		
		switch (var)
		{
		case VariableType.Colour: colour = (Color) argList [3]; break;
		case VariableType.Alpha:  alpha  = (float) argList [3]; break;
		}
		
		// Set Target colour
		Color targetColor = Color.black;
		switch (var)
		{
		case VariableType.Alpha:  
			switch (componentType)
			{
			case ComponentType.Image: targetColor = image.color; break;
			case ComponentType.Text:  targetColor = text.color;  break;
			case ComponentType.TMPro: targetColor = tmPro.color; break;
			case ComponentType.CanvasGroup: targetColor.a = canvasGroup.alpha; break;
			}
			targetColor.a = alpha;
			break;
		case VariableType.Colour: 
			targetColor = colour; 
			break;
		}
		
		// Assign target colour and break if time is null
		if (time <= 0)
		{
			if (!isFadeTo) yield break;
			
			switch (componentType)
			{
			case ComponentType.Image: image.color = targetColor; break;
			case ComponentType.Text:  text.color  = targetColor; break;
			case ComponentType.TMPro: tmPro.color = targetColor; break;
			case ComponentType.CanvasGroup: canvasGroup.alpha = targetColor.a; break;
			}
			yield break;
		}
		
		// If fadeFrom, targetColor=componentInitialColor & componentColor=targetColor
		if (!isFadeTo)
		{
			Color tempColor = Color.black;
			switch (componentType)
			{
			case ComponentType.Image: tempColor = image.color; image.color = targetColor; break;
			case ComponentType.Text:  tempColor = text.color;  text.color  = targetColor; break;
			case ComponentType.TMPro: tempColor = tmPro.color; tmPro.color = targetColor; break;
			case ComponentType.CanvasGroup: tempColor.a = canvasGroup.alpha; canvasGroup.alpha = targetColor.a; break;
			}
			targetColor = tempColor;
		}
		
		// Set starting color for lerping
		Color initialColor = Color.black;
		switch (componentType)
		{
		case ComponentType.Image: initialColor = image.color; break;
		case ComponentType.Text:  initialColor = text.color;  break;
		case ComponentType.TMPro: initialColor = tmPro.color; break;
		case ComponentType.CanvasGroup: initialColor.a = canvasGroup.alpha; break;
		}
		
		// Start lerping in between initial color and target color
		Color lerpColor = Color.black;
		float currentTime = 0;
		while (currentTime / time < 1 && gameObject.activeSelf)
		{
			currentTime += Time.deltaTime;
			
			lerpColor = Color.Lerp (initialColor, targetColor, getLerpValue (fadeType, currentTime, time));
			switch (componentType)
			{
			case ComponentType.Image: image.color = lerpColor; break;
			case ComponentType.Text:  text.color  = lerpColor; break;
			case ComponentType.TMPro: tmPro.color = lerpColor; break;
			case ComponentType.CanvasGroup: canvasGroup.alpha = lerpColor.a; break;
			}
			yield return null;
		}
		
		// Check if target color has been reached
		if (lerpColor != targetColor && gameObject.activeSelf)
		{
			switch (componentType)
			{
			case ComponentType.Image: image.color = targetColor; break;
			case ComponentType.Text:  text.color  = targetColor; break;
			case ComponentType.TMPro: tmPro.color = targetColor; break;
			case ComponentType.CanvasGroup: canvasGroup.alpha = targetColor.a; break;
			}
			yield return null;
		}
		
		// Destroy appropriately
		if (destroyOnComplete && gameObject.activeSelf)
		{
			Destroy (gameObject);
		}
		else
		{
			Destroy (GetComponent<UITweenFades> ());
		}
	}

	// Get [0,1] lerping value
	private float getLerpValue (UITween.UIFadeType fadeType, float currentTime, float totalTime)
	{
		float value = currentTime / totalTime;
		
		switch (fadeType)
		{
		case UITween.UIFadeType.easeInSine: return (Mathf.Sin ((value) * Mathf.PI * 0.5f));
		case UITween.UIFadeType.easeInCos:  return (1.0f - Mathf.Cos ((value) * Mathf.PI * 0.5f));
			
		default: 
			return (value);
		}
	}
}
