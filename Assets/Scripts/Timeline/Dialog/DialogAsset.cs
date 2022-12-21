using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackClipType(typeof(DialogBehaviour))]
public class DialogAsset : PlayableAsset
{
    [HideInInspector] public DialogEventManager dialogManager = null;

    [SerializeField] bool isTutorial = false;

    [SerializeField]
    public ExposedReference<GameObject> character;
    //[HideInInspector]
    GameObject characterGo;
    [SerializeField][Space(10)]
    public string animationClipName;

    [Header("For Subtitle")]
    public AudioClip audio;
    [Multiline] public string subtitle;

    [HideInInspector]
    public DialogBehaviour behaviour;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        behaviour = new DialogBehaviour();
        characterGo = character.Resolve(graph.GetResolver());
        behaviour.setProperties(dialogManager, characterGo, animationClipName, audio, subtitle, isTutorial);

        var playable = ScriptPlayable<DialogBehaviour>.Create(graph, behaviour);


        return playable;
    }

    [ContextMenu("Set animation name")]
    public void copyAudioName()
    {
        if (audio == null) return;
        animationClipName = "Default_" + audio.name;
    }

    private string MyCustomHeader(string s, GUIContent label)
    {
        GUIStyle style = new GUIStyle
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
            fontSize = 15
        };
        style.normal.background = null;
        style.normal.textColor = new Color(1, 1, 0, 1);
        //style.onNormal.textColor = Color.white;

        Color contentColor = GUI.contentColor;
        GUI.contentColor = new Color(1, 1, 1, 1);
        string result = UnityEditor.EditorGUILayout.TextField(s, style);
        GUI.contentColor = contentColor;
        return result;
    }
}
