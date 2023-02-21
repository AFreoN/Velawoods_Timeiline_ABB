using UnityEngine;

public class SnapToCameraComponent : ApartmentCameraComponent
{
	public override void Init (GameObject camera)
	{
		GameObject secondCamera = GameObject.Find ("Camera");
		camera.transform.rotation = secondCamera.transform.rotation;
		camera.transform.position = secondCamera.transform.position;
	}
	
	public override void Update () {}
}
