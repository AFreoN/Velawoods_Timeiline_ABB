using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GenericMisc_Timer_Ring : GenericMisc_Timer_ComponentBase {

	public GameObject _glowPoint;
	protected float _glowPointRadius;
	

	public override void TimerUpdate (float time, float originalTime)
	{
		base.TimerUpdate (time, originalTime);
		
		UpdateGlowPointPosition (time, originalTime);
	}
	
	public override void ColorUpdate (Color color)
	{
		base.ColorUpdate (color);
		
		_glowPoint.GetComponent<Image> ().color = color;
		GetComponent<Image> ().color = color;
	}
	
	
//-Privates------------------------------------------------------------------------------------------------

	private void UpdateGlowPointPosition (float time, float originalTime)
	{
		Vector3 glowPointPos = _glowPoint.transform.localPosition;
		
		float value = (time/originalTime) * (2*Mathf.PI);
		if (float.IsNaN (value)) value = 0.0f;
		
		glowPointPos.x = -1 * Mathf.Sin (value) * _glowPointRadius;
		glowPointPos.y = Mathf.Cos (value) * _glowPointRadius;
		
		_glowPoint.transform.localPosition = glowPointPos;
	}
}





















