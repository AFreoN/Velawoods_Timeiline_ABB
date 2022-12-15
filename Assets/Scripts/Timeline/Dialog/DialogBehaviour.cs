using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using CustomExtensions;

[System.Serializable]
public class DialogBehaviour : PlayableBehaviour
{
    //[ShowInInspector, CustomValueDrawer("MyCustomHeader"),HideLabel, MultiLineProperty,PropertySpace(20, 20)]
    //string ss => subtitle;

    //[HideInInspector] public DialogEventManager dialogManager = null;

    //[SerializeField, Required("Character required"), TitleGroup("For Character", alignment: TitleAlignments.Centered)]
    //public ExposedReference<GameObject> character;
    ////[HideInInspector]
    //public GameObject characterGo;
    //[SerializeField, PropertySpace(0, 10)]
    //public string animationClipName;

    //[TitleGroup("For Subtitle", alignment: TitleAlignments.Centered), Required("AudioClip required")]
    //public AudioClip audio;
    //[Multiline] public string subtitle;

    public DialogEventManager dialogManager;
    public GameObject character;
    public string animationClipName;
    public AudioClip audioClip;
    public string subtitle;
    public bool isTutorial;

    public void setProperties(DialogEventManager _dialogManager, GameObject _character, string _animationClipName, AudioClip _audioClip, string _subtitle, bool _isTutorial)
    {
        dialogManager = _dialogManager;
        character = _character;
        animationClipName = _animationClipName;
        audioClip = _audioClip;
        subtitle = _subtitle;
        isTutorial = _isTutorial;
    }

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        //base.OnBehaviourPlay(playable, info);
        //Debug.Log("Playing behaviour : " + dialogManager.name  + " : " + character.name);

        if (dialogManager != null)
            dialogManager.OnClipStart(this);
            //dialogManager.processDialog(character, animationClipName, subtitle, audioClip, (float)playable.GetDuration(), isTutorial);
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        //Debug.Log("Playable time : " + playable.GetTime());
        //Debug.Log("Playable duration : " + playable.GetDuration());

        //var duration = playable.GetDuration();
        //var time = playable.GetTime();
        //var count = time + info.deltaTime;

        //if((info.effectivePlayState == PlayState.Paused && count > duration) || Mathf.Approximately((float)time, (float)duration))
        //{
        //    //Debug.Log("Clip ended : " + subtitle);
        //    dialogManager.disableSubtitle();
        //}

        if (playable.isPlayableCompleted(info))
            dialogManager.OnClipEnd(this);
    }

    [ContextMenu("Set animation name")]
    public void copyAudioName()
    {
        if (audioClip == null) return;
        animationClipName = "Default_" + audioClip.name;
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
