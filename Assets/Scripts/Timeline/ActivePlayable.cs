using UnityEngine;
using System.Collections.Generic;
using CustomExtensions;

[CreateAssetMenu(menuName = "CurrentPlayables", fileName = "Current Playable")]
public class ActivePlayable : ScriptableObject
{
    public List<TimelineBehaviour> playables = new List<TimelineBehaviour>();

    public void Add(TimelineBehaviour behaviour)
    {
        if (!playables.Contains(behaviour))
            playables.Add(behaviour);
    }

    public void Remove(TimelineBehaviour behaviour)
    {
        if (playables.Contains(behaviour))
            playables.Remove(behaviour);
    }

    public void Skip(float duration)
    {
        if (playables.Count == 0) return;

        List<TimelineBehaviour> currentPlayable = playables.clone();

        for (int i = 0; i < playables.Count; i++)
        {
            if (isSkippable(duration, playables[i].startTime, playables[i].endTime))
            {
                playables[i].OnSkip();
                currentPlayable.Remove(playables[i]);
            }
            else
                Debug.Log("Unskippable on : " + playables[i].gameObject.name);
        }

        playables = currentPlayable;
    }

    public void clear() => playables.Clear();

    bool isSkippable(float duration, float start, float end)
    {
        return duration < start || duration >= end;
    }
}
