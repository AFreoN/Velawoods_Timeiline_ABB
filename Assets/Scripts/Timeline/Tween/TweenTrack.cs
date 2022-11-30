using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using System.Collections.Generic;

[TrackClipType(typeof(TweenAsset))]
[TrackBindingType(typeof(Transform))]
[TrackColor(0,0,1)]
public class TweenTrack : TrackAsset
{
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject gameObject, int inputCount)
    {
        PlayableDirector pd = gameObject.GetComponent<PlayableDirector>();
        Transform t = (Transform)gameObject.GetComponent<PlayableDirector>().GetGenericBinding(this);

        foreach (var c in GetClips())
        {
            ((TweenAsset)(c.asset)).targetTransform = t;
            ((TweenAsset)(c.asset)).track = this;
            ((TweenAsset)(c.asset)).behaviour.t = t;
        }

        TimelineAsset asset = (TimelineAsset)gameObject.GetComponent<PlayableDirector>().playableAsset;
        List<TweenBehaviour> behaviours = new List<TweenBehaviour>();

        foreach(TimelineClip clip in this.GetClips())
        {
            double startTime = clip.start;
            double endTime = clip.end;

            TweenAsset ta = (TweenAsset)clip.asset;
            ta.behaviour.startTime = startTime;
            ta.behaviour.endTime = endTime;
            ta.behaviour.director = pd;
            behaviours.Add(ta.behaviour);
        }
        //foreach (var v in asset.GetOutputTracks())
        //{
        //    if (v == this)
        //    {
        //        foreach (TimelineClip clip in v.GetClips())
        //        {

        //        }
        //    }
        //}

        for(int i = 1; i < behaviours.Count; i++)
        {
            if(behaviours[i].translateType == TweenBehaviour.TranslateType.FromPreviousClip)
            {
                behaviours[i].startPosition = behaviours[i - 1].endPosition;
                behaviours[i].startRotation = behaviours[i - 1].endRotation;
            }
            else if(behaviours[i].translateType == TweenBehaviour.TranslateType.HoldNewPosition)
            {
                behaviours[i].endPosition = behaviours[i].startPosition;
                behaviours[i].endRotation = behaviours[i].startRotation;
            }
        }

        return ScriptPlayable<TweenBehaviour>.Create(graph, inputCount);
    }
}
