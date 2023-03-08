using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using HighlightingSystem;

namespace CoreLib
{
	public class FlashColourNoTouch : MonoBehaviour 
	{
		private Highlighter outline;
		
		void Awake ()
		{
			outline = gameObject.AddComponent<Highlighter> ();
			outline.FlashingOn (new Color(0.762f, 0, 0.082f), Color.clear, 1f);
		}
		
		public void Reset()
		{
			outline.FlashingOff ();
			outline.Off ();
			Destroy (outline);
			Destroy (this);
		}
	}
}