using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;

[TrackColor(1,0,0)]
[TrackClipType(typeof(CharacterSwitchAsset))]
[TrackBindingType(typeof(CharacterSwitch))]
public class CharacterSwitchTrack : TrackAsset
{
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        CharacterSwitch g = (CharacterSwitch)go.GetComponent<PlayableDirector>().GetGenericBinding(this);

        foreach (var c in GetClips())
        {
            ((CharacterSwitchAsset)(c.asset)).ogCharacter = g;
        }

        return ScriptPlayable<CharacterSwitchBehaviour>.Create(graph, inputCount);
    }
}
