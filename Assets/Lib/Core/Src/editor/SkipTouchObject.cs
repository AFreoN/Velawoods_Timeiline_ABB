using UnityEngine;
using System.Collections;
using UnityEditor;
using CoreSystem;

public class SkipTouchObject {

	[MenuItem("Window/Route1/Core/SkipTouchObject %T")]
	public static void SkipObject()
	{
		TouchManager.Instance.TouchNextObject ();
	}

}
