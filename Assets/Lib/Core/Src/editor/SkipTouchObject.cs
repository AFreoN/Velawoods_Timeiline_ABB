using UnityEngine;
using System.Collections;
using UnityEditor;
using CoreLib;

public class SkipTouchObject {

	[MenuItem("Window/Route1/Core/SkipTouchObject %T")]
	public static void SkipObject()
	{
		TouchManager.Instance.TouchNextObject ();
	}

}
