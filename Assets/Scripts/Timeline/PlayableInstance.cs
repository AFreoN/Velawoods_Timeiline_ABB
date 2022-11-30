using UnityEngine;
using UnityEngine.Playables;

public class PlayableInstance : MonoBehaviour
{
    [SerializeField] ActivePlayable activePlayable;

    public static ActivePlayable playable;

    private void Awake()
    {
        playable = activePlayable;
        playable.clear();
    }

    public static void AddPlayable(TimelineBehaviour behaviour) => playable.Add(behaviour);

    public static void RemovePlayable(TimelineBehaviour behaviour) => playable.Remove(behaviour);

    public static void Skip(float duration) => playable.Skip(duration);
}
