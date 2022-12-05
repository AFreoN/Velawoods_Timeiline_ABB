using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using System;

[CreateAssetMenu(fileName = "TimelineData", menuName ="Scriptables/TimelineData")]
public class TimelineData : ScriptableObject
{
    public PlayableDirector playableDirector => getPlayableDirector();   //PlayableDirector responsible for the playing current timeline
    public TimelineAsset timelineAsset;
    public RuntimeAnimatorController resetController;

    Func<PlayableDirector> director;

    public void setData(Func<PlayableDirector> _func, PlayableAsset _pa)
    {
        director = _func;
        timelineAsset = (TimelineAsset)_pa;
    }

    PlayableDirector getPlayableDirector()
    {
        return director?.Invoke();
    }
}
