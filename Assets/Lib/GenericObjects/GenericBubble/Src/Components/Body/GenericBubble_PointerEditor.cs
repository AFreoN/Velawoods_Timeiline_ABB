using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class GenericBubble_PointerEditor : MonoBehaviour {
	
	GenericBubble_Pointer.Alignment _alignment;
	
	// Update is called once per frame
	void Update () {
		
		if (_alignment != GetComponent<GenericBubble_Pointer> ()._alignment)
		{
			_alignment = GetComponent<GenericBubble_Pointer> ()._alignment;
			GetComponent<GenericBubble_Pointer> ().FitToAlignment (_alignment);
		}
	}
}
