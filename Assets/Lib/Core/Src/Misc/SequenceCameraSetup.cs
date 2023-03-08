using UnityEngine;
using System.Collections;


public class SequenceCameraSetup : MonoBehaviour
{
	public Vector3 initialCameraPosition = Vector3.zero;
    public Vector3 initialCameraRotation = Vector3.zero;
	
	
	void Start()
	{
		if(Camera.main == null) {
			Debug.LogError("SequenceCameraSetup: Camera.main is null!");
			return;
		}
		
		Camera.main.transform.position = initialCameraPosition;
		Camera.main.transform.eulerAngles = initialCameraRotation;
	}
}
