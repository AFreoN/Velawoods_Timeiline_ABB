using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using UnityEditor.Timeline;
using System.Reflection;

[InitializeOnLoad]
static class ShortcutsTimeline
{

    static ShortcutsTimeline()
    {
        SceneView.duringSceneGui += SceneGUI;
    }

    static void SceneGUI(SceneView sceneView)
    {
        if (Selection.activeGameObject == null) return;

        Event e = Event.current;
        switch (e.type)
        {
            case EventType.KeyDown:
                if (Event.current.keyCode == KeyCode.T)
                {
                    Transform t = Selection.activeTransform;

                    Selection.activeObject = getTimelineClip(t.name);
                    InvokeInternalFrameTrack();
                    

                    //Bounds b = new Bounds();
                    //b.center = t.position;
                    //b.size = Vector3.one * 2;
                    //SceneView.lastActiveSceneView.Frame(b);
                }
                break;
        }
    }

    static Object getTimelineClip(string _name)
    {
        Object result = null;

        var tracks = TimelineEditor.masterAsset.GetRootTracks();

        //Debug.Log("Tracks count : " + )

        foreach(TrackAsset t in tracks)
        {
            if(t.name == _name)
            {
                result = t;
                t.SetCollapsed(false);
                TimelineEditor.Refresh(RefreshReason.ContentsAddedOrRemoved);
                break;
            }
            foreach (TrackAsset c in t.GetChildTracks())
            {
                if (c.name == _name)
                {
                    result = c;
                    t.SetCollapsed(false);
                    c.SetCollapsed(false);
                    TimelineEditor.Refresh(RefreshReason.ContentsAddedOrRemoved);
                    break;
                }
            }

        }

        return result;
    }

    [MenuItem("GameObject/Show in Timeline")]
    static void ShowInTimeline()
    {
        if (Selection.activeTransform == null) return;

        Object select = getTimelineClip(Selection.activeTransform.name);
        if(select)
            Selection.activeObject = select;
        InvokeInternalFrameTrack();
    }

    
    static void InvokeInternalFrameTrack()
    {
        const string keyboardNavigationTypeName = "UnityEditor.Timeline.KeyboardNavigation";
        const string frameTrackHeaderMethodName = "FrameTrackHeader";

        Assembly timelineEditorAssembly = typeof(TimelineEditorWindow).Assembly;
        System.Type keyboardNavigation = timelineEditorAssembly.GetType(keyboardNavigationTypeName);
        MethodInfo method = keyboardNavigation.GetMethod(frameTrackHeaderMethodName);
        method.Invoke(null, new[] { System.Type.Missing });
    }
}
