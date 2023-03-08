using UnityEngine;
using System.Collections;
using System;

namespace CoreLib
{
	public class BaseTouchObject : MonoBehaviour
	{
		public Action<GameObject> ObjectTouched = delegate {};
	}
}
