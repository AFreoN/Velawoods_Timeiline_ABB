using UnityEngine;
using UnityEngine.UI;
using System.Collections;

using TMPro;

public class UITween : MonoBehaviour {
	
	public enum UIFadeType 
	{
		easeInSine,
		easeInCos,
		Linear
	}
	
	public static bool _debugLog = false;
	
	//--
	
	/// <summary>
	/// Lerps the alpha of the Image/TextMeshProUGUI component over time to set target.	/// </summary>
	/// <param name="target">gameObject to be faded. Image component required.</param>
	/// <param name="toAlpha">Target alpha.</param>
	/// <param name="time">Time length of fade.</param>
	/// <param name="fadeType">Ease type.</param>
	public static void fadeTo (GameObject target, float toAlpha, float time, UIFadeType fadeType, bool destroyOnComplete)
	{
		if (!Check (target)) return;
			
		if (target.GetComponent<Image> ()) 			 target.GetComponent <UITweenFades> ().StartFade (new object[] { UITweenFades.ComponentType.Image, target.GetComponent<Image> (), UITweenFades.VariableType.Alpha, toAlpha, time, fadeType, destroyOnComplete, true });
		else
		if (target.GetComponent<TextMeshProUGUI> ()) target.GetComponent <UITweenFades> ().StartFade (new object[] { UITweenFades.ComponentType.TMPro, target.GetComponent<TextMeshProUGUI> (), UITweenFades.VariableType.Alpha, toAlpha, time, fadeType, destroyOnComplete, true });
		else
		if (target.GetComponent<Text> ()) 			 target.GetComponent <UITweenFades> ().StartFade (new object[] { UITweenFades.ComponentType.Text, target.GetComponent<Text> (), UITweenFades.VariableType.Alpha, toAlpha, time, fadeType, destroyOnComplete, true });
	}
	
	/// <summary>
	/// Lerps the color of the Image/TextMeshProUGUI component over time to set target.	/// </summary>
	/// <param name="target">gameObject to be faded. Image component required.</param>
	/// <param name="toAlpha">Target alpha.</param>
	/// <param name="time">Time length of fade.</param>
	/// <param name="fadeType">Ease type.</param>
	public static void fadeTo (GameObject target, Color toColor, float time, UIFadeType fadeType, bool destroyOnComplete)
	{
		if (!Check (target)) return;
		
		if (target.GetComponent<Image> ()) 			 target.GetComponent <UITweenFades> ().StartFade (new object[] { UITweenFades.ComponentType.Image, target.GetComponent<Image> (), UITweenFades.VariableType.Colour, toColor, time, fadeType, destroyOnComplete, true });
		else
		if (target.GetComponent<TextMeshProUGUI> ()) target.GetComponent <UITweenFades> ().StartFade (new object[] { UITweenFades.ComponentType.TMPro, target.GetComponent<TextMeshProUGUI> (), UITweenFades.VariableType.Colour, toColor, time, fadeType, destroyOnComplete, true });
		else
		if (target.GetComponent<Text> ())			 target.GetComponent <UITweenFades> ().StartFade (new object[] { UITweenFades.ComponentType.Text, target.GetComponent<Text> (), UITweenFades.VariableType.Colour, toColor, time, fadeType, destroyOnComplete, true });
	}

	/// <summary>
	/// Changes the alpha of the Image/TextMeshProUGUI component to fromAlpha and lerps it over time to original.	/// </summary>
	/// <param name="target">gameObject to be faded. Image component required.</param>
	/// <param name="fromAlpha">Alpha to start from.</param>
	/// <param name="time">Time length of fade.</param>
	/// <param name="fadeType">Ease type.</param>
	public static void fadeFrom (GameObject target, float fromAlpha, float time, UIFadeType fadeType, bool destroyOnComplete)
	{
		if (!Check (target)) return;
		
		if (target.GetComponent<Image> ())  		 target.GetComponent <UITweenFades> ().StartFade (new object[] { UITweenFades.ComponentType.Image, target.GetComponent<Image> (), UITweenFades.VariableType.Alpha, fromAlpha, time, fadeType, destroyOnComplete, false });
		else
		if (target.GetComponent<TextMeshProUGUI> ()) target.GetComponent <UITweenFades> ().StartFade (new object[] { UITweenFades.ComponentType.TMPro, target.GetComponent<TextMeshProUGUI> (), UITweenFades.VariableType.Alpha, fromAlpha, time, fadeType, destroyOnComplete, false });
		else
		if (target.GetComponent<Text> ())  			 target.GetComponent <UITweenFades> ().StartFade (new object[] { UITweenFades.ComponentType.Text, target.GetComponent<Text> (), UITweenFades.VariableType.Alpha, fromAlpha, time, fadeType, destroyOnComplete, false });
	}
	
