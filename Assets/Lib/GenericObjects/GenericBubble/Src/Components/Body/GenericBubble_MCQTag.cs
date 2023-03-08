using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class GenericBubble_MCQTag : GenericObject {

	public GameObject _box;
	public GameObject _tag;
	public GameObject _bubbleText;
	
	private static string[] _tags = new string[] {"a", "b", "c", "d", "e", "f", "g", "h"};
	
	private bool IsActive {
		get {
			return (GetComponent<Image> ().enabled);
		}
		set {
			GetComponent<Image> ().enabled = value;
			_tag.GetComponent<TextMeshProUGUI> ().enabled = value;
			
			if (value)
			{
				if (GetComponent<Image> ().color.a != 1)
				{
					Color tempColor = GetComponent<Image>().color;
					tempColor.a = 1; 
					GetComponent<Image> ().color = tempColor;
					
					tempColor = _tag.GetComponent<TextMeshProUGUI> ().color;
					tempColor.a = 1;
					_tag.GetComponent<TextMeshProUGUI> ().color = tempColor;
				}
			
				_bubbleText.GetComponent<RectTransform> ().SetSizeWithCurrentAnchors (RectTransform.Axis.Horizontal, _box.GetComponent<RectTransform> ().rect.width + _bubbleText.GetComponent<RectTransform> ().offsetMax.x*2 - GetComponent<RectTransform> ().rect.width);
			}
			else
			{
				_bubbleText.GetComponent<RectTransform> ().SetSizeWithCurrentAnchors (RectTransform.Axis.Horizontal, _box.GetComponent<RectTransform> ().rect.width + _bubbleText.GetComponent<RectTransform> ().offsetMax.x*2);
			}
		}
	}

//-------------------------------------------------------------------------------------
	
	public override void Show (object[] paramList)
	{
		if (_box.transform.parent.childCount > 1)
		{
			base.Show (paramList);
		}
	}
	
	public override void Hide (object[] paramList)
	{
		if (IsActive)
		{
			base.Hide (paramList);
		}
	}
	
	public void SetMCQTag (object[] param)
	{
		bool active = (bool) param[0];
		int tagIndex = (int) param[1];
		bool upperCase = (param.Length>2) ? (bool) param[2] : false;
		
		if (active)
		{
			string tag = _tags [tagIndex];
			if (upperCase)
				tag = tag.ToUpper();
			_tag.GetComponent<TextMeshProUGUI> ().text = tag;
		}	
		else
		{
			_tag.GetComponent<TextMeshProUGUI> ().text = "";
		}
	
		if (active == IsActive) return;
		IsActive = active;
	}
}
