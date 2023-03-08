using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using CoreLib;

public class ConvoButtons_test : MonoBehaviour {

	public GameObject _recordButton;

	int index = 0;
	//int index1 = 0;
	
	public void Update ()
	{
		/*
		if (Input.GetKeyDown (KeyCode.R))
		{
			_recordButton.GetComponent<GenericButton_Record> ().Show (new object[] {1.0f});
		}
		

		if (Input.GetKeyDown (KeyCode.X))
		{
			if (GetComponent<Image>() == null)
				gameObject.AddComponent<Image> ();

			Sprite[] courseTestSprites  = Resources.LoadAll<Sprite> ("CourseTests");
			Debug.Log (courseTestSprites.Length);
			foreach (Sprite sprite in courseTestSprites)
			{
				//CoreHelper.LoadStreamingAssetsSprite (CoreHelper.StreamingAssetsSpriteFolder.Minigames, sprite.name, this, done);
				Debug.Log (index1 + " --> " + sprite.name);
			}
		}*/
	}
	
	private void done (Sprite img)
	{
		if (img == null)
			Debug.Log (index + " --> NULL");
		index++;
	}
}
