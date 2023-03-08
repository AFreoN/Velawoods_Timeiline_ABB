using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MiniGameCreator : MonoBehaviour 
{
	private Dictionary<string, GameObject> _loadedObjects;
    private GameObject _blankWidget;

	public void Init()
	{
		_loadedObjects = new Dictionary<string, GameObject> ();
        _blankWidget = Resources.Load<GameObject>("Minigames/MG_Blank_Widget");
	}

	public GameObject Create(string minigame_type)
	{
		if (BeenLoaded (minigame_type)
		    || LoadType(minigame_type)) 
		{
            Debug.Log("Minigame Creator: Creating " + minigame_type);
			return GameObject.Instantiate(_loadedObjects[minigame_type]);
		}
		else 
		{
			return null;
		}
	}

	private bool BeenLoaded(string minigame_type)
	{
		if (_loadedObjects != null)
		{
			if(_loadedObjects.ContainsKey(minigame_type))
			{
				return true;
			}
		}

		return false;
	}

	private bool LoadType(string minigame_type)
	{
		//check object exists
        GameObject obj = Resources.Load<GameObject>("MiniGames/MG_" + minigame_type);

        if(obj != null)
        {
            _loadedObjects.Add(minigame_type, obj);
            return true;
        }
        else if(_blankWidget != null)
        {
            _loadedObjects.Add(minigame_type, _blankWidget);
            return true;
        }

		return false;
	}

}
