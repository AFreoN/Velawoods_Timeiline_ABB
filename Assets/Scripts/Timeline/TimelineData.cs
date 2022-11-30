using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;

[CreateAssetMenu(fileName = "TimelineData", menuName ="Scriptables/TimelineData")]
public class TimelineData : ScriptableObject
{
    public PlayableDirector playableDirector;   //PlayableDirector responsible for the playing current timeline
    public TimelineAsset timelineAsset;
    public RuntimeAnimatorController resetController;

    public void setData(PlayableDirector _pd)
    {
        playableDirector = _pd;
    }
}
