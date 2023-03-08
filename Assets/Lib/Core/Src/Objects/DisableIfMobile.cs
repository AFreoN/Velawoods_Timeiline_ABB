using UnityEngine;
using System.Collections;

public class DisableIfMobile : MonoBehaviour 
{
	void Start ()
    {
#if (UNITY_IPHONE || UNITY_ANDROID)
        gameObject.SetActive(false);   
#endif
    }
}
