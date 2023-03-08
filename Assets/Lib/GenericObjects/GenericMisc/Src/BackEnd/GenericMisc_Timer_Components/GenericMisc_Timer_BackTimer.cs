using UnityEngine;
using UnityEngine.UI;
using System.Collections;

using TMPro;

public class GenericMisc_Timer_BackTimer : GenericMisc_Timer_ComponentBase {

	public GameObject _text;
	

	public override void TimerUpdate (float time, float originalTime)
	{
		base.TimerUpdate (time, originalTime);
		
		_text.GetComponent<TextMeshProUGUI> ().text = ((int) time).ToString ();
	}
	
	public override void ColorUpdate (Color color)
	{
		base.ColorUpdate (color);
		
		GetComponent<Image> ().color = color;
	}
}
