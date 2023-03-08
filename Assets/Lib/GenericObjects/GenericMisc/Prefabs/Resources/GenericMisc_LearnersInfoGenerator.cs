using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif
/// <summary>
/// A very simple script which pulls information from the playerProfile to be used to display dynamic content.
/// Adding this script onto an object will automatically add either image or TextMeshPro component to the object, with player profile information
/// which is selected from the public enum.
/// However it is suggested that you add and configure the component before rather than letting the script add it. You do not have to link
/// the component, unless you are setting the component up on a child object.
/// </summary>

public class GenericMisc_LearnersInfoGenerator : MonoBehaviour {

	public TextMeshProUGUI textComponent;
	public Image	imageComponent;
	public enum Tutor { FirstName, LastName, Country, ProfilePicture };
	public Tutor information = Tutor.FirstName;

	// Use this for initialization
	void Start () {
		if(information != Tutor.ProfilePicture){
			if (textComponent == null) {
				textComponent = this.gameObject.GetComponent<TextMeshProUGUI> ();
				if (textComponent == null) {
					textComponent = this.gameObject.AddComponent<TextMeshProUGUI> ();
				}
			}
		}
		else{
			if (imageComponent == null) {
				imageComponent = this.gameObject.GetComponent<Image> ();
				if (imageComponent == null) {
					imageComponent = this.gameObject.AddComponent<Image> ();
				}
			}
        }

#if CLIENT_BUILD
        if (information == Tutor.FirstName) 
			textComponent.text = PlayerProfile.Instance.FirstName.ToString();
		if (information == Tutor.LastName) 
			textComponent.text = PlayerProfile.Instance.LastName.ToString();
		if (information == Tutor.Country) 
			textComponent.text = PlayerProfile.Instance.Country.ToString();
		if(information == Tutor.ProfilePicture)
			imageComponent.sprite = PlayerProfile.Instance.Avatar.Picture;
#elif UNITY_EDITOR

        if (information == Tutor.FirstName)
        {
            textComponent.text = "First Name";
        }
        if (information == Tutor.LastName)
        {
            textComponent.text = "Last Name";
        }
        if (information == Tutor.Country)
        {
            textComponent.text = "Country";
        }
        if (information == Tutor.ProfilePicture)
        {
            imageComponent.sprite = AssetDatabase.LoadAssetAtPath("Assets/Assets/2D/Phone/Character Circles/SPR_C001_Bob.png", typeof(Sprite)) as Sprite;
        }
#endif


    }
}