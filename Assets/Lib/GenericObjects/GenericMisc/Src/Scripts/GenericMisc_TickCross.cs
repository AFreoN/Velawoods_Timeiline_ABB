using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GenericMisc_TickCross : GenericObject {

//-Show and Hide will affect only the active contents of the button. Activate/Deactivate as you wish. 
//-Also, you can use the public methods below to control the tick/cross individually from the whole button.
	
	public void SetTick ()
	{
		transform.Find ("Tick").gameObject.SetActive (true);
		transform.Find ("Cross").gameObject.SetActive (false);
	}	
	
	public void SetCross ()
	{
		transform.Find ("Tick").gameObject.SetActive (false);
		transform.Find ("Cross").gameObject.SetActive (true);
	}
	
//-Content

	public void ShowTick (float lerpTime)
	{
		transform.Find ("Tick").GetComponent<GenericObject> ().Show (new object[] {lerpTime});
		iTween.ValueTo (gameObject, iTween.Hash ("from", GetComponent<Image>().color, "to", Color.white, "time", lerpTime, "onupdate", "FadingBG" ));
	}
	
	public void HideTick (float lerpTime)
	{
		transform.Find ("Tick").GetComponent<GenericObject> ().Hide (new object[] {lerpTime});
		iTween.ValueTo (gameObject, iTween.Hash ("from", GetComponent<Image>().color, "to", new Color(0f,0f,0f,0f), "time", lerpTime, "onupdate", "FadingBG" ));
	}
	
	public void ShowCross (float lerpTime, bool withBackground = false)
	{
		transform.Find ("Cross").GetComponent<GenericObject> ().Show (new object[] {lerpTime});
		if (withBackground)
			iTween.ValueTo (gameObject, iTween.Hash ("from", GetComponent<Image>().color, "to", Color.white, "time", lerpTime, "onupdate", "FadingBG" ));
	}
	
	public void HideCross (float lerpTime)
	{
		transform.Find ("Cross").GetComponent<GenericObject> ().Hide (new object[] {lerpTime});
		if (GetComponent<Image>().color.a > 0f)
			iTween.ValueTo (gameObject, iTween.Hash ("from", GetComponent<Image>().color, "to", new Color(0f,0f,0f,0f), "time", lerpTime, "onupdate", "FadingBG" ));
	}

	void FadingBG (Color c){
		GetComponent<Image> ().color = c;
	}
}
