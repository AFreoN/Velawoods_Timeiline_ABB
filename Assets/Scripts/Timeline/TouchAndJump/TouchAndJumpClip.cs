using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace CustomTracks
{
    [Serializable]
    public class TouchAndJumpClip : PlayableAsset, ITimelineClipAsset
    {
        public ExposedTouchAndJump[] touchables;
        [HideInInspector] public TouchAndJumpBehaviour behaviour = new TouchAndJumpBehaviour();

        public ClipCaps clipCaps
        {
            get { return ClipCaps.ClipIn; }
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<TouchAndJumpBehaviour>.Create(graph, behaviour);
            TouchAndJumpBehaviour clone = playable.GetBehaviour();

            if (touchables != null)
            {
                foreach (var v in touchables)
                {
                    GameObject g = v.ExposedReference.Resolve(graph.GetResolver());
                    TouchableData td = new TouchableData(g, v.jumpToPrefabNames, v.shouldFlash, v.skipTo);
                    clone.touchables.Add(td);

                    #if CLIENT_BUILD
                    if (TimelineController.instance)
                    {
                        v.ExposedReference.exposedName = UnityEditor.GUID.Generate().ToString();
                        TimelineController.instance.getPlayableDirector().SetReferenceValue(v.ExposedReference.exposedName, g);
                    }
                    #endif
                }
            }

            return playable;
        }

        [Serializable]
        public class ExposedTouchAndJump : ExposedReferenceHolder<GameObject>, ISerializationCallbackReceiver
        {
            public float skipTo = -1f;
            public string jumpToPrefabNames;
            public bool shouldFlash = true;

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
            public float skipTo;
            public string jumpToPrefabNames;
            public bool shouldFlash;

            public TouchableData(GameObject _g, string _jumpToPrefabNames, bool _flash, float _skip = -1f)
            {
                touchObject = _g;
                shouldFlash = _flash;
                skipTo = _skip;
                jumpToPrefabNames = _jumpToPrefabNames;
            }
        }
    }
}
