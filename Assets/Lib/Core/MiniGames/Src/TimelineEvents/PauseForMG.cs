using UnityEngine;
using System.Collections;

public class PauseForMG : ITimelineBehaviour
{
    public double startTime { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public double endTime { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }


    public void OnSkip()
    {
        FireEvent();
    }

    public void FireEvent()
    {
        MiniGameManager.Instance.TriggerPause();
    }

	public virtual void OnReset()
	{
		
	}


}
