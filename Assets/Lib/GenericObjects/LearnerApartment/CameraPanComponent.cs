using UnityEngine;

public class CameraPanComponent : ApartmentCameraComponent
{
	private const float ROT_LEFT_AMOUNT = 5;
	private const float ROT_RIGHT_AMOUNT = 5;
	private const float MAX_ROT_SPEED = 2.5f;
	private const float ROT_ACCEL = 1.25f;
	private GameObject camera;
	private Quaternion cameraRot;
	private Vector3 initialRot;
	private bool rotatingRight;
	private float rotSpeed;

	public override void Init (GameObject camera)
	{
		this.camera = camera;
		cameraRot = camera.transform.rotation;
		initialRot = cameraRot.eulerAngles;
		rotatingRight = true;
	}
	
	public override void Update ()
	{
		float rotAmount;
		if(rotatingRight)
		{
			if(cameraRot.eulerAngles.y > initialRot.y + ROT_RIGHT_AMOUNT)
			{
				rotatingRight = false;
			}
			if(rotSpeed < MAX_ROT_SPEED)
			{
				rotSpeed += ROT_ACCEL * Time.deltaTime;
			}
		}
		else
		{
			if(cameraRot.eulerAngles.y < initialRot.y - ROT_RIGHT_AMOUNT)
			{
				rotatingRight = true;
			}
			if(rotSpeed > -MAX_ROT_SPEED)
			{
				rotSpeed -= ROT_ACCEL * Time.deltaTime;
			}
		}
		rotAmount = rotSpeed * Time.deltaTime;
		cameraRot.eulerAngles = new Vector3(cameraRot.eulerAngles.x,
		                                    cameraRot.eulerAngles.y + rotAmount,
		                                    cameraRot.eulerAngles.z);
		camera.transform.rotation = cameraRot;
	}
}
