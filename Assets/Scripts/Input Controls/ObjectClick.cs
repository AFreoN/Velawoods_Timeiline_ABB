using UnityEngine;
using CustomExtensions;
using cakeslice;

public class ObjectClick : MonoBehaviour
{
    Collider col = null;    //Collider attached to this gameobject
    bool addedCollider = false;
    bool playAfterClick = true;

    Transform targetTransform = null;
    System.Action<GameObject> action;
    Camera mainCam = null;  //Main camera in the scene cached

    public void Initialize(Transform _targetTransform, System.Action<GameObject> _action, bool _play)
    {
        targetTransform = _targetTransform;
        action = _action;
        playAfterClick = _play;
    }

    private void OnEnable()
    {
        col = GetComponent<Collider>();

        if (!col)
        {
            col = gameObject.AddComponent<MeshCollider>();
            addedCollider = true;
        }

        mainCam = Camera.main;
    }

    private void Update()
    {
#if UNITY_EDITOR
        MouseControl();
#else
        if (Input.touchSupported)
            TouchControl();
#endif
    }

    void MouseControl()
    {
        if (Input.GetMouseButtonDown(0))
        {
            CheckRayHit(Input.mousePosition);
        }
    }

    void TouchControl()
    {

    }

    //Check if the object is the clicked position
    void CheckRayHit(Vector2 clickPos)
    {
        Ray ray = mainCam.ScreenPointToRay(clickPos);

        if(Physics.Raycast(ray, out RaycastHit hit))
        {
            //if (hit.transform != null)
            //    Debug.Log("Clicking on : " + hit.transform.name);

            if(hit.transform == transform || hit.transform.parent == transform)
            {
                action?.Invoke(gameObject);

                if(playAfterClick)
                    TimelineController.instance.PlayTimeline();     //If this object is clicked, play timeline

                gameObject.executeAction((Outline o) => Destroy(o));     //Destroy outline highlighter

                if (addedCollider && col)
                    Destroy(col);                           //Destroy if collider is added by this component

                Destroy(this);                              //And finally remove this component to not detect touch further
            }
        }
    }

    private void OnDestroy()
    {
        if (addedCollider && col)
            Destroy(col);
    }
}
