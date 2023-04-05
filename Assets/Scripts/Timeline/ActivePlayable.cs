using UnityEngine;
using System.Collections.Generic;
using CustomExtensions;

[CreateAssetMenu(menuName = "CurrentPlayables", fileName = "Current Playable")]
public class ActivePlayable : ScriptableObject
{
    public List<ITimelineBehaviour> playables = new List<ITimelineBehaviour>();

    //Add new ITimelineBehaviour in the playables list
    public void Add(ITimelineBehaviour behaviour)
    {
        if (!playables.Contains(behaviour))
        {
            playables.Add(behaviour);
        }
    }

    //Remove ITimelineBehaviour in the playables list
    public void Remove(ITimelineBehaviour behaviour)
    {
        if (playables.Contains(behaviour))
        {
            playables.Remove(behaviour);
        }
    }

    //Call OnSkip() on ITimelineBehaviours in the playable list if playable director time is outside of this ITimelineBehaviours play times
    public void Skip(float duration)
    {
        if (playables.Count == 0) return;

        List<ITimelineBehaviour> currentPlayable = playables.Clone();

        for (int i = 0; i < playables.Count; i++)
        {
            if (isSkippable(duration, (float)playables[i].startTime, (float)playables[i].endTime))
            {
                playables[i].OnSkip();
                currentPlayable.Remove(playables[i]);
                //Debug.Log("Skipped playable");
            }
            //else
            //    Debug.Log("Unskippable on : " + playables[i].ToString());
        }

        playables = currentPlayable;
    }

    public void Reset()
    {
        if (playables.Count == 0) return;

        for (int i = 0; i < playables.Count; i++)
        {
            playables[i].OnReset();
        }

        playables.Clear();
    }

    public void clear() => playables.Clear();

    bool isSkippable(float duration, float start, float end)
    {
        return duration < start || duration >= end;
    }
}
