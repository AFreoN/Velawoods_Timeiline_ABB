using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;


public class ResizeButtonToText : MonoBehaviour 
{
    public Button ButtonToRisize;
    public float Padding = 60.0f;

    private TextMeshProUGUI m_TMElement = null;
    private RectTransform m_RectTransform = null;

    void Start()
    {
#if DEBUG_BUILD
        if(null == ButtonToRisize)
        {
            Debug.LogError("ResizeButtonToText::Start: Failed to find button to resize.");
        }
#endif
        m_RectTransform = (RectTransform)ButtonToRisize.transform;
        m_TMElement = GetComponent<TextMeshProUGUI>();
#if DEBUG_BUILD
        if (null == m_TMElement)
        {
            Debug.LogError("ResizeButtonToText::Start: Failed to find Text mesh pro element.");
        }

        if (null == m_RectTransform)
        {
            Debug.LogError("ResizeButtonToText::Start: Failed to find buttom rect transform.");
        }
#endif
    }

	void Update () 
    {
        m_RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, m_TMElement.bounds.size.x + (Padding * 2));
	}
}
