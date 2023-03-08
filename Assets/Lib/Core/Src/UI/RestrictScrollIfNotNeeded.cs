using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(ScrollRect), typeof(RectTransform))]
public class RestrictScrollIfNotNeeded : MonoBehaviour 
{
    ScrollRect scrollRect = null;
    RectTransform rectTransform = null;

    // User set scrolling values
    bool userSetScrollHorizontal = false;
    bool userSetScrollVertical = false;

	void Start () 
    {
        scrollRect = GetComponent<ScrollRect>();
        rectTransform = GetComponent<RectTransform>();

        // Grab the user set scrolling states to restrict movement.
        userSetScrollHorizontal = scrollRect.horizontal;
        userSetScrollVertical = scrollRect.vertical;
    }
	
	void Update () 
    {
	    if(null != scrollRect && null != rectTransform)
        {
            scrollRect.horizontal = ((rectTransform.rect.width < scrollRect.content.rect.width) && userSetScrollHorizontal);
            scrollRect.vertical = ((rectTransform.rect.height < scrollRect.content.rect.height) && userSetScrollVertical);

            if(false == scrollRect.horizontal)
            {
                scrollRect.horizontalNormalizedPosition = 0.0f;
            }

            if (false == scrollRect.vertical)
            {
                scrollRect.verticalNormalizedPosition = 0.0f;
            }
        }
	}
}
