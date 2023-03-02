using UnityEngine;

public class DialogArrow : MonoBehaviour
{
    [SerializeField] RectTransform rect = null;
    Transform target = null;
    Camera cam = null;

    [SerializeField] float transitionDuration = 0.3f;
    float timer = 0;

    Vector3 startPos = Vector3.zero, endPos = Vector3.zero;

    private void Start()
    {
        cam = Camera.main;
        startPos = rect.position;
    }

    public void changeTarget(GameObject _target)
    {
        if (_target == null) return;
        target = _target.transform;

        endPos = cam.WorldToScreenPoint(target.position);
        endPos.y = startPos.y;
        endPos.z = 0;

        timer = 0;
    }

    private void Update()
    {
        if (!target) return;

        if (timer >= 1) return;

        timer += Time.deltaTime / transitionDuration;

        timer = Mathf.Clamp(timer, 0, 1);

        rect.position = Vector3.Lerp(startPos, endPos, timer);
    }
}
