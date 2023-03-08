using UnityEngine;
using System.Collections;

public class ListBubble : ConversationBubble {
	
	
//-Interface------------------------------------------------------
	
	public void SlideVerticallyBy (float distance, float time = 0.0f)
	{
		SlideTo (new Vector2 (0.0f, distance), time, false);
	}
	
	public void FadeOutButtons (float time = 0.0f)
	{
		FadeButtons (true, time);
	}

	protected override void FadeButtons (bool fadeOut, float lerpTime)
	{
		//base.FadeButtons (fadeOut, lerpTime);
		//Buttons.gameObject.BroadcastMessage ((fadeOut) ? "Hide" : "Show", new object[] {lerpTime});
		
		if (Buttons.LeftSideButtonObj != null && Buttons.LeftSideButtonObj.name.Contains ("CharacterCircle") == false)
			Buttons.LeftSideButtonObj.gameObject.BroadcastMessage ((fadeOut) ? "Hide" : "Show", new object[] {lerpTime});
		if (Buttons.RightSideButtonObj != null && Buttons.RightSideButtonObj.name.Contains ("Record") == false)
			Buttons.RightSideButtonObj.gameObject.BroadcastMessage ((fadeOut) ? "Hide" : "Show", new object[] {lerpTime});
	}
}
