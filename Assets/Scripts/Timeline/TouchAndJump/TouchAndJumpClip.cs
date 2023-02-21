using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class TouchAndJumpClip : PlayableAsset, ITimelineClipAsset
{
    public ExposedTouchAndJump[] touchables;
    [HideInInspector] public TouchAndJumpBehaviour behaviour = new TouchAndJumpBehaviour ();

    public ClipCaps clipCaps
    {
        get { return ClipCaps.ClipIn; }
    }

    public override Playable CreatePlayable (PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<TouchAndJumpBehaviour>.Create (graph, behaviour);
        TouchAndJumpBehaviour clone = playable.GetBehaviour ();

        if(touchables != null)
        {
            foreach (var v in touchables)
            {
                GameObject g = v.ExposedReference.Resolve(graph.GetResolver());
                TouchableData td = new TouchableData(g, v.shouldFlash, v.skipTo);
                clone.touchables.Add(td);
            }
        }

        return playable;
    }

    [Serializable]
    public class ExposedTouchAndJump : ExposedReferenceHolder<GameObject>, ISerializationCallbackReceiver
    {
        public bool shouldFlash = true;
        public float skipTo = -1f;

        [HideInInspector] public bool serialized = false;

        public ExposedTouchAndJump()
        {
            shouldFlash = true;
            skipTo = -1f;
        }

        public void OnAfterDeserialize()
        {
            //throw new NotImplementedException();
        }

        public void OnBeforeSerialize()
        {
            if (serialized)
                return;

            serialized = true;
            shouldFlash = true;
            skipTo = -1f;
        }
    }

    public class TouchableData
    {
        public GameObject touchObject;
        public bool shouldFlash;
        public float skipTo;

        public TouchableData(GameObject _g, bool _flash, float _skip = -1f)
        {
            touchObject = _g;
            shouldFlash = _flash;
            skipTo = _skip;
        }
    }
}
