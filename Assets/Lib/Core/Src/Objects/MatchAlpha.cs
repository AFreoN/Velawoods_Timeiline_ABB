using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class MatchAlpha : MonoBehaviour 
{
    public Button AlphaToMatch;
    public Image ImageAlphaToMatch;
    private Image TheImageToUpdate;
    private float StartingAlpha = 0.0f;
    private TextMeshProUGUI TextElement = null;

	// Use this for initialization
	void Start () 
    {
        TheImageToUpdate = GetComponent<Image>();

        if(null != TheImageToUpdate)
        {
            StartingAlpha = TheImageToUpdate.color.a;
        }
        else
        {
            TextElement = GetComponent<TextMeshProUGUI>();

            if (null != TextElement)
            {
                StartingAlpha = TextElement.color.a;
            }
        }
	}
	
	// Update is called once per frame
	void Update () 
    {
        if (null != AlphaToMatch && null != TheImageToUpdate)
        {
            Color newColour = TheImageToUpdate.color;

            if (!AlphaToMatch.IsInteractable())
            { 
                newColour.a = AlphaToMatch.colors.disabledColor.a;
            }
            else
            {
                newColour.a = StartingAlpha;
            }

            TheImageToUpdate.color = newColour;
        }
        else if(null != ImageAlphaToMatch && null != TextElement)
        {

            Color newColour = TextElement.color;
            newColour.a = ImageAlphaToMatch.color.a;
            TextElement.color = newColour;
        }
	}
}
