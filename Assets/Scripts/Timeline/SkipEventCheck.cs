using UnityEngine;

public class SkipEventCheck : TimelineBehaviour
{
    public override void OnClipStart(object o)
    {
        Debug.Log("Event started at : " + name);
    }

    public override void OnClipEnd(object o)
    {
        //PlayableInstance.RemovePlayable(this);
    }

    public override void OnProcessFrame(object o)
    {
        base.OnProcessFrame(o);
    }

    public override void OnSkip()
    {
        base.OnSkip();
        Debug.Log("SKip called on : " + name);
    }
}
