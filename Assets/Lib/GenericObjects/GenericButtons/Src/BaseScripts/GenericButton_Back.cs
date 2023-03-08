using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GenericButton_Back : GenericObject {

	private GameObject _icon;
	public GameObject Icon {
		get {
			if (_icon == null)
			{
				_icon = Ring.transform.Find ("Icon").gameObject;
			}
			return _icon;
		}
	}
	
	private GameObject _ring;
	public GameObject Ring {
		get {
			if (_ring == null)
			{
				_ring = transform.Find ("Ring").gameObject;
			}
			return _ring;
		}
	}

	private static Color _iconColor = new Color (203.0f/255.0f, 17.0f/255.0f, 34.0f/255.0f, 1.0f);
	
	
	public void OnDestroy ()
	{
		StopAllCoroutines ();
	}
	
	//--
	
	public bool Interactable {
		get { return GetComponent<Button> ().interactable; }
		set {
			GetComponent<Button> ().interactable = value;
			UITween.fadeTo (Icon, (value) ? _iconColor : Color.grey, 0.1f, UITween.UIFadeType.easeInSine, false);
			UITween.fadeTo (Ring, (value) ? _iconColor : Color.grey, 0.1f, UITween.UIFadeType.easeInSine, false);
		}
	}
	
	public override void Show (object[] paramList)
	{
		base.Show (paramList);
	}
	
	public override void Hide (object[] paramList)
	{
		base.Hide (paramList);
	}
	
	public void SetInteractableAfter (bool interactable, float time)
	{
		StartCoroutine (SetInteractableAfterRoutine (interactable, time + 0.1f));
	}
	
	//--
	
	private IEnumerator SetInteractableAfterRoutine (bool interactable, float time)
	{
		yield return new WaitForSeconds (time);
		Interactable = interactable;
	}
	
}
