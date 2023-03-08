using UnityEngine;
using System.Collections;
using System;

public class FadeCanvasGroup : MonoBehaviour {

	public CanvasGroup _canvasGroup;
	public Action<float> FadeComplete = delegate {};

	private float _timeTaken = 0.5f;
	private float _fadeTo = 1.0f;
	private float _delay = 0.0f;

	private bool _AfterFadeToggle = false;

	public void FadeTo(float to, float time=0.5f, float delay=0.0f)
	{
		if(_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();

		_delay = delay;
		_timeTaken = time;
		_fadeTo = to;

		StartCoroutine("WaitThenFade");
	}



	public void FadeWithToggle (float to, float time=0.5f, float delay=0.0f){
		if(_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
		
		_delay = delay;
		_timeTaken = time;
		_fadeTo = to;

		if (to == 1f) _AfterFadeToggle = true; // It will become interactable after fading 
		else{
			DoToggle(false);
		}
		
		StartCoroutine("WaitThenFade");
	}

	void DoToggle (bool b){
		_canvasGroup.interactable = b;
		_canvasGroup.blocksRaycasts = b;
	}

	private IEnumerator WaitThenFade()
	{
		yield return new WaitForSeconds(_delay);
		
		Hashtable ht = iTween.Hash("from",_canvasGroup.alpha,"to",_fadeTo,"time",_timeTaken,"onupdate","changeAlpha", "oncomplete", "OnComplete");
		
		//make iTween call:
		iTween.ValueTo(gameObject,ht);
	}

	private void OnComplete()
	{
		if (_AfterFadeToggle){
			_AfterFadeToggle = false;
			DoToggle (true);
		}
		FadeComplete (_fadeTo);
	}

	private void changeAlpha(float newValue)
	{
		_canvasGroup.alpha = newValue;
	}
}
