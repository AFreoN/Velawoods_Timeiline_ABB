using UnityEngine;
using System.Collections.Generic;
using HighlightingSystem;

public class FlashComponent : MonoBehaviour
{
	private Highlighter outline;

	public void Init(Color colour, bool occluderOff, float flashFrequency = 1.0f)
	{
		outline = gameObject.AddComponent<Highlighter> ();
		outline.FlashingOn (colour, Color.clear, flashFrequency);
		if (occluderOff == true) {
			turnOccluderOff();
		} else {
			turnOccluderOn();
		}

#if CLIENT_BUILD
        CoreEventSystem.Instance.AddListener (MainMenu.Messages.MENU_SHOWING, MenuShow);
		CoreEventSystem.Instance.AddListener (MainMenu.Messages.MENU_HIDING, MenuHide);
#endif
    }

	public void turnOccluderOn()
	{
		outline.OccluderOn ();
		outline.SeeThroughOff();
	}

	public void turnOccluderOff()
	{
		outline.OccluderOff ();
		outline.SeeThroughOn();
	}

	private void MenuShow(object parameters)
	{
		outline.FlashingOff ();
		outline.Off ();
	}
	
	private void MenuHide(object parameters)
	{
		outline.FlashingOn ();
		outline.On ();
	}

	public void Reset()
	{
#if CLIENT_BUILD
        CoreEventSystem.Instance.RemoveListener (MainMenu.Messages.MENU_SHOWING, MenuShow);
		CoreEventSystem.Instance.RemoveListener (MainMenu.Messages.MENU_HIDING, MenuHide);
#endif
        outline.FlashingOff ();
		outline.Off ();
		Destroy (outline);
	}

	public int flashingCount()
	{
		return outline.flashingCount;
	}

	public void flashingCount(int count)
	{
		outline.flashingCount = count;
	}
}

