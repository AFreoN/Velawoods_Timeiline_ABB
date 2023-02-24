using UnityEngine;
using UnityEditor;
using UnityEngine.Timeline;
using UnityEditor.Timeline;
using System.Collections.Generic;
using CustomExporter;
using CustomExtensions;

public class TimelineClipGenerator : EditorWindow
{
    TimelineClip clipType;
    TrackType type = TrackType.Animation;

    #region Style
    static GUIStyle bgStyle;
    static Texture2D texture;
    static GUIStyle boldStyle;
    static GUIStyle errorStyle;
    #endregion

    #region Animation track
    public Animator anim;
    List<AnimClipData> animClipDatas = new List<AnimClipData>();
    #endregion

    Transform transform;
    #region Warp Track
    List<WarpClipData> warpClipDatas = new List<WarpClipData>();
    #endregion

    #region 
    FaceLookAt faceLookAt;
    static List<LookAtClipData> lookAtClipDatas = new List<LookAtClipData>();
    #endregion

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

        if(errorStyle == null)
        {
            errorStyle = new GUIStyle();
            errorStyle.normal.textColor = Color.red;
        }

        if (animClipDatas.Count == 0)
        {
            animClipDatas.Add(new AnimClipData());
        }

        if (warpClipDatas.Count == 0)
            warpClipDatas.Add(new WarpClipData());

        if (lookAtClipDatas.Count == 0)
            lookAtClipDatas.Add(new LookAtClipData());
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

            case TrackType.Warp:
                showWarpTrackGenerator();
                break;

