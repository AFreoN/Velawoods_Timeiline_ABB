using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GenericMisc_ScrollbarBase : MonoBehaviour {

	// Public

	public ScrollRect _scrollRect = null;

	public bool IsActive {
		get { return _isActive; }
		set {
			_isActive = value;

			MyScrollbar.enabled = value;
			MyImage.enabled = value;
            MyHandle.enabled = value;

        } }

	
	// Private

	private bool _isActive = true;
	private Scrollbar _scrollBar;
	private Image _image;
    private Image _handle;


    // Protected

    protected RectTransform _scrollRectTransform;
	
	protected Scrollbar MyScrollbar {
		get {
			if (_scrollBar == null)
				_scrollBar = this.GetComponent<Scrollbar> ();
			return _scrollBar;
		} }
	protected Image MyImage {
		get {
			if (_image == null)
				_image = this.GetComponent<Image> ();
			return _image;
		} }
    protected Image MyHandle
    {
        get
        {
            if (_handle == null)
                _handle = transform.Find("Sliding Area/Handle").GetComponent<Image>();
            return _handle;
        }
    }


    // Init 

    public void Start ()
	{
		if (_scrollRect == null)
		{
			Debug.LogError ("(!) Generic Scrollbar : Required Scroll Rect field is null!");
			return;
		}

		IsActive = false;
		_scrollRectTransform = _scrollRect.GetComponent<RectTransform> ();

		OnStart ();
	}

	// Update 

	public void Update ()
	{
		if (_scrollRect == null)
			return;

		OnUpdate ();
	}

	//

	protected virtual void OnStart ()
	{
		// override me. awh yeah
	}

	protected virtual void OnUpdate ()
	{
		// override me. awh yeah
	}
}
