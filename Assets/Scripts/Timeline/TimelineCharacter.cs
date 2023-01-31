using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AnimPlayer))]
public class TimelineCharacter : MonoBehaviour
{
    private void OnEnable()
    {
        //TimelineController.onTimelineStateChange += playAnimation;
    }

    private void OnDisable()
    {
        //TimelineController.onTimelineStateChange -= playAnimation;
    }

    /// <summary>
    /// Continue playing animation seamlessly when timeline is paused, revert back animation control to timeline when it's resumed
    /// </summary>
    /// <param name="isPaused">Is Timeline paused</param>
    public void playAnimation(bool isPaused)
    {
        if (isPaused)
        {
            //if (anim != null)
            //    anim.Play(animName);
            AnimPlayer at = GetComponent<AnimPlayer>();
            if(at != null)
                at.playAnimation();
        }
        else
        {
            AnimPlayer at = GetComponent<AnimPlayer>();
            if (at != null)
                at.resetAnimation();
        }
    }
}
