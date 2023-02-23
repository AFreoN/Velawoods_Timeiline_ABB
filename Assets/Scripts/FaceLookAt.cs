using UnityEngine;
using CustomExtensions;
using System.Collections;

[RequireComponent(typeof(Animator))]
public class FaceLookAt : TimelineBehaviour
{
    Animator anim = null;
    const float angleLimit = 55;

    [SerializeField, Range(0,1)] float lookWeight = 1;
    LookType lookType;

    [SerializeField] Transform head = null;
    public Transform Head => head;

    [SerializeField] bool debugTarget = false;    //Debug current target position in Scene View (uses Gizmos)

    float forwardDirectionMultiplier = 5;

    Transform target = null;
    Vector3 currentTargetPosition = Vector3.zero, prevPosition = Vector3.positiveInfinity;

    float transtitionDuration = .5f, timer = 0;
    bool onFreeLook = false;

    Transform defaultLookTarget;

    private void OnEnable()
    {
        anim = GetComponent<Animator>();

        if (defaultLookTarget == null)
        {
            defaultLookTarget = new GameObject("LookTarget:" + name).transform;
            defaultLookTarget.SetParent(transform);
            defaultLookTarget.localPosition = head.forward * forwardDirectionMultiplier + head.position;
        }
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

        //StartCoroutine(freeLook());
        target = null;
        onFreeLook = true;
        timer = 0;
    }

    IEnumerator freeLook()
    {
        yield return new WaitForEndOfFrame();


        //if (lookType == LookType.FreeLook)
            timer = 0;
    }

    public void setTarget(Transform _target)    //Set the character head a target to look, requires it parent transform as parameter
    {
        foreach (Transform child in _target.gameObject.GetComponentsInChildren<Transform>())
        {
            if (child.gameObject.name == "Character1_Head")
            {
                prevPosition = head.forward * 5 + head.position;

                target = child;
                timer = 0;
                onFreeLook = false;
            }
        }
    }

    public void setRootTarget(Transform _target)    //Set the target to the transform sent in parameter
    {
        prevPosition = head.parent.forward * 5 + head.parent.position;

        target = _target;
        timer = 0;
        onFreeLook = false;
    }

    private void OnAnimatorIK(int layerIndex)
    {
        //if (layerIndex != 1) return; 
        if (target != null)
        {
            //Vector3 eyePosition = target.TransformPoint(new Vector3(0, 0.08493738f, 0.1173861f));

            anim.SetLookAtWeight(Mathf.Clamp(timer, 0, 1));
            anim.SetLookAtPosition(currentTargetPosition);
        }
        else if(onFreeLook)
        {
            float clampedTimer =   ( 1 - Mathf.Clamp(timer, 0, 1));
            anim.SetLookAtWeight(clampedTimer);
            anim.SetLookAtPosition(currentTargetPosition);
        }
        else if(!onFreeLook)
        {
            //If there is no target and on free look, set look weight to zero
            anim.SetLookAtWeight(0);
        }
    }

    private void Update()
    {
        if (target != null)
        {
            if (timer < 1)
            {
                timer += Time.deltaTime / transtitionDuration;
                currentTargetPosition = Vector3.Lerp(prevPosition, target.position, timer);
            }
            else
                currentTargetPosition = target.position;
        }
        else if(onFreeLook)
        {
            timer += Time.deltaTime / transtitionDuration;
            currentTargetPosition = Vector3.Lerp(prevPosition, defaultLookTarget.position, timer);
            if (timer >= 1)
                onFreeLook = false;
        }
    }

    private void OnDrawGizmos()
    {
        if (debugTarget && target != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(target.position, .2f);
        }
    }
}
