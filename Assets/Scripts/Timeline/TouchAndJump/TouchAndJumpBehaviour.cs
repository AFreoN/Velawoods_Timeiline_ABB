using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System.Collections.Generic;
using cakeslice;
using CustomExtensions;

[Serializable]
public class TouchAndJumpBehaviour : PlayableBehaviour
{
    public List<TouchAndJumpClip.TouchableData> touchables = new List<TouchAndJumpClip.TouchableData>();
    [HideInInspector] public List<GameObject> touchableGameObjects { get 
        {
            if (touchables == null) return null;

            List<GameObject> result = new List<GameObject>();
            foreach (TouchAndJumpClip.TouchableData td in touchables)
                result.Add(td.touchObject);

            return result;
        } 
    }

    [HideInInspector] public bool pause = true;
    bool isTriggerered = false;

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying) return;
#endif

        if (touchables == null || touchables.Count == 0 || isTriggerered) return;

        for(int i = 0; i < touchables.Count; i++)
        {
            TouchAndJumpClip.TouchableData data = touchables[i];

            Outline o = data.touchObject.GetComponent<Outline>();

            if(data.shouldFlash && o == null)
            {
                data.touchObject.AddComponent<Outline>();
            }

            data.touchObject.executeAction((ObjectClick obj) => UnityEngine.Object.Destroy(obj));
            data.touchObject.AddComponent<ObjectClick>().Initialize(data.touchObject.transform, OnTouch, false);
        }

        isTriggerered = true;
        if(pause)
            TimelineController.instance.PauseTimeline();
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying) return;
#endif

        if (playable.isPlayableCompleted(info))
        {
            for (int i = 0; i < touchables.Count; i++)
            {
                TouchAndJumpClip.TouchableData data = touchables[i];

                data.touchObject.executeAction((Outline o) => UnityEngine.Object.Destroy(o));
                data.touchObject.executeAction((ObjectClick o) => UnityEngine.Object.Destroy(o));
            }

            isTriggerered = false;
        }
    }

    public void OnTouch(GameObject _touchedObject)
    {
        int index = touchableGameObjects.IndexOf(_touchedObject);
        TouchAndJumpClip.TouchableData td = touchables[index];
        if(td.skipTo != -1f)
        {
            for (int i = 0; i < touchables.Count; i++)
            {
                TouchAndJumpClip.TouchableData data = touchables[i];

                data.touchObject.executeAction((Outline o) => UnityEngine.Object.Destroy(o));
                data.touchObject.executeAction((ObjectClick o) => UnityEngine.Object.Destroy(o));
            }

            isTriggerered = false;

            TimelineController.instance.SkipTimeline(td.skipTo, true);
            Camera.main.GetComponent<CameraApartmentController>().ObjectTouched(null);
        }
        //Debug.Log("Calling on touch from : " + _touchedObject.name);
    }

    public override void OnGraphStop(Playable playable)
    {
        if (touchables != null)
            touchables.Clear();
    }
}
