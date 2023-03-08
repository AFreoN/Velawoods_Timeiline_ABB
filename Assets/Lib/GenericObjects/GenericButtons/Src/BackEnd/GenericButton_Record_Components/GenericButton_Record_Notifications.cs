using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using CoreLib;

public class GenericButton_Record_Notifications : MonoBehaviour {
	
	public struct Warnings {
		public static string tooLoud;
		public static string tooSoft;
		public static string tooNoisy;
		public static string noMicrophone;
		public static string badRecording;
		public static string noInternet;
		public static string noServer;
		public static string internalError;
		public static string encodingError;
		public static string selfAssessment;
	}
	
	public enum Alignment { Left, Right}
	public enum ColorScheme { Warning, Notification } 
	
	[Header ("Color schemes")]
	public Color warningColor = new Color (203.0f/255.0f, 17.0f/255.0f, 34.0f/255.0f);
	public Color notificationColor = new Color (17.0f/255.0f, 135.0f/255.0f, 34.0f/255.0f);
	
	[Header ("Anim controls")]
	public Alignment _alignment;
	public float _slideTime = 0.4f;
	public float _idleTime = 3.0f;
	public float _spacing = 15.0f;
	
	[Header ("Object References")]
	public GameObject _messageTemplate;
	public GameObject _buttonBackground;

	private bool _fontSizeSet = false;
	
	//-Interface-------------------------------------------------------------------------------------------------------------------
	
	void Start ()
	{
#if CLIENT_BUILD
		CoreEventSystem.Instance.AddListener (PlayerProfile.Messages.PLAYER_LANGUAGE_CHANGED, OnLanguageChanged);
#endif
        TextMeshProUGUI textMeshComponent = GetComponentInChildren<TextMeshProUGUI>();
        if(textMeshComponent != null)
        {
            if(textMeshComponent.GetComponent<ContentSizeFitter>() == null)
            {
                ContentSizeFitter contentSizeFitter = textMeshComponent.gameObject.AddComponent<ContentSizeFitter>();
                contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

                textMeshComponent.rectTransform.pivot = new Vector2(0.0f, 0.5f);
                textMeshComponent.rectTransform.offsetMin = new Vector2(15.0f, 0.0f);

				textMeshComponent.overflowMode = TextOverflowModes.Overflow;
            }
        }

        if (_messageTemplate.transform.GetChild(0).GetComponent<Mask>() == null)
        {
            _messageTemplate.transform.GetChild(0).gameObject.AddComponent<Mask>();
        }

        _messageTemplate.SetActive(false);
        SetUpWarnings ();
	}

	void OnDestroy ()
	{
#if CLIENT_BUILD
		CoreEventSystem.Instance.RemoveListener (PlayerProfile.Messages.PLAYER_LANGUAGE_CHANGED, OnLanguageChanged);
#endif
	}

	private void OnLanguageChanged (object param = null)
	{
		_fontSizeSet = false;
		SetUpWarnings ();
	}
	
	/// <summary> Shows the message. </summary> 
	public void ShowMessage (string messageText, ColorScheme messageType = ColorScheme.Warning, float secondsToWait = 0.0f) 
	{
		if (messageText == "" || messageText == null)
			return;
	
		if (_fontSizeSet == false)
		{
			_fontSizeSet = true;

			// Detect appropriate font size
			TextMeshProUGUI _templateText = _messageTemplate.transform.Find("BaseImage/TextMeshPro").GetComponent<TextMeshProUGUI> ();
            if (_templateText != null)
            {
                // Test
                _templateText.enableAutoSizing = true;
                _templateText.text = messageText.Substring(0, 3);
                _templateText.ForceMeshUpdate();
                // Record size
                float fontSize = _templateText.fontSize;
                _templateText.text = "";
                _templateText.enableAutoSizing = false;
                // Assign new size
                _templateText.fontSize = fontSize;
            }
		}
		
		//Hide all and start lerping
		HideMessages ();
		StartCoroutine ("show", new object[] {createMessage (messageText, messageType), secondsToWait});
	}
	
