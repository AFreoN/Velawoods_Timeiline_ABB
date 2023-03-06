using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;

namespace CustomTracks
{
    [TrackColor(1, 0, 0)]
    [TrackClipType(typeof(CharacterSwitchAsset))]
    [TrackBindingType(typeof(CharacterSwitch))]
    public class CharacterSwitchTrack : TrackAsset
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            CharacterSwitch g = (CharacterSwitch)go.GetComponent<PlayableDirector>().GetGenericBinding(this);

            //Get all clips in this track and set it's clips ogCharacter value to the one attached in this track
            foreach (var c in GetClips())
            {
                ((CharacterSwitchAsset)(c.asset)).ogCharacter = g;
            }

            return ScriptPlayable<CharacterSwitchBehaviour>.Create(graph, inputCount);
        }
    }
}
