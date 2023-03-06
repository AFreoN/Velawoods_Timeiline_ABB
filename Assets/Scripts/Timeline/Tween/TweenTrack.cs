using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using System.Collections.Generic;

namespace CustomTracks
{
    [TrackClipType(typeof(TweenAsset))]
    [TrackBindingType(typeof(Transform))]
    [TrackColor(0, 0, 1)]
    public class TweenTrack : TrackAsset
    {
        static PlayableDirector director;

        static Dictionary<Transform, List<TweenBehaviour>> allClips = new Dictionary<Transform, List<TweenBehaviour>>();

        public const float IGNORE_ROTATION_VALUE = -9999;

        public static bool IsEmptyFrame(Transform _t)
        {
            List<TweenBehaviour> tbs = allClips[_t];
            foreach (TweenBehaviour tb in tbs)
            {
                bool result = director.time <= tb.startTime || director.time > tb.endTime;
                if (!result)
                    return result;
            }
            return true;
        }

        public static TweenBehaviour getLastClip(Transform _t)
        {
            List<TweenBehaviour> tbs = allClips[_t];

            TweenBehaviour result = null;
            double closeTime = double.MaxValue;

            for (int i = 0; i < tbs.Count; i++)
            {
                double diff = director.time - tbs[i].endTime;
                if (diff < 0)
                    continue;
                else if (diff < closeTime)
                {
                    closeTime = diff;
                    result = tbs[i];
                }
            }

            return result;
        }

        private void OnValidate()
        {
            allClips = new Dictionary<Transform, List<TweenBehaviour>>();
        }

        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject gameObject, int inputCount)
        {
            director = gameObject.GetComponent<PlayableDirector>();
            Transform t = (Transform)gameObject.GetComponent<PlayableDirector>().GetGenericBinding(this);

            //Get all clips in this track and set it's clips targetTransform to the one attached in this track
            foreach (var c in GetClips())
            {
                ((TweenAsset)(c.asset)).targetTransform = t;
                ((TweenAsset)(c.asset)).behaviour.t = t;
            }

            TimelineAsset asset = (TimelineAsset)gameObject.GetComponent<PlayableDirector>().playableAsset;
            List<TweenBehaviour> behaviours = new List<TweenBehaviour>();

            //Get all clips in this track
            foreach (TimelineClip clip in this.GetClips())
            {
                double startTime = clip.start;
                double endTime = clip.end;

                //Set start time, end time and playable director for timely interpolation
                TweenAsset ta = (TweenAsset)clip.asset;
                ta.behaviour.startTime = startTime;
                ta.behaviour.endTime = endTime;
                ta.behaviour.director = director;
                behaviours.Add(ta.behaviour);

            }

            if (!allClips.ContainsKey(t))
                allClips.Add(t, behaviours);


            //Set start & end position, rotation based on it's clip translation type. Some Translation type requires auto value and it's done here
            if (behaviours.Count > 0)
            {
                if (behaviours[0].translateType == TweenBehaviour.TranslateType.HoldNewPosition)
                {
                    behaviours[0].endPosition = behaviours[0].startPosition;
                    behaviours[0].endRotation = behaviours[0].startRotation;
                }
            }

            for (int i = 1; i < behaviours.Count; i++)
            {
                if (behaviours[i].translateType == TweenBehaviour.TranslateType.FromPreviousClip)
                {
                    behaviours[i].startPosition = behaviours[i - 1].endPosition;
                    //behaviours[i].startRotation = behaviours[i - 1].endRotation;
                    behaviours[i].startRotation = behaviours[i - 1].getEndRotation().eulerAngles;
                }
                else if (behaviours[i].translateType == TweenBehaviour.TranslateType.HoldNewPosition)
                {
                    behaviours[i].endPosition = behaviours[i].startPosition;
                    behaviours[i].endRotation = behaviours[i].startRotation;
                }
                else if (behaviours[i].translateType == TweenBehaviour.TranslateType.Hold)
                {
                    behaviours[i].startPosition = behaviours[i - 1].endPosition;
                    behaviours[i].startRotation = behaviours[i - 1].getEndRotation().eulerAngles + Vector3.down * behaviours[i - 1].rotationOffset;

                    behaviours[i].endPosition = behaviours[i].startPosition;
                    behaviours[i].endRotation = behaviours[i].startRotation;
                }

                if (behaviours[i].rotationOffset == 0)
                    behaviours[i].rotationOffset = behaviours[i - 1].rotationOffset;
            }

            return ScriptPlayable<TweenBehaviour>.Create(graph, inputCount);
        }
    }
}
