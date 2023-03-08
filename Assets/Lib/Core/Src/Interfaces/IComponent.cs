using UnityEngine;
using System.Collections;

namespace CoreLib	
{
	public interface IComponent 
	{
		void Show(object parameters);
		void Hide(object parameters);
	}
}
