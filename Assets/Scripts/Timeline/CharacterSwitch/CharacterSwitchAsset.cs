using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;

[TrackClipType(typeof(CharacterSwitchBehaviour))]
public class CharacterSwitchAsset : PlayableAsset
{
    [SerializeField][HideInInspector] public CharacterSwitch ogCharacter;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        CharacterSwitchBehaviour behaviour = new CharacterSwitchBehaviour();
        behaviour.setProperties(ogCharacter);

        var playable = ScriptPlayable<CharacterSwitchBehaviour>.Create(graph, behaviour);
        return playable;
    }
}
