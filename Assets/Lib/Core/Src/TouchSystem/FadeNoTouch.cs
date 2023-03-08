using UnityEngine;
using System;
using System.Collections;
using CoreLib;

public class FadeNoTouch : TouchSystemBaseCommand {

	private CustomFadeComponent fadeComponent;
	

	public void FadeObjectIn(float time, float delay = 0.0f)
	{
		if(gameObject == null)
		{
			return;
		}

		gameObject.SetActive(true);

		fadeComponent = gameObject.GetComponent<CustomFadeComponent>();
		
		if(fadeComponent != null)
		{
			DestroyImmediate(fadeComponent);
		}
		
		fadeComponent = gameObject.AddComponent<CustomFadeComponent> ();

		if (fadeComponent.isActiveAndEnabled) 
		{
			fadeComponent.SetFadeValue(0.0f);

			fadeComponent.FadeObject (0.0f, 1.0f, time, delay, false);
		} 
	}

	public void FadeObjectOut(float time, float delay = 0.0f)
	{
		if(gameObject == null)
		{
			return;
		}
		
		gameObject.SetActive(true);
		

		fadeComponent = gameObject.GetComponent<CustomFadeComponent>();
		
		if(fadeComponent != null)
		{
			DestroyImmediate(fadeComponent);
		}

		fadeComponent = gameObject.AddComponent<CustomFadeComponent> ();

		if (fadeComponent.isActiveAndEnabled) 
		{
			fadeComponent.SetFadeValue(1.0f);

			fadeComponent.FadeObject (1.0f, 0.0f, time, delay, true);
		} 
	}

	public void FadeObject(float from, float to, float time, float delay, bool disableOnEnd=false)
	{
		if(fadeComponent == null)
		{
			fadeComponent = gameObject.GetComponent<CustomFadeComponent>();

			if(fadeComponent == null)
			{
				fadeComponent = gameObject.AddComponent<CustomFadeComponent> ();
			}
		}

		if (fadeComponent != null && fadeComponent.isActiveAndEnabled) {
			fadeComponent.FadeObject (from, to, time, delay, disableOnEnd);
		} else {
			Debug.LogWarning ("Object is inactive, check object is active in scene");
		}
	}

	void OnDestroy()
	{
		if(fadeComponent != null)
		{
			CoreHelper.SafeDestroy(fadeComponent);
		}
		else
		{
			fadeComponent = gameObject.GetComponent<CustomFadeComponent>();
			
			if(fadeComponent != null)
			{
				CoreHelper.SafeDestroy(fadeComponent);
			}
		}
	}

	public override void DoStateChange ()
	{

	}

	private void FadeComplete()
	{
		Destroy (this);
	}
}
