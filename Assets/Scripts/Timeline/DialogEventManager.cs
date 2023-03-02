using UnityEngine;
using TMPro;
using CustomExtensions;
using System;

public class DialogEventManager : TimelineBehaviour
{
    //public static DialogEventManager instance { get; private set; }

    //public void OnEnable()
    //{
    //    instance = this;
    //}
    AudioSource audioSource = null;

    [SerializeField] GameObject subHolder = null;   //Gameobject holding the subtitle text
    [SerializeField] TMP_Text subtitleText = null;  //Subtitle display text
    
    [Header("For Tutorial")]
    [SerializeField] Transform tutorialContentHolder = null;    //Gameobjects that holds all the tutorial text
    [SerializeField] Transform tutorialSubPrefab = null;    //Prefab to instantiate when new text has to be shown

    DialogOpener dialogOpener = null;

    private void Start()
    {
        subHolder.SetActive(true);

        dialogOpener = GetComponent<DialogOpener>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    /// <summary>
    /// Called when the DialogTrack clip starts in the timeline
    /// </summary>
    /// <param name="o">DialogBehaviour object</param>
    public override void OnClipStart(object o)
    {
        o.executeAction((DialogBehaviour db) => processDialog(db.character, db.animationClipName, db.subtitle, db.audioClip,db.isLearner, db.isTutorial));
    }

    /// <summary>
    /// Called when the DialogTrack clip ends in the timeline
    /// </summary>
    /// <param name="o">DialogBehaviour object</param>
    public override void OnClipEnd(object o)
    {
        disableSubtitle();
    }

    /// <summary>
    /// Display subtitle in the UI, play character face animation and audio
    /// </summary>
    /// <param name="character">Character gameobjects responsible for this conversation</param>
    /// <param name="animationClipName">Name of the animation to play</param>
    /// <param name="subtitle">Write subtitles</param>
    /// <param name="audioClip">Audio clip to play</param>
    /// <param name="isTutorial">Is tutorial coversation or normal conversation</param>
    public void processDialog(GameObject character, string animationClipName, string subtitle, AudioClip audioClip,bool isLearner, bool isTutorial)
    {
        if (Application.isPlaying == false) return;

        if (!isTutorial)
        {
            subHolder.GetComponent<DialogArrow>().changeTarget(character);
            writeSubtitle(subtitle);
        }
        else
            writeTutorialSubtititle(subtitle);

        if (!isLearner && character != null && audioClip != null)
            character.GetComponent<FaceAnim>().playAnimAudio(audioClip, animationClipName);
        else if(character == null && audioClip != null)
        {
            audioSource.clip = audioClip;
            audioSource.Play();
        }
    }

    public void writeSubtitle(string s)
    {
        subtitleText.text = s;
        //subHolder.SetActive(true);
        dialogOpener.OpenDialog();
    }

    public void disableSubtitle()
    {
        dialogOpener.CloseDialog();
        //subHolder.SetActive(false);
    } 

    void writeTutorialSubtititle(string subtitle)
    {
        Transform t = Instantiate(tutorialSubPrefab);
        t.SetParent(tutorialContentHolder);
        t.localScale = Vector3.one;

        t.Find("Subtitle_Text").GetComponent<TMP_Text>().text = subtitle;
    }
}
