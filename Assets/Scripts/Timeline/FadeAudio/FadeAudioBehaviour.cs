using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using CustomExtensions;

[Serializable]
public class FadeAudioBehaviour : PlayableBehaviour
{
    [HideInInspector]
    public AudioSource audioSource;     //Audio source binded in this track

    public AnimationCurve fadeCurve;    //Curve used to evaluate audio source volume

    [HideInInspector]
    public double startTime, endTime;
    [HideInInspector]
    public PlayableDirector director;
    bool IsInTime => director.time >= startTime && director.time <= endTime;

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        #region UNITY_EDITOR
        if (Application.isPlaying == false) return;
        #endregion

        if (startTime == 0 && endTime == 0) return;

        if (audioSource == null) return;

        string s = audioSource.clip != null ? audioSource.clip.name : "NO CLIP :(";
        //Debug.Log("Started the clip : " + s);
    }

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        #region UNITY_EDITOR
        if (Application.isPlaying == false) return;
        #endregion

        if (!director || !IsInTime) return;

        //Interpolate this clip playtime to evaluate the animation curve, use it's value to set volume of the audio source
        float i = (float)Extensions.InverseLerp(startTime, endTime, director.time);
        Fade(fadeCurve.Evaluate(i));
    }

    void Fade(float _volume)
    {
        if (!audioSource)
        {
            Debug.LogError("No Audio Source!");
            return;
        }

        //_volume = Mathf.Clamp(0.0f, 1.0f, _volume);
        _volume = Mathf.Min(Mathf.Max(0.0f, _volume), 1.0f);
        audioSource.volume = _volume;
        //Debug.Log("Volume now : " + _volume);
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        #region UNITY_EDITOR
        if (Application.isPlaying == false) return;
        #endregion

        //if(playable.isPlayableCompleted(info) && audioSource)
        //{
        //    Debug.Log("Fading audio completed");
        //}
    }
}
