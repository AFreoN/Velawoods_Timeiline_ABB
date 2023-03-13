using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using CoreSystem;

/// <summary> Grabbing text from the content.xml file. Set this object in the editor to hold the content tag. ex: LOC_101 / See the other tutorial prefabs </summary>
public class Tutorial_Static_InstructionsText : MonoBehaviour {

	private TextMeshProUGUI _text;
	private TextMeshProUGUI Text {
		get {
			if (_text == null)
			{
				_text = GetComponent<TextMeshProUGUI> ();
				if (_text == null)
					Debug.LogError ("Tutorial : Script requires a TextMeshProUGUI component!");
			}
			return _text;
		}
	}	


	// Use this for initialization
	void Start () 
	{
		string contentTag = Text.text.Trim ();
		
#if CLIENT_BUILD
		Text.text = ContentManager.Instance.getString (contentTag, PlayerProfile.Instance.Language.Initials);
#else	
		Text.text = ContentManager.Instance.getString (contentTag, "EN");
#endif
	
		Text.text = Text.text.ToUpper ();
		Text.text = Text.text.Replace (@"\N", @"\n");
		Text.text = Text.text.Replace ("<COLOR", "<color").Replace ("</COLOR>", "</color>");
	}
}
