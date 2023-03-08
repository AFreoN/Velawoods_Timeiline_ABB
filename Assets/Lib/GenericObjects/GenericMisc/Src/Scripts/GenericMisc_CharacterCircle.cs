using UnityEngine;
using System.Collections;
using UnityEngine.UI;


public class GenericMisc_CharacterCircle : GenericObject {

	/// <summary> Looks up the character's sprite by name at paths UI/CharacterCircles/ and UI/Avatars/ and sets it up if found. </summary>
	public string CharacterImageName {
		set {
			Sprite characterSprite = Resources.Load<Sprite> ("UI/CharacterCircles/" + value.Trim ());
			if (characterSprite == null)
				characterSprite = Resources.Load<Sprite> (value.Trim ());
			if (characterSprite == null)
				characterSprite = Resources.Load<Sprite> ("UI/Avatars/" + value.Trim ());
			
			if (characterSprite == null) 
			{
				Debug.LogWarning ("GenericButton_CharacterCircle : Character Sprite not found at paths UI/CharacterCircles/ and UI/Avatars/");	
				return;
			}
			
			CharacterImage.sprite = characterSprite;
		}
	}
	
	private Image _characterImage;
	/// <summary> Returns this button's character Image components </summary>
	public Image CharacterImage {
		get { 
			if (_characterImage == null)
				_characterImage = transform.Find ("Sprite").GetComponent<Image> ();
			return _characterImage; 
		}
	}
	
	private Image _ringImage;
	/// <summary> Gets and sets this button's ring color component </summary>
	public Color RingColor {
		get {
			if (_ringImage == null)
				_ringImage = transform.Find ("Sprite").Find ("Ring").GetComponent<Image> ();
			return _ringImage.color; 
		}
		set {
			if (_ringImage == null)
				_ringImage = transform.Find ("Sprite").Find ("Ring").GetComponent<Image> ();
			_ringImage.color = value;
		}
	}
	
	/// <summary> Set learner's avatar image and ring </summary>
	public void SetAsLearner ()
	{
#if CLIENT_BUILD

		CharacterImage.sprite = PlayerProfile.Instance.Avatar.Picture;
		RingColor = PlayerProfile.Instance.Avatar.RingColor;
	
#endif
	}
}
