using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GenericMisc_ScrollbarVertical : GenericMisc_ScrollbarBase {

	protected override void OnStart()
	{
		base.OnStart();

		_scrollRect.verticalScrollbar = MyScrollbar;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if (IsActive)
		{
			if (_scrollRectTransform.rect.height >= _scrollRect.content.rect.height)
			{
				IsActive = false;
			}
		}
		else
		{
			if (_scrollRectTransform.rect.height < _scrollRect.content.rect.height)
			{
				IsActive = true;
			}
		}
	}
}

