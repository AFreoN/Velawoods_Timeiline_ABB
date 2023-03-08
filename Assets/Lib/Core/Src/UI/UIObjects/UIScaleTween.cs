using UnityEngine;
using System.Collections;
using CoreLib;

public class UIScaleTween : MonoBehaviour {

	private RectTransform _rectTrans;

	public void To(float from, float to, float time, RectTransform rect_trans)
	{
		Stop();

		_rectTrans = rect_trans;

		Hashtable ht = iTween.Hash("from",from,"to",to,"time",time,"onupdate","ChangeScale");

		iTween.ValueTo(_rectTrans.gameObject, ht);
	}

	private void ChangeScale(float new_value)
	{
		_rectTrans.localScale = new Vector3(new_value, new_value, new_value);

		if(new_value == 0)
		{
			Stop();
			CoreHelper.SafeDestroy(this);
		}
	}

	public void Stop()
	{
		if(_rectTrans != null)
			iTween.Stop(_rectTrans.gameObject);
	}
}
