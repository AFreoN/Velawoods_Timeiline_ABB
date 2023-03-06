using UnityEngine;
using CustomExtensions;
using System.Collections;
using CustomTracks;

[RequireComponent(typeof(Animator))]
public class FaceLookAt : TimelineBehaviour
{
    Animator anim = null;
    const float angleLimit = 55;

    [SerializeField, Range(0,1)] float lookWeight = 1;
    LookType lookType = LookType.FreeLook;

    [SerializeField] Transform head = null;
    public Transform Head => head;

    [SerializeField] bool debugTarget = false;    //Debug current target position in Scene View (uses Gizmos)

    float forwardDirectionMultiplier = 5;

    Transform target = null;
    Transform prevTarget = null;

    float targetTimer = 0;
    bool lookAtInitialized = false;
    Vector3 currentTargetPosition = new Vector3(-99,0,0), prevPosition = Vector3.positiveInfinity;

    float transtitionDuration = 1f, timer = 0;
    bool onFreeLook = false;

    Transform defaultLookTarget;

    private void OnEnable()
    {
        anim = GetComponent<Animator>();

        if (defaultLookTarget == null)
        {
            defaultLookTarget = new GameObject("LookTarget:" + name).transform;
            defaultLookTarget.SetParent(head);
            defaultLookTarget.localPosition = Vector3.forward;

            foreach(Transform child in GetComponentsInChildren<Transform>())
            {
                if(child.gameObject.name == "Character1_Hips")
                {
                    defaultLookTarget.SetParent(child);
                }
            }
            //defaultLookTarget.localPosition = head.forward * forwardDirectionMultiplier + head.position;
        }
    }

    [ContextMenu("Select default look target")]
    void selectDefault()
    {
        UnityEditor.Selection.activeGameObject = defaultLookTarget.gameObject;
    }

    /// <summary>
    /// Called when the LookAtTrack clip starts in the timeline
    /// </summary>
    /// <param name="o">LookAtBehaviour object</param>
    public override void OnClipStart(object o)
    {
        //Execture action if given object is LookAtBehaviour (uses extension method)
        o.executeAction((LookAtBehaviour lb) =>
        {
            lookType = lb.lookType;
            prevTarget = target;

            if (lookAtInitialized == false)
            {
                currentTargetPosition = defaultLookTarget.position;
                lookAtInitialized = true;
            }

            if (lb.lookType == LookType.FreeLook)
                setFreeLook();
            else if (lb.lookType == LookType.Face)
                setTarget(lb.target);
            else if (lb.lookType == LookType.Target)
                setRootTarget(lb.target);
        });
    }

    /// <summary>
    /// Called when the LookAtTrack clip ends in the timeline
    /// </summary>
    /// <param name="o">LookAtBehaviour object</param>
    public override void OnClipEnd(object o)
    {
        setFreeLook();
        lookType = LookType.FreeLook;
    }

    public void setFreeLook()   //Set the rotation of the character to be controlled completely by animation
    {
        prevPosition = head.forward * forwardDirectionMultiplier + head.position;
        prevPosition = currentTargetPosition;
        //GameObject.CreatePrimitive(PrimitiveType.Sphere).transform.position = prevPosition;

        //target = null;
        onFreeLook = true;
        //timer = 0;
    }

    public void setTarget(Transform _target)    //Set the character head a target to look, requires it parent transform as parameter
    {
        foreach (Transform child in _target.gameObject.GetComponentsInChildren<Transform>())
        {
            if (child.gameObject.name == "Character1_Head")
            {
                //prevPosition = head.forward * 5 + head.position;
                prevPosition = currentTargetPosition;

                target = child;
                onFreeLook = false;
                targetTimer = 0;
            }
        }

        if (target == null)
            lookType = LookType.FreeLook;
    }

    public void setRootTarget(Transform _target)    //Set the target to the transform sent in parameter
    {
        if (_target == null) return;
        prevPosition = currentTargetPosition;
        //GameObject.CreatePrimitive(PrimitiveType.Sphere).transform.position = prevPosition;

        target = _target;
        targetTimer = 0;
        onFreeLook = false;
    }

    private void OnAnimatorIK(int layerIndex)
    {
        float clampedTimer = Mathf.Clamp(ease(timer), 0, 1);
        anim.SetLookAtWeight(clampedTimer);
        anim.SetLookAtPosition(currentTargetPosition);
        //if(!onFreeLook && target != null)
        return;
        //if (layerIndex != 1) return; 
    }

    private void Update()
    {
        if  (lookType == LookType.Face || lookType == LookType.Target)// && target != null)
        {
            Vector3 targetPos = target.position;
            if (targetTimer < 1)
            {
                targetTimer += Time.deltaTime / transtitionDuration;
                if (prevTarget != null)
                    targetPos = Vector3.Lerp(prevPosition, target.position, ease(targetTimer));
            }

            if (timer < 1)
            {
                timer +=  Time.deltaTime / transtitionDuration;
                //Vector3 targetPos = target.position;
                //if (prevTarget != null)
                //    targetPos = Vector3.Lerp(prevTarget.position, target.position, ease(timer));
                currentTargetPosition = Vector3.Lerp(prevPosition, targetPos, ease(timer));
            }
            else
            {

                currentTargetPosition = targetPos;
            }
        }
        else if(onFreeLook)
        {
            timer -= Time.deltaTime / transtitionDuration;
            currentTargetPosition = Vector3.Lerp(defaultLookTarget.position, prevPosition, ease(timer)); //Lerps from 1 to 0
            //targetTimer = Mathf.Clamp01(timer);
            if (timer <= 0)
            {
                onFreeLook = false;
                prevTarget = null;
                targetTimer = 0;
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (debugTarget && target != null)
        {
        }
        if (debugTarget)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(currentTargetPosition, .2f);
        }
    }

    float ease(float _t)
    {
        //return _t;
        return easeInOutCubic(_t);
    }

    float easeInOutCubic(float x)
    {
        return x < 0.5 ? 4 * x * x * x : 1 - Mathf.Pow(-2 * x + 2, 3) / 2;
    }
}
