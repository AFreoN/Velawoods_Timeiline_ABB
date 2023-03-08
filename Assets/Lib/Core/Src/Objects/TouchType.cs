using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TouchType : MonoBehaviour 
{
	private Text textElement;
	public float gap = 0.33f;
    public AudioClip[] TypingSounds;
    public float variance = 0.3f;


    private float currentGap = 0.33f;
	private bool AnimationStarted = false;
	private string intendedString = "";
	private int currentLength = 1;
	private float currentTimestamp = 0.0f;
    private AudioSource Source;

    public string IntendedString
    {
        get
        {
            return intendedString;
        }
    }

	void Awake () 
	{
		textElement = GetComponent<Text> ();

        if (textElement)
        {
            intendedString = textElement.text;
            textElement.text = "";
            currentGap = gap + (gap * Random.Range(-variance, variance));

            if (TypingSounds.Length > 0 && Source == null)
            {
                Source = gameObject.AddComponent<AudioSource>();
                Source.volume = 1.0f;
            }

        }
        else
        {
            Debug.LogError("Failed to find text element on game object: " + gameObject.name);
        }
	}
	
	void Update () 
	{
		if (AnimationStarted) 
		{
			currentTimestamp += Time.deltaTime;

            if (currentTimestamp > currentGap)
			{
				textElement.text = intendedString.Substring(0, currentLength);

                if (TypingSounds.Length > 0)
                {
                    int selection = Random.Range(0, (TypingSounds.Length - 1));
                    Source.clip = TypingSounds[selection];
                    Source.Play();
                }

				++currentLength;
				currentTimestamp = 0.0f;
                currentGap = gap + (gap * Random.Range(-variance, variance));

				if(currentLength > intendedString.Length)
				{
					AnimationStarted = false;
				}
			}
		}
	}

	public void StartAnimation()
	{
		AnimationStarted = true;
	}

	public void Reset()
	{
		textElement.text = "";
		currentLength = 1;
		currentTimestamp = 0.0f;
		AnimationStarted = false;
        Source.volume = 1.0f;
	}

	void OnDisable() 
	{
		Reset ();
	}

	void OnEnable() 
	{
         Reset();

         if (textElement != null)
         {
            StartAnimation();
         }
         else
         {
             Debug.LogError("Failed to find text element on game object");
         }
	}

    public void Skip()
    {
        if(null != textElement)
        {
            textElement.text = intendedString;
        }
        
        AnimationStarted = false;

        if (null != Source)
        {
            Source.volume = 0.0f;
        }
    }
}
