using UnityEngine;
using System.Collections;

[RequireComponent (typeof (BoxCollider2D))]
public class GenericButton_Record_Touch : TouchZone {
	
	private bool wasTouched = false;
	
	void OnEnable ()
	{
		if (GetComponent<BoxCollider2D> ().size.x != GetComponent<RectTransform> ().rect.width) {
			GetComponent<BoxCollider2D> ().size = new Vector3 (GetComponent<RectTransform> ().rect.width, GetComponent<RectTransform> ().rect.height, 1.0f);
		}
	}
	
	protected override void Update () 
	{
		if(!wasTouched) {
			if(IsTouchInZone())
			{
				//First touch
				wasTouched = true;
				GetComponent<GenericButton_Record> ().ButtonTouched ();
			}
		}
		else if(!GetTouchPos())
		{
			//touch released
			wasTouched = false;
			GetComponent<GenericButton_Record> ().ButtonReleased ();
		}
	}
}
