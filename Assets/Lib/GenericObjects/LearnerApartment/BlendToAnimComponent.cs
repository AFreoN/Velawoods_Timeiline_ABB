using UnityEngine;

public class BlendToAnimComponent : ApartmentCameraComponent
{
	private GameObject camera;
	private GameObject secondCamera;
	private float blendStateTime;

    private float currentLength = 1.0f;
    private float currentLerpValue;

	AnimPlayer.TimelineAnimationClips currentTimelineClip = null;

	public override void Init (GameObject camera)
	{
		this.camera = camera;
		secondCamera = GameObject.Find ("Camera");

        Animator animator = secondCamera.GetComponent<Animator>();

        if (animator)
        {
            currentTimelineClip = secondCamera.GetComponent<AnimPlayer>().getCurrentTimelineClip((float)TimelineController.instance.currentPlayableTime);

            if (currentTimelineClip != null)
            {
                currentLength = currentTimelineClip.endTime - currentTimelineClip.startTime;

                if (currentLength <= 0.5f)
                    currentLength = 0.001f;
            }
        }
    }

	public override void Update ()
	{
		if(currentTimelineClip == null)
        {
			currentTimelineClip = secondCamera.GetComponent<AnimPlayer>().getCurrentTimelineClip((float)TimelineController.instance.currentPlayableTime);

			if (currentTimelineClip != null)
			{
				currentLength = currentTimelineClip.endTime - currentTimelineClip.startTime;

				if (currentLength <= 0.5f)
					currentLength = 0.001f;
			}
		}
        
        currentLerpValue += Time.deltaTime;

		double final = 1;
		if(currentTimelineClip != null)
			final = CustomExtensions.Extensions.InverseLerp(currentTimelineClip.startTime, currentTimelineClip.endTime, TimelineController.instance.currentPlayableTime);

		float currentValue = (float)CubicEaseInOut((float)final, 0.0f, 1.0f, 1.0f);
        camera.transform.rotation = Quaternion.Lerp(camera.transform.rotation, secondCamera.transform.rotation, currentValue);
		camera.transform.position = Vector3.Lerp(camera.transform.position, secondCamera.transform.position, currentValue);
	}


	/// <summary>
	/// Easing equation function for a cubic (t^3) easing in/out: 
	/// acceleration until halfway, then deceleration.
	/// </summary>
	/// <param name="t">Current time in seconds.</param>
	/// <param name="b">Starting value.</param>
	/// <param name="c">Final value.</param>
	/// <param name="d">Duration of animation.</param>
	/// <returns>The correct value.</returns>
	public static double CubicEaseInOut(double t, double b, double c, double d)
	{
		if ((t /= d / 2) < 1)
		{
			return c / 2 * t * t * t + b;
		}

		return c / 2 * ((t -= 2) * t * t + 2) + b;
	}
}
