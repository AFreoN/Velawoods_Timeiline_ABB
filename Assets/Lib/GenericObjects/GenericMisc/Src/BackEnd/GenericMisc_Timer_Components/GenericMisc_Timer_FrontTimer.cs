using UnityEngine;
using UnityEngine.UI;
using System.Collections;

using TMPro;

public class GenericMisc_Timer_FrontTimer : GenericMisc_Timer_ComponentBase {

	public GameObject _text;
	
	
	public override void TimerUpdate (float time, float originalTime)
	{
		base.TimerUpdate (time, originalTime);
		
		_text.GetComponent<TextMeshProUGUI> ().text = ((int) time).ToString ();
		GetComponent<Image> ().fillAmount = time / originalTime;
	}
	
	public override void ColorUpdate (Color color)
	{
		base.ColorUpdate (color);
	}
}
