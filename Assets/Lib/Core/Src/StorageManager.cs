using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace CoreLib
{
	public class StorageManager : MonoSingleton<StorageManager>
	{
		public Dictionary<string, string> _storedValues;
		public Dictionary<string, Sprite> _storedSprites;

		protected override void Init ()
		{
			base.Init ();

			_storedValues = new Dictionary<string, string> ();
			_storedSprites = new Dictionary<string, Sprite>();
		}

		public void StoreSprite(string name, Sprite value)
		{
			_storedSprites [name] = value;
		}
		
		/// <summary>
		/// Gets the sprite, if the sprite has not yet been
		/// stored the the default value is used
		/// </summary>
		/// <returns>The value.</returns>
		/// <param name="name">Name.</param>
		/// <param name="defaultValue">Default value.</param>
		public Sprite GetSprite(string name, Sprite defaultValue)
		{
			if (_storedSprites.ContainsKey (name)) {
				return _storedSprites [name];
			} 
			else 
			{
				_storedSprites[name] = defaultValue;
				return _storedSprites[name];
			}
		}
		
		public bool SpriteExists(string name)
		{
			if(string.IsNullOrEmpty(name))
			{
				return false;
			}
			return _storedSprites.ContainsKey(name);
		}

		public void SetValue(string name, string value)
		{
			_storedValues [name] = value;
		}

		/// <summary>
		/// Gets the value, if the value has not yet been
		/// stored the the default value is used
		/// </summary>
		/// <returns>The value.</returns>
		/// <param name="name">Name.</param>
		/// <param name="defaultValue">Default value.</param>
		public string GetValue(string name, string defaultValue="")
		{
			if (_storedValues.ContainsKey (name)) {
				return _storedValues [name];
			} 
			else 
			{
				_storedValues[name] = defaultValue;
				return _storedValues[name];
			}
		}

		public bool ValueExists(string name)
		{
			return _storedValues.ContainsKey(name);
		}
	}
}