	/// <summary> Hides the messages. </summary>
	public void HideMessages () 
	{
		if (transform == null) return;
		//Hide all apart from the hidden template
		if (transform.childCount > 1)
			for (int i=1; i<transform.childCount; i++)
				if (transform.GetChild (i) != null)
					StartCoroutine ("hide", transform.GetChild (i));
	}

    /// <summary> Hides the messages. </summary>
    public void InstantHideMessages()
    {
        if (transform == null) return;
        //Hide all apart from the hidden template
        if (transform.childCount > 1)
            for (int i = 1; i < transform.childCount; i++)
                if (transform.GetChild(i) != null)
                    Destroy(transform.GetChild(i).gameObject);
    }

    //-Privates--------------------------------------------------------------------------------------------------------------------

    private void FitToAlignment (GameObject messageBox)
	{
		Transform textObj = messageBox.transform.GetChild (0).GetChild (0);
		Vector2 offsetMin = textObj.GetComponent<RectTransform> ().offsetMin;
		Vector2 offsetMax = textObj.GetComponent<RectTransform> ().offsetMax;
		
		switch (_alignment)
		{
		case Alignment.Left:
			messageBox.transform.GetChild (0).GetComponent<RectTransform> ().pivot = new Vector2 (1.0f, 0.5f);
			
			offsetMin.x = _spacing;
			offsetMax.x = -1 * (_buttonBackground.GetComponent<RectTransform> ().rect.width / 2.0f);
			textObj.GetComponent<RectTransform> ().offsetMin = offsetMin;
			textObj.GetComponent<RectTransform> ().offsetMax = offsetMax;
			break;
		default:
			messageBox.transform.GetChild (0).GetComponent<RectTransform> ().pivot = new Vector2 (0.0f, 0.5f);
			
			offsetMax.x = -1 * _spacing;
			offsetMin.x = (_buttonBackground.GetComponent<RectTransform> ().rect.width / 2.0f);
			textObj.GetComponent<RectTransform> ().offsetMin = offsetMin;
			textObj.GetComponent<RectTransform> ().offsetMax = offsetMax;
			break;
		}
	}
	
	/// <summary> Creates the message. </summary>
	private Transform createMessage (string messageText, ColorScheme messageType) 
	{
		//Instantiate, init
		GameObject messageBox = Instantiate (_messageTemplate, _messageTemplate.transform.localPosition, _messageTemplate.transform.localRotation) as GameObject;
		messageBox.transform.SetParent (transform, false);
        messageBox.gameObject.SetActive(true);

        // Block input on this object
        CanvasGroup canvasGroup = messageBox.AddComponent<CanvasGroup> ();
		canvasGroup.blocksRaycasts = false;

		//Set message text
		TextMeshProUGUI textMesh = messageBox.transform.Find ("BaseImage").Find ("TextMeshPro").GetComponent<TextMeshProUGUI> ();
		textMesh.enableWordWrapping = false;
		
		//Set color scheme
		textMesh.color = Color.white;
		switch (messageType) {
		case ColorScheme.Warning:
			messageBox.transform.Find ("BaseImage").GetComponent<Image> ().color = warningColor;
			break;
		case ColorScheme.Notification:
			messageBox.transform.Find ("BaseImage").GetComponent<Image> ().color = notificationColor;
			break;
		}
		
		messageBox.SetActive (false); // Avoid tmpro twitches
		textMesh.text = messageText;
		textMesh.ForceMeshUpdate ();

		return messageBox.transform;
	}
	
