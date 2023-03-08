using UnityEngine;
using UnityEngine.UI;
using CoreLib;
using System.Collections;

public class GenericMisc_AssistArea : MonoBehaviour
{
    #region Attributes

    // Interface References ----------------------------------------------------

    public GenericMisc_AudioPlayer AudioPlayer;
    public GameObject ReferenceButton;
	
    // Componenets -------------------------------------------------------------

	private RectTransform _rect;
	public RectTransform Rect {
		get {
			if (_rect == null)
				_rect = GetComponent<RectTransform> ();
			return _rect;
		} }

    // Private Variables -------------------------------------------------------

	private GameObject _referenceObject;
    private ReferenceToken _referenceToken;

    #endregion

    #region Functions

    // MonoBehaviour Functions -------------------------------------------------

    public void OnDestroy()
    {
        // Disable the reference object
        if (_referenceObject != null)
        {
            _referenceObject.SetActive(false);
        }

        // Disable the reference tokens and call close
        if (_referenceToken != null)
        {
            _referenceToken.PrefabAnimator.gameObject.SetActive(false);
            _referenceToken.ContentAnimator.gameObject.SetActive(false);
            _referenceToken.Close();
        }

        if(AudioPlayer != null)
        {
            AudioPlayer.Disable();
        }
    }

    // Public Functions --------------------------------------------------------

    public void Setup(int activityID, int referenceTypeID, GameObject referenceObject)
	{
        // Enable the objects based on the reference type
        switch (referenceTypeID)
		{
            case 1: // Audio
                {
                    AudioPlayer.gameObject.SetActive(true);
#if CLIENT_BUILD
                    AudioPlayer._playlist = DialogueAudioHelper.GetAudioListFromSpecificActivity(activityID);
#endif
                    break;
                }
            case 2: // Image
                {
                    ReferenceButton.SetActive(true);
                    if (referenceObject != null)
                    {
                        // If a reference object is passed in
                        if (referenceObject != null)
                        {
                            // Update _referenceObject reference and set active
                            _referenceObject = referenceObject;

                            // Update _referenceToken reference and set active
                            _referenceToken = referenceObject.GetComponent<ReferenceToken>();
                            _referenceToken.PrefabAnimator.gameObject.SetActive(true);
                            _referenceToken.ContentAnimator.gameObject.SetActive(true);
                        }

                        // Add button callback
                        ReferenceButton.GetComponentInChildren<Button>().onClick.AddListener(DisplayImageReference);
                    }
                    break;
                }
        }
    }

    // Private Functions -------------------------------------------------------

    private void DisplayImageReference()
    {
        // If referenceObject is not active, activate it
        if(!_referenceObject.activeSelf)
        {
            _referenceObject.SetActive(true);
        }

        // If a reference object is set
        if (_referenceToken != null)
		{
            // Open the reference token
            _referenceToken.Open();
        }
        else
        {
            // Display error
            Debug.LogError("GenericMisc_AssistArea::DisplayImageReference() - No reference object set.");
        }
    }

    #endregion
}
