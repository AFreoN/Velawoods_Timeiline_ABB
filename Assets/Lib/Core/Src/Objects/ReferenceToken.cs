using CoreSystem;
using UnityEngine;
using UnityEngine.UI;

public class ReferenceToken : MonoBehaviour 
{
    #region Attributes

    // Constants ---------------------------------------------------------------

    private int CLOSE_BUTTON_TEMP_SORT_ORDER = 3;
    private int CONTENT_TEMP_SORT_ORDER = 2;
    private int PREFAB_TEMP_SORT_ORDER = 1;

    // Public Variables --------------------------------------------------------

    public Animator PrefabAnimator;
    public Animator ContentAnimator;
	public Animator CloseButtonAnimator;

    // Private Variables -------------------------------------------------------

    private int _closeButtonAnimatorOriginalSortOrder;
    private int _contentAnimatorOriginalSortOrder;
    private int _prefabAnimatorOriginalSortOrder;

    private Canvas _uiLayerCanvasComponent;
    private Canvas _menuLayerCanvasComponent;

    #endregion

    #region Functions

    // MonoBehaviour Functions -------------------------------------------------

    public void OnEnable()
    {
        // Store original sorting orders
        StoreOriginalSettings();
    }

    public void OnDisable()
    {
        // Restore original sorting orders
        RestoreOriginalSettings();
    }

    // Public Functions --------------------------------------------------------

    public void Open()
    {
        // Show the close button
        if (CloseButtonAnimator != null &&
        CloseButtonAnimator.HasState(0, Animator.StringToHash("SlideIn_A2_CloseButton")))
        {
            CloseButtonAnimator.Play("SlideIn_A2_CloseButton");
        }

        // Show the content
        if (ContentAnimator != null &&
        ContentAnimator.HasState(0, Animator.StringToHash("SlideIn_A2_Content")))
        {
            ContentAnimator.Play("SlideIn_A2_Content");
        }

        // Show the background(prefab)
        if (PrefabAnimator != null &&
        PrefabAnimator.HasState(0, Animator.StringToHash("SlideIn_A2")))
        {
            PrefabAnimator.Play("SlideIn_A2");
        }
    }

    public void Close()
    {
        // Hide the close button
        if (CloseButtonAnimator != null &&
            CloseButtonAnimator.HasState(0, Animator.StringToHash("SlideOut_A2_CloseButton")))
        {
            CloseButtonAnimator.Play("SlideOut_A2_CloseButton");
        }

        // Hide the content
        if (ContentAnimator != null &&
            ContentAnimator.HasState(0, Animator.StringToHash("SlideOut_A2_Content")))
        {
            ContentAnimator.Play("SlideOut_A2_Content");
        }

        // Hide the background(prefab)
        if (PrefabAnimator != null &&
            PrefabAnimator.HasState(0, Animator.StringToHash("SlideOut_A2")))
        {
            PrefabAnimator.Play("SlideOut_A2");
        }
    }

    // Private Functions -------------------------------------------------------

    private void StoreOriginalSettings()
    {
        if ((PrefabAnimator != null) && (PrefabAnimator != ContentAnimator))
        {
            Canvas canvas = PrefabAnimator.GetComponent<Canvas>();
            if (canvas != null)
            {
                _prefabAnimatorOriginalSortOrder = canvas.sortingOrder;
                canvas.sortingOrder = PREFAB_TEMP_SORT_ORDER;
            }
        }

        if (ContentAnimator != null)
        {
            Canvas canvas = ContentAnimator.GetComponent<Canvas>();
            if (canvas != null)
            {
                _contentAnimatorOriginalSortOrder = canvas.sortingOrder;
                canvas.sortingOrder = CONTENT_TEMP_SORT_ORDER;
            }
        }

        if (CloseButtonAnimator != null)
        {
            Canvas canvas = CloseButtonAnimator.GetComponent<Canvas>();
            if(canvas != null)
            {
                _closeButtonAnimatorOriginalSortOrder = canvas.sortingOrder;
                canvas.sortingOrder = CLOSE_BUTTON_TEMP_SORT_ORDER;
            }
        }

        // Add components to UI layer and Menu to bring them on top of the reference token
#if CLIENT_BUILD
        _uiLayerCanvasComponent = LayerSystem.Instance.GetLayer(UILayers.UI.ToString()).GetComponent<Canvas>();
        if (_uiLayerCanvasComponent == null)
        {
            _uiLayerCanvasComponent = LayerSystem.Instance.GetLayer(UILayers.UI.ToString()).AddComponent<Canvas>();
        }
        _uiLayerCanvasComponent.overrideSorting = true;
        _uiLayerCanvasComponent.sortingOrder = (CLOSE_BUTTON_TEMP_SORT_ORDER + 1);

        GraphicRaycaster uiLayerGraphicRaycasterComponent = LayerSystem.Instance.GetLayer(UILayers.UI.ToString()).GetComponent<GraphicRaycaster>();
        if(uiLayerGraphicRaycasterComponent == null)
        {
            uiLayerGraphicRaycasterComponent = LayerSystem.Instance.GetLayer(UILayers.UI.ToString()).AddComponent<GraphicRaycaster>();
        }

        // Add components to UI layer and Menu to bring them on top of the reference token
        _menuLayerCanvasComponent = LayerSystem.Instance.GetLayer(UILayers.MainMenu.ToString()).GetComponent<Canvas>();
        if (_menuLayerCanvasComponent == null)
        {
            _menuLayerCanvasComponent = LayerSystem.Instance.GetLayer(UILayers.MainMenu.ToString()).AddComponent<Canvas>();
        }
        _menuLayerCanvasComponent.overrideSorting = true;
        _menuLayerCanvasComponent.sortingOrder = (CLOSE_BUTTON_TEMP_SORT_ORDER + 2);

        GraphicRaycaster menuLayerGraphicRaycasterComponent = LayerSystem.Instance.GetLayer(UILayers.MainMenu.ToString()).GetComponent<GraphicRaycaster>();
        if (menuLayerGraphicRaycasterComponent == null)
        {
            menuLayerGraphicRaycasterComponent = LayerSystem.Instance.GetLayer(UILayers.MainMenu.ToString()).AddComponent<GraphicRaycaster>();
        }
#endif
    }

    private void RestoreOriginalSettings()
    {
        if (CloseButtonAnimator != null)
        {
            Canvas canvas = CloseButtonAnimator.GetComponent<Canvas>();
            if (canvas != null)
            {
                canvas.sortingOrder = _closeButtonAnimatorOriginalSortOrder;
            }
        }

        if (ContentAnimator != null)
        {
            Canvas canvas = ContentAnimator.GetComponent<Canvas>();
            if (canvas != null)
            {
                canvas.sortingOrder = _contentAnimatorOriginalSortOrder;
            }
        }

        if ((PrefabAnimator != null) && (PrefabAnimator != ContentAnimator))
        {
            Canvas canvas = PrefabAnimator.GetComponent<Canvas>();
            if (canvas != null)
            {
                canvas.sortingOrder = _prefabAnimatorOriginalSortOrder;
            }
        }

        // Stop overriding sorting
#if CLIENT_BUILD
        _uiLayerCanvasComponent.overrideSorting = false;
        _menuLayerCanvasComponent.overrideSorting = false;
#endif
    }

    #endregion
}