            case TrackType.LookAt:
                showLookAtTrackGenerator();
                break;
        }
    }

    void OnAnimationTrackLoadClick()
    {
        if (!EditorPrefs.HasKey(DialogManager.KEY_PATH))
        {
            Debug.Log("No Path Found to load Animation Track");
            return;
        }

        Undo.RecordObject(this, "Loaded animation track in timeline clip generator");
        animClipDatas.Clear();
        ListAnimClipData loadedData = JsonImporter.LoadAnimationTrack(EditorPrefs.GetString(DialogManager.KEY_PATH));

        GameObject targetObject = GameObject.Find(loadedData.targetObject);
        if (targetObject == null)
        {
            Debug.LogError("No gameobject found with name : " + loadedData.targetObject);
            return;
        }

        anim = targetObject.GetComponent<Animator>();
        if(anim == null)
        {
            Debug.LogError("Object founded with name " + targetObject.name + " doesn't have animator component attached to it");
            return;
        }
        for (int i = 0; i < loadedData.stateNames.Count; i++)
        {
            AnimClipData cData = new AnimClipData();

            cData.clip = targetObject.GetComponent<Animator>().getClipFromStateName(loadedData.stateNames[i], loadedData.layers[i]);
            cData.stateName = loadedData.stateNames[i];
            cData.startTime = loadedData.startTimes[i];
            cData.duration = loadedData.durations[i];
            animClipDatas.Add(cData);
        }
    }

    void showAnimationTrackGenerator()
    {
        EditorGUILayout.Space(10);

        EditorGUILayout.BeginHorizontal();
        anim = (Animator) EditorGUILayout.ObjectField("Animator", anim, typeof(Animator), true);
        //GUILayout.FlexibleSpace();
        if(GUILayout.Button("Load Animation Track"))
        {
            OnAnimationTrackLoadClick();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(10);

        EditorGUILayout.LabelField("AnimationClip | StartTime | Duration", boldStyle);
        GUILayout.Space(5);

        List<AnimClipData> removableData = new List<AnimClipData>();

        EditorGUILayout.BeginVertical(bgStyle);
        foreach(AnimClipData cData in animClipDatas)
        {
            EditorGUILayout.BeginHorizontal();
            cData.clip = (AnimationClip)EditorGUILayout.ObjectField(cData.clip, typeof(AnimationClip), false);
            if (cData.clip == null)
                EditorGUILayout.LabelField(cData.stateName, errorStyle);
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
            if(animClipDatas.Contains(cData))
                animClipDatas.Remove(cData);
        }

        GUILayout.Space(5);
        if (GUILayout.Button("Add"))
            animClipDatas.Add(new AnimClipData());

        GUILayout.Space(10);

        TimelineAsset asset = TimelineEditor.inspectedAsset;
        if(anim != null && animClipDatas.Count > 0 && asset != null && GUILayout.Button("Generate Clips"))
        {
            string trackName = anim.name + "_anim";

            foreach(TrackAsset track in asset.GetOutputTracks())
            {
                if(track.name == trackName)
                {
                    Debug.Log("Track with name " + trackName + " already exists");
                    //return;
                }
            }
            AnimationTrack newTrack = asset.CreateTrack<AnimationTrack>(trackName);
            UndoExtensions.RegisterTrack(newTrack, "Animation track created");

            foreach(AnimClipData cd in animClipDatas)
            {
                TimelineClip clip = newTrack.CreateClip(cd.clip);
                clip.start = cd.startTime;
                clip.duration = cd.duration;
            }

            TimelineEditor.inspectedDirector.SetGenericBinding(newTrack, anim);

            TimelineEditor.Refresh(RefreshReason.ContentsAddedOrRemoved);
        }
    }

    void showWarpTrackGenerator()
    {
        EditorGUILayout.Space(10);

        transform = (Transform)EditorGUILayout.ObjectField("Target Transform", transform, typeof(Transform), true);

        EditorGUILayout.Space(10);

        EditorGUILayout.LabelField("Warp To | StartTime | Duration", boldStyle);
        GUILayout.Space(5);

        List<WarpClipData> removableData = new List<WarpClipData>();

        EditorGUILayout.BeginVertical(bgStyle);
        foreach (WarpClipData cData in warpClipDatas)
        {
            EditorGUILayout.BeginHorizontal();
            cData.warpTo = (Transform)EditorGUILayout.ObjectField(cData.warpTo, typeof(Transform), true);
            GUILayout.FlexibleSpace();
            cData.useObjectRotation = EditorGUILayout.Toggle("Use Object Rotation", cData.useObjectRotation);
            GUILayout.FlexibleSpace();
            cData.startTime = EditorGUILayout.DoubleField(cData.startTime);
            GUILayout.FlexibleSpace();
            //cData.duration = EditorGUILayout.DoubleField(cData.duration);

            if (GUILayout.Button("Remove"))
            {
                removableData.Add(cData);
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();

        foreach (WarpClipData cData in removableData)
        {
            if (warpClipDatas.Contains(cData))
                warpClipDatas.Remove(cData);
        }

        GUILayout.Space(5);
        if (GUILayout.Button("Add"))
            warpClipDatas.Add(new WarpClipData());

        GUILayout.Space(10);

        TimelineAsset asset = TimelineEditor.inspectedAsset;
        if (transform != null && warpClipDatas.Count > 0 && asset != null && GUILayout.Button("Generate Clips"))
        {
            string trackName = transform.name + "_Warp";

            WarpTrack newTrack = asset.CreateTrack<WarpTrack>(trackName);
            UndoExtensions.RegisterTrack(newTrack, "Warp track created");

            for(int i = 0; i < warpClipDatas.Count; i++)
            {
                WarpClipData cd = warpClipDatas[i];

                TimelineClip tClip = newTrack.CreateClip<WarpClip>();
                WarpClip wClip = tClip.asset as WarpClip;
                tClip.start = cd.startTime;
                if (i + 1 < warpClipDatas.Count)
                    tClip.duration = warpClipDatas[i + 1].startTime - cd.startTime;
                else
                    tClip.duration = 5f;

                wClip.objectToWarpTo.exposedName = GUID.Generate().ToString();
                TimelineEditor.inspectedDirector.SetReferenceValue(wClip.objectToWarpTo.exposedName, cd.warpTo);
                wClip.behaviour.useObjectRotation = cd.useObjectRotation;
            }
/*            foreach (WarpClipData cd in warpClipDatas)
            {
                TimelineClip tClip = newTrack.CreateClip<WarpClip>();
                WarpClip wClip = tClip.asset as WarpClip;
                wClip.objectToWarpTo.exposedName = GUID.Generate().ToString();
                TimelineEditor.inspectedDirector.SetReferenceValue(wClip.objectToWarpTo.exposedName, cd.warpTo);
                tClip.start = cd.startTime;
                wClip.behaviour.useObjectRotation = cd.useObjectRotation;
            }*/

            TimelineEditor.inspectedDirector.SetGenericBinding(newTrack, transform);

            TimelineEditor.Refresh(RefreshReason.ContentsAddedOrRemoved);
        }
    }

    void showLookAtTrackGenerator()
    {
        EditorGUILayout.Space(10);

        faceLookAt = (FaceLookAt)EditorGUILayout.ObjectField("Face Look At", faceLookAt, typeof(FaceLookAt), true);

        EditorGUILayout.Space(10);

        EditorGUILayout.LabelField("Target to Look | Look Type | StartTime | EndTime", boldStyle);
        GUILayout.Space(5);

        List<LookAtClipData> removableData = new List<LookAtClipData>();

        EditorGUILayout.BeginVertical(bgStyle);
        foreach (LookAtClipData cData in lookAtClipDatas)
        {
            EditorGUILayout.BeginHorizontal();
            cData.targetToLook = (Transform)EditorGUILayout.ObjectField(cData.targetToLook, typeof(Transform), true);
            GUILayout.FlexibleSpace();
            cData.lookType = (LookType)EditorGUILayout.EnumPopup("Look Type", cData.lookType);
            GUILayout.FlexibleSpace();
            cData.startTime = EditorGUILayout.DoubleField(cData.startTime);
            //GUILayout.FlexibleSpace();
            cData.endTime = EditorGUILayout.DoubleField(cData.endTime);

            if (GUILayout.Button("Remove"))
            {
                removableData.Add(cData);
            }
            EditorGUILayout.EndHorizontal();
            if (cData.targetToLook == null)
                EditorGUILayout.LabelField("        No Transform", errorStyle);
            else if (cData.targetToLook.GetComponent<FaceLookAt>() == null && cData.lookType == LookType.Face)
                EditorGUILayout.LabelField("        Is this character?", errorStyle);
        }
        EditorGUILayout.EndVertical();

        foreach (LookAtClipData cData in removableData)
        {
            if (lookAtClipDatas.Contains(cData))
                lookAtClipDatas.Remove(cData);
        }

        GUILayout.Space(5);
        if (GUILayout.Button("Add"))
        {
            LookAtClipData cd = new LookAtClipData();
            if(lookAtClipDatas.Count > 0)
            {
                cd.startTime = lookAtClipDatas[lookAtClipDatas.Count - 1].endTime;
            }
            lookAtClipDatas.Add(cd);
        }

        GUILayout.Space(10);

        TimelineAsset asset = TimelineEditor.inspectedAsset;
        if (faceLookAt != null && lookAtClipDatas.Count > 0 && asset != null && GUILayout.Button("Generate Clips"))
        {
            string trackName = faceLookAt.name + "_LookAt";

            LookAtTrack newTrack = asset.CreateTrack<LookAtTrack>(trackName);
            UndoExtensions.RegisterTrack(newTrack, "LookAt track created");

            for (int i = 0; i < lookAtClipDatas.Count; i++)
            {
                LookAtClipData cd = lookAtClipDatas[i];

                TimelineClip tClip = newTrack.CreateClip<LookAtAsset>();
                LookAtAsset lookAtClip = tClip.asset as LookAtAsset;
                tClip.start = cd.startTime;
                tClip.duration = cd.endTime - cd.startTime;
                tClip.displayName = cd.targetToLook != null ? cd.targetToLook.name : "NULL";

                lookAtClip.target.exposedName = GUID.Generate().ToString();
                TimelineEditor.inspectedDirector.SetReferenceValue(lookAtClip.target.exposedName, cd.targetToLook);
                lookAtClip.type = cd.lookType;
            }

            TimelineEditor.inspectedDirector.SetGenericBinding(newTrack, faceLookAt);

            TimelineEditor.Refresh(RefreshReason.ContentsAddedOrRemoved);
        }
    }

    enum TrackType
    {
        Animation,
        Warp,
        LookAt
    }

    class AnimClipData
    {
        public AnimationClip clip;
        public double startTime;
        public double duration;
        public string stateName;
    }

    class WarpClipData
    {
        public Transform warpTo;
        public double startTime;
        public bool useObjectRotation;
        //public double duration;
    }

    class LookAtClipData
    {
        public Transform targetToLook;
        public LookType lookType;
        public double startTime;
        public double endTime;
    }
}
