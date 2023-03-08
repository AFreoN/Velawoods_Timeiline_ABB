using TMPro;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public enum NonLatinTextTypes
{
    ARABIC = 1,
    JAPANESE,
    NONE
}

public class NonLatinTextFixerMessages
{
    public const string NON_LATIN_TEXT_FIXED = "NonLatinTextFixed";
}

[RequireComponent(typeof(TextMeshProUGUI))]
public class NonLatinTextFixer : MonoBehaviour
{
    public NonLatinTextTypes nonLatinTextType { get; private set; }
    public bool isBoldLatinText = false;
    public bool isBoldFormatted = false;

	private Coroutine _currentCoroutine = null;
	private bool _forceUpdateInProgress;

	// TEXT
	private TextMeshProUGUI _tmProElement;
	private TextMeshProUGUI TextMeshProElement
	{
		get {
			if (_tmProElement == null)
				_tmProElement = GetComponent<TextMeshProUGUI> ();
			return _tmProElement;
        }
	}
	private string _originalString;

	// RECT
    private RectTransform _rectElement;
	private RectTransform RectTransform {
		get {
			if (_rectElement == null)
				_rectElement = GetComponent<RectTransform> ();
			return _rectElement;
		}
	}
    private float _previousWidth;
    private float _previousHeight;
	

    public void Awake()
    {
        _previousWidth = RectTransform.rect.width;
        _previousHeight = RectTransform.rect.height;

        _forceUpdateInProgress = false;
    }

    public void UpdateNonLatinTextType(NonLatinTextTypes newTextType)
    {
        nonLatinTextType = newTextType;
        ForceUpdate();
    }

    public void Update()
    {
        if(TextMeshProElement.havePropertiesChanged)
        {
            _originalString = null;
        }

        if(_previousWidth != RectTransform.rect.width || _previousHeight != RectTransform.rect.height)
        {
            _previousWidth = RectTransform.rect.width;
            _previousHeight = RectTransform.rect.height;

            if(_currentCoroutine != null)
                StopCoroutine(_currentCoroutine);

            switch(nonLatinTextType)
            {
                case NonLatinTextTypes.ARABIC:
                    RestoreBoldness();
                    _currentCoroutine = StartCoroutine(FixArabicText());
                    break;
                case NonLatinTextTypes.JAPANESE:
                    RemoveBoldness();
                    RevertRightAlignment();
                    break;
                default:
                    RestoreBoldness();
                    RevertRightAlignment();
                    break;
            }                
        }
    }

    public void ForceUpdate()
    {
        if (_forceUpdateInProgress)
            return;

        _forceUpdateInProgress = true;
        //TextMeshProElement.IgnoreReverse = false;

        if (_currentCoroutine != null)
            StopCoroutine(_currentCoroutine);

        switch (nonLatinTextType)
        {
            case NonLatinTextTypes.ARABIC:
                RestoreBoldness();
                _originalString = null;
				if (!gameObject.activeInHierarchy) return;
                _currentCoroutine = StartCoroutine(FixArabicText());
                break;
            case NonLatinTextTypes.JAPANESE:
                RemoveBoldness();
                RevertRightAlignment();
                _forceUpdateInProgress = false;
                break;
            default:
                RestoreBoldness();
                RevertRightAlignment();
                _forceUpdateInProgress = false;
                break;
        }            
    }

