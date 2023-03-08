#if CLIENT_BUILD
using System;
using UnityEngine.UI;

public class ContentLocaliserUnityUI : ContentLocaliser
{
	private Text localisedText;

	protected override void Init()
	{
		localisedText = GetComponent<Text>();
	}

	protected override void LocaliseText (string localised)
	{
		localisedText.text = localised;
	}
}
#endif