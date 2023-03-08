using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GenericMisc_ScrollbarHorizontal : GenericMisc_ScrollbarBase {

	protected override void OnStart()
	{
		base.OnStart();

		_scrollRect.horizontalScrollbar = MyScrollbar;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if (IsActive)
		{
			if (_scrollRectTransform.rect.width >= _scrollRect.content.rect.width)
			{
				IsActive = false;
			}
		}
		else
		{
			if (_scrollRectTransform.rect.width < _scrollRect.content.rect.width)
			{
				IsActive = true;
			}
		}
	}
}