	private void SetUpWarnings ()
	{
#if CLIENT_BUILD
		string languageInitials = PlayerProfile.Instance.Language.Initials;
		Warnings.tooLoud 	    = ContentManager.Instance.getString ("LOC_820", languageInitials).ToUpper();
		Warnings.tooSoft        = ContentManager.Instance.getString ("LOC_821", languageInitials).ToUpper();
		Warnings.tooNoisy       = ContentManager.Instance.getString ("LOC_822", languageInitials).ToUpper();
		Warnings.noMicrophone   = ContentManager.Instance.getString ("LOC_823", languageInitials).ToUpper();
		Warnings.badRecording   = ContentManager.Instance.getString ("LOC_824", languageInitials).ToUpper();
		Warnings.noInternet     = ContentManager.Instance.getString ("LOC_825", languageInitials).ToUpper();
		Warnings.noServer       = ContentManager.Instance.getString ("LOC_826", languageInitials).ToUpper();
		Warnings.selfAssessment = ContentManager.Instance.getString ("LOC_905", languageInitials).ToUpper();
		Warnings.encodingError  = "ENCODING ERROR";
		Warnings.internalError  = "INTERNAL ERROR";
#else
		Warnings.tooLoud 	  = "TOO LOUD";
		Warnings.tooSoft      = "TOO SOFT";
		Warnings.tooNoisy     = "BACKGROUND NOISE";
		Warnings.noMicrophone = "NO MICROPHONE DETECTED";
		Warnings.badRecording = "BAD RECORDING";
		Warnings.noInternet   = "NO INTERNET CONNECTION";
		Warnings.noServer     = "NO SERVER CONNECTION";
		Warnings.selfAssessment = "SELF ASSESSMENT";
		Warnings.encodingError  = "ENCODING ERROR";
		Warnings.internalError  = "INTERNAL ERROR";
#endif
	}
	
	//-Coroutines-----------------------------------------------------------------------------------------------------------------------------------------------------
	
	//Lerping to screen
	IEnumerator show (object[] paramList) 
	{
		Transform messageBox = (Transform) paramList [0];
		float waitTime = (float) paramList [1];

        messageBox.gameObject.SetActive(true); // Re-activate message object (de-activated because of tmpro twitches)

        yield return new WaitForSeconds (waitTime);
		
		//Get base and text rectangles
		RectTransform baseRect = messageBox.GetChild (0).GetComponent<RectTransform> ();
        //Get initial values
        float initialWidth = baseRect.rect.width;
        float finalWidth = messageBox.GetChild(0).GetChild(0).GetComponent<RectTransform>().rect.width + _buttonBackground.GetComponent<RectTransform>().rect.width * 0.5f + _spacing * 2.0f;

        FitToAlignment (messageBox.gameObject);

		float currentTime = 0.0f;
        float lerpSize = 0.0f;
        while ((lerpSize != finalWidth) && (messageBox != null)) 
		{
             //Lerp
             currentTime += (Time.deltaTime * (1.0f / _slideTime));
            //Set lerp value
            lerpSize = Mathf.Lerp(initialWidth, finalWidth, currentTime);
            //Lerp base size
            baseRect.SetSizeWithCurrentAnchors (RectTransform.Axis.Horizontal, lerpSize);
			
			yield return null;
		}
		//Wait for user to read the message
		yield return new WaitForSeconds (_idleTime);
		//Hide message
		if (messageBox != null)
			StartCoroutine (hide (messageBox));
	}
	
	//Lerping out of the screen
	IEnumerator hide (Transform messageBox) 
	{
		if (messageBox.name == "Message (hiding)")
			yield break;
		//Change name
		messageBox.name = "Message (hiding)";
		//Get transforms
		Transform messageBase = messageBox.GetChild(0);
		Transform messageText = messageBase.GetChild(0);
		//Lerp
		UITween.fadeTo (messageBase.gameObject, 0, _slideTime, UITween.UIFadeType.easeInSine, false);
		UITween.fadeTo (messageText.gameObject, 0, _slideTime, UITween.UIFadeType.easeInSine, false);
		
		yield return new WaitForSeconds (_slideTime + 0.001f);

		Destroy (messageBox.gameObject);
	}
}




