	/// <summary>
	/// Changes the color of the Image/TextMeshProUGUI component to fromAlpha and lerps it over time to original.	/// </summary>
	/// <param name="target">gameObject to be faded. Image component required.</param>
	/// <param name="fromAlpha">Alpha to start from.</param>
	/// <param name="time">Time length of fade.</param>
	/// <param name="fadeType">Ease type.</param>
	public static void fadeFrom (GameObject target, Color fromColor, float time, UIFadeType fadeType, bool destroyOnComplete)
	{
		if (!Check (target)) return;
		
		if (target.GetComponent<Image> ()) 			 target.GetComponent <UITweenFades> ().StartFade (new object[] { UITweenFades.ComponentType.Image, target.GetComponent<Image> (), UITweenFades.VariableType.Colour, fromColor, time, fadeType, destroyOnComplete, false });
		else
		if (target.GetComponent<TextMeshProUGUI> ()) target.GetComponent <UITweenFades> ().StartFade (new object[] { UITweenFades.ComponentType.TMPro, target.GetComponent<TextMeshProUGUI> (), UITweenFades.VariableType.Colour, fromColor, time, fadeType, destroyOnComplete, false });
		else
		if (target.GetComponent<Text> ())  		     target.GetComponent <UITweenFades> ().StartFade (new object[] { UITweenFades.ComponentType.Text, target.GetComponent<Text> (), UITweenFades.VariableType.Colour, fromColor, time, fadeType, destroyOnComplete, false });
	}
	
	/// <summary>
	/// Changes the alpha of the CanvasGroup component to toAlpha over time.	/// </summary>
	/// <param name="target">Target.</param>
	/// <param name="toAlpha">To alpha.</param>
	/// <param name="time">Time.</param>
	/// <param name="fadeType">Fade type.</param>
	/// <param name="destroyOnComplete">If set to <c>true</c> destroy on complete.</param>
	public static void fadeCanvasGroupTo (GameObject target, float toAlpha, float time, UIFadeType fadeType, bool destroyOnComplete)
	{
		if (!Check (target, true)) return;
		
		target.GetComponent <UITweenFades> ().StartFade (new object[] { UITweenFades.ComponentType.CanvasGroup, target.GetComponent<CanvasGroup> (), UITweenFades.VariableType.Alpha, toAlpha, time, fadeType, destroyOnComplete, true });
	}
	
	/// <summary>
	/// Changes the alpha of the CanvasGroup component to fromAlpha and lerps it over time to original.	/// </summary>
	/// <param name="target">Target.</param>
	/// <param name="toAlpha">To alpha.</param>
	/// <param name="time">Time.</param>
	/// <param name="fadeType">Fade type.</param>
	/// <param name="destroyOnComplete">If set to <c>true</c> destroy on complete.</param>
	public static void fadeCanvasGroupFrom (GameObject target, float fromAlpha, float time, UIFadeType fadeType, bool destroyOnComplete)
	{
		if (!Check (target, true)) return;
		
		target.GetComponent <UITweenFades> ().StartFade (new object[] { UITweenFades.ComponentType.CanvasGroup, target.GetComponent<CanvasGroup> (), UITweenFades.VariableType.Alpha, fromAlpha, time, fadeType, destroyOnComplete, false });
	}
	
	//--
	
	private static bool Check (GameObject target, bool isCanvasGroupMethod = false)
	{
		if (!isCanvasGroupMethod)
		{
			if (!target.GetComponent<Image> () && !target.GetComponent<TextMeshProUGUI> () && !target.GetComponent<Text> ())
			{
				if (Debug.isDebugBuild && _debugLog)
						Debug.LogWarning ("UITween error: Game object <" + target.name + "> to be faded requires an Image, Text or a TextMeshProUGUI component!");
				return false;
			}
		}
		else
		{
			if (!target.GetComponent<CanvasGroup> ())
			{
				if (Debug.isDebugBuild && _debugLog)
					Debug.LogWarning ("UITween error: Game object <" + target.name + "> to be faded requires a CanvasGroup component!");
				return false;
			}
		}
		
		if (target.activeInHierarchy == false) 
			return false;
		
		if (target.GetComponent<UITweenFades> ())
			DestroyImmediate (target.GetComponent<UITweenFades> ());
		target.AddComponent <UITweenFades> ();
		
		return true;
	}
}