    private IEnumerator FixArabicText()
    {
        if(false)// (TextMeshProElement.IgnoreReverse)
        {
            //TextMeshProElement.IgnoreReverse = false;
            //_forceUpdateInProgress = false;

            yield return null;
        }
        else
        {
            // Hide text
            //TextMeshProElement.Hide();

            // Store off the original string if it has not already been done.
            if (string.IsNullOrEmpty(_originalString))
            {
                yield return null;
                _originalString = TextMeshProElement.text;
            }
            else
            {
                TextMeshProElement.text = _originalString;
                //TextMeshProElement.IgnoreReverse = true;
                yield return null;
            }

            yield return null;

            // Reverse the line order.
            List<List<string>> textLines = new List<List<string>>();
            MatchCollection matches = Regex.Matches(TextMeshProElement.text, @"\S+|\s+");

            int offset = 0;

            float baselineTolerance = 0.1f;
            float currentBaseLine = TextMeshProElement.textInfo.characterInfo[matches[0].Index].baseLine;
            List<string> currentLine = new List<string>();

            foreach (Match match in matches)
            {
                if (TextMeshProElement.textInfo.characterInfo[match.Index - offset].baseLine >= (currentBaseLine - baselineTolerance) &&
                        TextMeshProElement.textInfo.characterInfo[match.Index - offset].baseLine <= (currentBaseLine + baselineTolerance))
                {
                    currentLine.Add(match.Value);
                }
                else
                {
                    textLines.Add(new List<string>(currentLine));
                    currentLine.Clear();
                    currentLine.Add(match.Value);
                    currentBaseLine = TextMeshProElement.textInfo.characterInfo[match.Index - offset].baseLine;
                }

                foreach (Match tagMatch in Regex.Matches(match.Value, @"\<(.*?)\>"))
                {
                    offset += tagMatch.Value.Length;
                }
            }
            if (currentLine.Count > 0)
            {
                textLines.Add(new List<string>(currentLine));
            }

            string fixedBodyText = "";
            foreach (List<string> line in textLines)
            {
                foreach (string word in line)
                {
                    if (Regex.IsMatch(word, RegexCharSet_ar.charSetRegex))
                    {
                        // Handle formatting tags
                        for (int tagWordIndex = 0; tagWordIndex < line.Count; ++tagWordIndex)
                        {
                            string newWord = "";
                            foreach (Match tagMatch in Regex.Matches(line[tagWordIndex], @"<.*?\>"))
                            {
                                string nextPart = Regex.Replace(tagMatch.Value, @"<(?:/)", "<|");
                                nextPart = Regex.Replace(nextPart, @"<(?![/|])", "</");
                                nextPart = nextPart.Replace("<|", "<");

                                newWord = nextPart + newWord;
                            }
                            if(!string.IsNullOrEmpty(newWord))
                            {
                                line[tagWordIndex] = newWord;
                            }
                        }

                        line.Reverse();
                        break;
                    }
                }

                int firstWordIndex = 0;
                string lastWord = "";
                if (Regex.IsMatch(line[0], @"[\n\r]+"))
                {
                    firstWordIndex = 1;
                    lastWord = line[0];
                }
                else if(line[0] == " ")
                {
                    firstWordIndex = 1;
                    lastWord = "\n";
                }

                // Loop over the line array to see where to start from. (ignore whitespace and null at the beginning)
                for (int wordIndex = firstWordIndex; wordIndex < line.Count; ++wordIndex)
                {
                    if(line[wordIndex].Length > 0)
                    {
                        if(char.IsWhiteSpace(line[wordIndex][0]))
                        {
                            ++firstWordIndex;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        ++firstWordIndex;
                    }
                }

                for (; firstWordIndex < line.Count; ++firstWordIndex)
                {
                    if (Regex.IsMatch(line[firstWordIndex], @"(\w+)([^\w\)]+)") && Regex.IsMatch(line[firstWordIndex], RegexCharSet_Latin.negativeCharSetRegex))
                    {
                        foreach (Match match in Regex.Matches(line[firstWordIndex], @"(\w+)([^\w\)]+)"))
                        {
                            string newWord = "";
                            foreach (Group group in match.Groups)
                            {
                                if (group.Value != match.Value)
                                {
                                    foreach (Match subGroup in Regex.Matches(group.Value, @"\w+|\W"))
                                    {
                                        newWord = subGroup.Value + newWord;
                                    }
                                }
                            }
                            fixedBodyText += newWord;
                        }
                    }
                    else
                    {
                        fixedBodyText += line[firstWordIndex];
                    }
                }
                fixedBodyText += lastWord;
            }

            TextMeshProElement.text = fixedBodyText;
            //TextMeshProElement.IgnoreReverse = true;

            if (TextMeshProElement.alignment == TextAlignmentOptions.TopLeft ||
                TextMeshProElement.alignment == TextAlignmentOptions.Left ||
                TextMeshProElement.alignment == TextAlignmentOptions.BottomLeft ||
                TextMeshProElement.alignment == TextAlignmentOptions.BaselineLeft)
            {
                TextMeshProElement.alignment += 2;
            }

            // Show text
            //TextMeshProElement.Show();

            yield return null;

            _forceUpdateInProgress = false;

            CoreEventSystem.Instance.SendEvent(NonLatinTextFixerMessages.NON_LATIN_TEXT_FIXED, gameObject);
        }
    }

    private void RemoveBoldness()
    {
        // Remove bold tags
        isBoldFormatted = ((TextMeshProElement.fontStyle & FontStyles.Bold) != 0);
        TextMeshProElement.fontStyle &= ~FontStyles.Bold;
        TextMeshProElement.text = Regex.Replace(TextMeshProElement.text, @"<[/]*b>", "");
    }

    private void RestoreBoldness()
    {
        if(isBoldFormatted)
        {
            TextMeshProElement.fontStyle |= FontStyles.Bold;
        }
    }

    private void RevertRightAlignment()
    {
        if (TextMeshProElement.alignment == TextAlignmentOptions.TopRight ||
            TextMeshProElement.alignment == TextAlignmentOptions.Right ||
            TextMeshProElement.alignment == TextAlignmentOptions.BottomRight ||
            TextMeshProElement.alignment == TextAlignmentOptions.BaselineRight)
        {
            TextMeshProElement.alignment -= 2;
        }
    }

    public void OnDestroy()
    {
        RestoreBoldness();
        RevertRightAlignment();
    }
}
