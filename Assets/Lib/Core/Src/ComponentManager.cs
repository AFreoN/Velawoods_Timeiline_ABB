using UnityEngine;
using CoreLib;
using System.Collections.Generic;

public class ComponentManager : MonoSingleton<ComponentManager>
{
	private Dictionary<string, GameObject> _components;

	protected override void Init()
	{
		_components = new Dictionary<string, GameObject> ();
	}

    public override void Reset()
    {
        // Loop through each component and delete instance
        foreach(KeyValuePair<string, GameObject> component in _components)
        {
            GameObject.Destroy(component.Value);
        }
        _components.Clear();

        Dispose();
    }

    public bool HasComponent(string component_name)
	{
		return _components.ContainsKey(component_name);
	}

	public GameObject AddComponent(string component_name, GameObject obj)
	{
		if(!_components.ContainsKey(component_name))
		{
			GameObject componentInstance = GameObject.Instantiate (obj);
			_components.Add (component_name, componentInstance);
			componentInstance.SetActive(false);
			return componentInstance;
		}
		else
			Debug.Log("ComponentManager: Component " + component_name + " already exists");
		return null;
	}

	public void RemoveComponent(string component_name)
	{
		if(_components.ContainsKey(component_name))
			_components.Remove(component_name);
		else
			Debug.Log("ComponentManager: Component " + component_name + " does not exist");
	}

	public void ShowComponent(string component_name, object parameters)
	{
		if (_components.ContainsKey (component_name))
		{
			_components [component_name].GetComponent<IComponent> ().Show (parameters);
		} 
		else
		{
			Debug.LogWarning("ComponentManager: Component ("+ component_name +") Did not exist, Show()");
		}
	}

	public void HideComponent(string component_name, object parameters)
	{
		if (_components.ContainsKey (component_name)) 
		{
			_components[component_name].GetComponent<IComponent>().Hide(parameters);
		}
		else
		{
			Debug.LogWarning("ComponentManager: Component ("+ component_name +") Did not exist, Hide()");
		}
	}

	public void PrintComponentCount()
	{
		Debug.Log("ComponentManager: Number of Components - " + _components.Count);
	}

	public void PrintComponents()
	{
		Debug.Log("ComponentManager: Printing Components");

		foreach(string component_name in _components.Keys)
		{
			Debug.Log("ComponentManager: Component - " + component_name);
		}

		Debug.Log("ComponentManager: Finished Printing Components");
	}
}
