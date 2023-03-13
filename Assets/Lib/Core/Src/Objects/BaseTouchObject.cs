using UnityEngine;
using System.Collections;
using System;

namespace CoreSystem
{
	public class BaseTouchObject : MonoBehaviour
	{
		public Action<GameObject> ObjectTouched = delegate {};
	}
}
