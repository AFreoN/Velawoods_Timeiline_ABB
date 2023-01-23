using UnityEngine;
using UnityEditor.Timeline;
using UnityEngine.Timeline;
using CustomExtensions;

[CustomTimelineEditor(typeof( TweenAsset ))]
public class TweenClipDraw : ClipEditor
{
    public override ClipDrawOptions GetClipOptions(TimelineClip clip)
    {
        TweenAsset ta = (TweenAsset)clip.asset;

        var clipOptions = base.GetClipOptions(clip);

        if(ta.behaviour.translateType != TweenBehaviour.TranslateType.Hold)
        {
            //return base.GetClipOptions(clip);
            return clipOptions;
        }
        else
        {
            clipOptions.displayClipName = false;
            clipOptions.highlightColor = new Color32(0, 0, 64, 255);//.ToColor();
            return clipOptions;
        }
    }
}
