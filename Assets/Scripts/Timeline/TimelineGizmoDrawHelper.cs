using UnityEngine;
using UnityEditor.Timeline;

public class TimelineGizmoDrawHelper : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        foreach (var clip in TimelineEditor.selectedClips)
        {
            var drawable = clip.asset as ITimelineGizmoDrawable;
            if (drawable is not null) drawable.OnDrawGizmoSelected();
        }
    }

    /* Draw gizmos and handles on all selected timeline clips
    (requires that tracks playable asset to be derived from ITimelineGizmoDrawable to draw Gizmos or ITimelineSceneDrawable to draw Handles) */
    public void OnSceneGUI()
    {
        foreach(var clip in TimelineEditor.selectedClips)
        {
            var s = clip.asset as ITimelineSceneDrawable;
            if (s is not null) s.OnSceneGuiSelected();
        }
    }
}

public interface ITimelineGizmoDrawable
{
    public void OnDrawGizmoSelected();
}

public interface ITimelineSceneDrawable
{
    public void OnSceneGuiSelected();
}
