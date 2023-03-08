using UnityEngine;
using System.Collections;

public class GenericButton_Record_RecordPulse : MonoBehaviour {
	[Header ("Record Pulse")] [Space (5)]
	
	public float _maxScale = 2.3f;
	public float _minScale = 1.3f;
	public float _showTime = 0.5f;
	public float _cycleTime = 2.0f;
	public float _timingOffset = 0.2f;
	
	
	//-Interface-------------------------------------------------------------------------------------------------

	public void StartPulse ()
	{
		stopCoroutines ();
		if(gameObject.activeSelf)
			StartCoroutine ("SetPulse", new object[] {true});
	}
	
	public void StopPulse ()
	{
		stopCoroutines ();
		if(gameObject.activeSelf)
			StartCoroutine ("SetPulse", new object[] {false});
	}
	
	
	//-Coroutines----------------------------------------------------------------------------------------------
	
	private IEnumerator SetPulse (object[] paramList)
	{
		bool show = (bool) paramList [0];
		
		for (int i=0; i<transform.childCount; i++)
		{
			Transform circle = transform.GetChild ((show) ? transform.childCount - (i+1) : i);
			
			float finalScale = (show) ? _minScale  :  1;	
			iTween.ScaleTo (circle.gameObject, iTween.Hash ("x", finalScale, "y", finalScale , "time", _showTime, "islocal", true, "easetype", "easeOutQuad"));
		}
		yield return new WaitForSeconds (_showTime / 2.0f);
		
		if (show)
		{
			StartCoroutine ("StartPulseRoutine");
		}
	}
	
	private IEnumerator StartPulseRoutine ()
	{
		for (int i=0; i<transform.childCount; i++)
		{
			Transform circle = transform.GetChild (transform.childCount - (i+1));
			
			float finalMaxScale = _minScale + (i+1) * ((_maxScale - _minScale) / transform.childCount);	
			StartCoroutine ("Pulse", new object[] {circle.gameObject, finalMaxScale, _minScale});
			
			yield return new WaitForSeconds (_timingOffset);
		}
	}
	
	private IEnumerator Pulse (object[] paramList)
	{
		GameObject circleObj = (GameObject) paramList [0];
		float finalMaxScale = (float) paramList [1];
		float minScale = (float) paramList [2];
		
		iTween.ScaleTo (circleObj, iTween.Hash ("x", finalMaxScale, "y", finalMaxScale, "time", _cycleTime / 2.0f, "islocal", true, "easetype", "easeInOutQuad"));
		yield return new WaitForSeconds (_cycleTime / 2.0f + 0.001f);
		iTween.ScaleTo (circleObj, iTween.Hash ("x", minScale, "y", minScale, "time", _cycleTime / 2.0f, "islocal", true, "easetype", "easeInOutSine"));
		yield return new WaitForSeconds (_cycleTime / 2.0f + 0.001f);
		
		StartCoroutine ("Pulse", new object[] {circleObj, finalMaxScale, minScale});
	}
	
	private void stopCoroutines ()
	{
		StopCoroutine ("SetPulse");
		StopCoroutine ("StartPulseRoutine");
		StopCoroutine ("Pulse");
	}
}
