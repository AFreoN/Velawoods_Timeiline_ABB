using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System.Collections.Generic;

namespace CustomTracks
{
    [TrackClipType(typeof(Fade3DObjectBehaviour))]
    [Serializable]
    public class Fade3DObjectClip : PlayableAsset, ITimelineClipAsset
    {
        public ExposedGameObject[] fadeObjects = null;

        public Fade3DObjectBehaviour behaviour = new Fade3DObjectBehaviour();

        public ClipCaps clipCaps
        {
            get { return ClipCaps.ClipIn; }
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<Fade3DObjectBehaviour>.Create(graph, behaviour);
            Fade3DObjectBehaviour clone = playable.GetBehaviour();

            //for(int i = 0; i < fadeObjects.Length; i++)
            //{
            //    clone.addFadeObject(fadeObjects[i].ExposedReference.Resolve(graph.GetResolver()));
            //}
            if (fadeObjects != null)
            {
                foreach (var v in fadeObjects)
                {
                    GameObject g = v.ExposedReference.Resolve(graph.GetResolver());
                    clone.addFadeObject(g);

#if CLIENT_BUILD
                    if(TimelineController.instance != null)
                    {
                        v.ExposedReference.exposedName = System.Guid.NewGuid().ToString();
                        TimelineController.instance.getPlayableDirector().SetReferenceValue(v.ExposedReference.exposedName, g);
                    }
#endif
                }
            }

            return playable;
        }

        [Serializable]
        public class ExposedGameObject : ExposedReferenceHolder<GameObject>
        {
        }
    }

    [Serializable]
    public class ExposedReferenceHolder<T> where T : UnityEngine.Object
    {
        public ExposedReference<T> ExposedReference;
    }
}
