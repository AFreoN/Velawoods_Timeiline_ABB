using UnityEngine;
using System.Collections;

namespace CoreSystem	
{
	public interface IComponent 
	{
		void Show(object parameters);
		void Hide(object parameters);
	}
}
