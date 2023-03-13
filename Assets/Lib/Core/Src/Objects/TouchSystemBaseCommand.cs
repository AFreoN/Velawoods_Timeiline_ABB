using UnityEngine;
using System.Collections;
using System;

namespace CoreSystem
{
	public abstract class TouchSystemBaseCommand : MonoBehaviour
	{
		public Action<GameObject> ObjectTouched = delegate {};
		public abstract void DoStateChange();
	}
}
