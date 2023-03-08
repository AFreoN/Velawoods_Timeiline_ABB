#if CLIENT_BUILD
using System;
using TMPro;

public class ContentLocaliserTextMeshPro : ContentLocaliser
{
	private TextMeshProUGUI localisedText;
	
	protected override void Init()
	{		
		localisedText = GetComponent<TextMeshProUGUI>();
	}
	
	protected override void LocaliseText (string localised)
	{
		localisedText.text = localised;
	}
}
#endif