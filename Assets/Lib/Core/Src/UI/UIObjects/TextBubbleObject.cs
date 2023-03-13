using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace CoreSystem
{
	public class TextBubbleObject : MonoBehaviour {

		public Image _triangle;
		public Image _circle;
		public Image _ring;

		private Color _currentColor = Color.white;

		public void ChangeColor(Color new_color, bool changeRing=false)
		{
			_triangle.color = new_color;
			_currentColor = new_color;

			if(changeRing) _ring.color = new_color;
		}

		public Color CurrentColor
		{
			get {return _currentColor;}
		}
	}
}
