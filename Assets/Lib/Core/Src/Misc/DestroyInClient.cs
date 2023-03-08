using UnityEngine;
using System.Collections;

public class DestroyInClient : MonoBehaviour 
{
	// Use this for initialization
	void Start () 
    {
#if CLIENT_BUILD
        Debug.LogError("Game object: " + this.gameObject.name + " has been left in the scene when it is marked not to be.");
        Object.Destroy(this.gameObject);
#endif
    }
	
	// Update is called once per frame
	void Update () 
    {
	
	}
}
