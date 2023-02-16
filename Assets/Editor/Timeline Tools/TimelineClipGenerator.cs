using UnityEngine;
using UnityEditor;
using UnityEngine.Timeline;
using UnityEditor.Timeline;
using System.Collections.Generic;

public class TimelineClipGenerator : EditorWindow
{
    TimelineClip clipType;
    TrackType type = TrackType.Animation;

    static GUIStyle bgStyle;
    static Texture2D texture;
    GUIStyle boldStyle;

    public Animator anim;
    public List<AnimClipData> clipDatas = new List<AnimClipData>();

    static ScriptableObject target;
    static SerializedObject so;

    [MenuItem("Tools/Timeline Clip Generator")]
    static void ShowWindow()
    {
        GetWindow<TimelineClipGenerator>();
    }

    private void OnEnable()
    {
        if (bgStyle == null)
        {
            bgStyle = new GUIStyle();
            if (texture == null)
            {
                texture = new Texture2D(1, 1);
                texture.SetPixel(0, 0, new Color(0.3f, 0.3f, 0.3f));
                texture.Apply();
            }
            bgStyle.normal.background = texture;
        }

        if(boldStyle == null)
        {
            boldStyle = new GUIStyle();
            boldStyle.fontStyle = FontStyle.Bold;
            boldStyle.onNormal.textColor = Color.white;
            boldStyle.normal.textColor = Color.white;
        }

        if (clipDatas.Count == 0)
        {
            clipDatas.Add(new AnimClipData());
        }
    }

    private void OnDisable()
    {
        if (texture != null)
        {
            DestroyImmediate(texture);
        }
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Generate Clips");

        type = (TrackType)EditorGUILayout.EnumPopup("Track Type", type);

        switch (type)
        {
            case TrackType.Animation:
                showAnimationTrackGenerator();
                break;
        }
    }

    void showAnimationTrackGenerator()
    {
        EditorGUILayout.Space(10);

        anim = (Animator) EditorGUILayout.ObjectField("Animator", anim, typeof(Animator), true);

        EditorGUILayout.Space(10);

        EditorGUILayout.LabelField("AnimationClip | StartTime | Duration", boldStyle);
        GUILayout.Space(5);

        List<AnimClipData> removableData = new List<AnimClipData>();

        EditorGUILayout.BeginVertical(bgStyle);
        foreach(AnimClipData cData in clipDatas)
        {
            EditorGUILayout.BeginHorizontal();
            cData.clip = (AnimationClip)EditorGUILayout.ObjectField(cData.clip, typeof(AnimationClip), false);
            GUILayout.FlexibleSpace();
            cData.startTime = EditorGUILayout.DoubleField(cData.startTime);
            cData.duration = EditorGUILayout.DoubleField(cData.duration);

            if (GUILayout.Button("Remove"))
            {
                removableData.Add(cData);
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();

        foreach(AnimClipData cData in removableData)
        {
            if(clipDatas.Contains(cData))
                clipDatas.Remove(cData);
        }

        GUILayout.Space(5);
        if (GUILayout.Button("Add"))
            clipDatas.Add(new AnimClipData());

        GUILayout.Space(10);

        TimelineAsset asset = TimelineEditor.inspectedAsset;
        if(anim != null && clipDatas.Count > 0 && asset != null && GUILayout.Button("Generate Clips"))
        {
            string trackName = anim.name + "_anim";

            foreach(TrackAsset track in asset.GetOutputTracks())
            {
                if(track.name == trackName)
                {
                    Debug.Log("Track with name " + trackName + " already exists");
                    return;
                }
            }
            AnimationTrack newTrack = asset.CreateTrack<AnimationTrack>(trackName);
            UndoExtensions.RegisterTrack(newTrack, "Animation track created");

            foreach(AnimClipData cd in clipDatas)
            {
                TimelineClip clip = newTrack.CreateClip(cd.clip);
                clip.start = cd.startTime;
                clip.duration = cd.duration;
            }

            TimelineEditor.inspectedDirector.SetGenericBinding(newTrack, anim);

            TimelineEditor.Refresh(RefreshReason.ContentsAddedOrRemoved);
        }
    }

    enum TrackType
    {
        Animation
    }

    public class AnimClipData
    {
        public AnimationClip clip;
        public double startTime;
        public double duration;
    }
}
