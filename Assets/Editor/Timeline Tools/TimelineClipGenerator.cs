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

    Vector2 scrollPos;

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
    #region Warp & Tween Track
    List<WarpClipData> warpClipDatas = new List<WarpClipData>();
    static List<TweenClipData> tweenClipDatas = new List<TweenClipData>();
    #endregion

    #region Dialog track
    DialogEventManager dialogEventManager;
    List<DialogClipData> dialogClipDatas = new List<DialogClipData>();
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

        if (tweenClipDatas.Count == 0)
            tweenClipDatas.Add(new TweenClipData());

        if (dialogClipDatas.Count == 0)
            dialogClipDatas.Add(new DialogClipData());
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

            //case TrackType.Warp:
            //    showWarpTrackGenerator();
            //    break;

            case TrackType.LookAt:
                showLookAtTrackGenerator();
                break;

            case TrackType.Tween:
                showTweenTrackGenerator();
                break;

            case TrackType.Dialog:
                showDialogTrackGenerator();
                break;
        }
    }

    #region Animation Track
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

            EditorUtility.DisplayDialog("Timeline Generator", "Animation Track created for " + anim.name, "Ok");
        }
    }
    #endregion

    #region Warp Track
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
    #endregion

    #region LookAt Track
    void showLookAtTrackGenerator()
    {
        EditorGUILayout.Space(10);

        EditorGUILayout.BeginHorizontal();
        faceLookAt = (FaceLookAt)EditorGUILayout.ObjectField("Face Look At", faceLookAt, typeof(FaceLookAt), true);
        if (GUILayout.Button("Load LookAt Track"))
        {
            OnLookAtTrackLoadClick();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(10);

        EditorGUILayout.LabelField("Target to Look | Look Type | StartTime | EndTime", boldStyle);
        GUILayout.Space(5);

        List<LookAtClipData> removableData = new List<LookAtClipData>();

        //EditorGUILayout.BeginVertical(bgStyle);
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, true, GUI.skin.horizontalScrollbar, GUI.skin.verticalScrollbar, bgStyle);
        foreach (LookAtClipData cData in lookAtClipDatas)
        {
            EditorGUILayout.BeginHorizontal();
            cData.targetToLook = (Transform)EditorGUILayout.ObjectField(cData.targetToLook, typeof(Transform), true);
            if(cData.targetToLook == null)
            {
                EditorGUILayout.LabelField(cData.targetName, errorStyle);
            }
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
        EditorGUILayout.EndScrollView();
        //EditorGUILayout.EndVertical();

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

            EditorUtility.DisplayDialog("Timeline Generator", "LookAt Track created for " + faceLookAt.name, "Ok");
        }
    }

    void OnLookAtTrackLoadClick()
    {
        if (!EditorPrefs.HasKey(DialogManager.KEY_PATH))
        {
            Debug.Log("No Path Found to load Animation Track");
            return;
        }

        lookAtClipDatas.Clear();
        ListLookAtClipData clipDatas = JsonImporter.LoadLookAtTrack(EditorPrefs.GetString(DialogManager.KEY_PATH));

        if(clipDatas == null)
        {
            Debug.LogError("No File Found to load LookAtTrack");
            return;
        }

        GameObject g = GameObject.Find(clipDatas.targetObject);
        if(g == null)
        {
            Debug.LogError("Unable to find the GameObject named " + clipDatas.targetObject);
            return;
        }

        faceLookAt = g.GetComponent<FaceLookAt>();
        if(faceLookAt == null)
        {
            Debug.LogError("There is no FaceLookAt attached to the GameObject " + clipDatas.targetObject);
            return;
        }

        for(int i = 0; i < clipDatas.targetsToLook.Count; i++)
        {
            LookAtClipData clip = new LookAtClipData();

            GameObject targetToLook = GameObject.Find(clipDatas.targetsToLook[i]);
            if (targetToLook != null)
                clip.targetToLook = targetToLook.transform;
            else
                clip.targetToLook = null;

            clip.lookType = clipDatas.lookTypes[i];
            clip.startTime = clipDatas.startTimes[i];
            clip.endTime = clipDatas.durations[i] + clipDatas.startTimes[i];
            clip.targetName = clipDatas.targetsToLook[i];

            lookAtClipDatas.Add(clip);
        }
    }
    #endregion

    #region Tween Track
    void OnLoadTweenTrackClick()
    {
        if (!EditorPrefs.HasKey(DialogManager.KEY_PATH))
        {
            Debug.Log("No Path Found to load Tween Track");
            return;
        }

        tweenClipDatas.Clear();
        ListTweenClipData clipDatas = JsonImporter.LoadTweenTrack(EditorPrefs.GetString(DialogManager.KEY_PATH));

        if (clipDatas == null)
        {
            Debug.LogError("No File Found to load Tween Track");
            return;
        }

        GameObject g = GameObject.Find(clipDatas.targetObject);
        if (g == null)
        {
            Debug.LogError("Unable to find the GameObject named " + clipDatas.targetObject);
            return;
        }

        transform = g.transform;
        
        for(int i = 0; i < clipDatas.startTimes.Count; i++)
        {
            TweenClipData clip = new TweenClipData();

            clip.translateType = clipDatas.translateTypes[i];
            clip.startPosition = clipDatas.startPositions[i];
            clip.startRotation = clipDatas.startRotations[i];
            clip.startTime = clipDatas.startTimes[i];
            clip.duration = clipDatas.durations[i];

            tweenClipDatas.Add(clip);
        }
    }

    void showTweenTrackGenerator()
    {
        EditorGUILayout.Space(10);

        EditorGUILayout.BeginHorizontal();
        transform = (Transform)EditorGUILayout.ObjectField("Target Transform", transform, typeof(Transform), true);
        if (GUILayout.Button("Load Tween Track"))
        {
            OnLoadTweenTrackClick();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(10);

        EditorGUILayout.LabelField("Translate Type | Start Position | Start Rotation | StartTime | Duration", boldStyle);
        GUILayout.Space(5);

        List<TweenClipData> removableData = new List<TweenClipData>();

        EditorGUILayout.BeginVertical(bgStyle);
        foreach (TweenClipData cData in tweenClipDatas)
        {
            EditorGUILayout.BeginHorizontal();
            cData.translateType = (TweenBehaviour.TranslateType)EditorGUILayout.EnumPopup(cData.translateType);
            GUILayout.FlexibleSpace();
            cData.startPosition = EditorGUILayout.Vector3Field("StartPos", cData.startPosition);
            cData.startRotation = EditorGUILayout.Vector3Field("StartRot", cData.startRotation);
            GUILayout.FlexibleSpace();
            cData.startTime = EditorGUILayout.DoubleField("Start Time", cData.startTime);
            cData.duration = EditorGUILayout.DoubleField("Duration", cData.duration);
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Remove"))
            {
                removableData.Add(cData);
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();

        foreach (TweenClipData cData in removableData)
        {
            if (tweenClipDatas.Contains(cData))
                tweenClipDatas.Remove(cData);
        }

        GUILayout.Space(5);
        if (GUILayout.Button("Add"))
            tweenClipDatas.Add(new TweenClipData());

        GUILayout.Space(10);

        TimelineAsset asset = TimelineEditor.inspectedAsset;
        if (transform != null && tweenClipDatas.Count > 0 && asset != null && GUILayout.Button("Generate Tween Clips"))
        {
            string trackName = transform.name + "_Tween";

            TweenTrack newTrack = asset.CreateTrack<TweenTrack>(trackName);
            UndoExtensions.RegisterTrack(newTrack, "Tween track created");

            for (int i = 0; i < tweenClipDatas.Count; i++)
            {
                TweenClipData cd = tweenClipDatas[i];

                TimelineClip tClip = newTrack.CreateClip<TweenAsset>();
                TweenAsset tweenClip = tClip.asset as TweenAsset;
                tClip.start = cd.startTime;
                tClip.duration = cd.duration;

                tweenClip.behaviour.startPosition = cd.startPosition;
                tweenClip.behaviour.startRotation = cd.startRotation;
                tweenClip.behaviour.translateType = cd.translateType;
            }

            TimelineEditor.inspectedDirector.SetGenericBinding(newTrack, transform);

            TimelineEditor.Refresh(RefreshReason.ContentsAddedOrRemoved);

            EditorUtility.DisplayDialog("Timeline Generator", "Tween Track created for " + transform.name, "Ok");
        }
    }
    #endregion

    #region DialogTrack
    void showDialogTrackGenerator()
    {
        EditorGUILayout.Space(10);

        EditorGUILayout.BeginHorizontal();
        dialogEventManager = (DialogEventManager)EditorGUILayout.ObjectField("Dialog Event Manager", dialogEventManager, typeof(DialogEventManager), true);
        if (GUILayout.Button("Load Dialog Track"))
        {
            OnDialogTrackLoadClick();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(10);

        EditorGUILayout.LabelField("Character | Audio Clip | Animation Name | Subtitle| Is Learner | Is Tutorial | Start | Duration", boldStyle);
        GUILayout.Space(5);

        List<DialogClipData> removableData = new List<DialogClipData>();

        //EditorGUILayout.BeginVertical(bgStyle);
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, true, GUI.skin.horizontalScrollbar, GUI.skin.verticalScrollbar, bgStyle);
        foreach (DialogClipData cData in dialogClipDatas)
        {
            EditorGUILayout.BeginHorizontal();
            cData.character = (GameObject)EditorGUILayout.ObjectField(cData.character, typeof(GameObject), true);
            cData.audioClip = (AudioClip)EditorGUILayout.ObjectField(cData.audioClip, typeof(AudioClip), false);
            cData.animationClipName = EditorGUILayout.TextField("Animation Clip Name",cData.animationClipName);
            cData.subtitle = EditorGUILayout.TextField("Subtitle", cData.subtitle);
            GUILayout.FlexibleSpace();
            cData.isLearner = EditorGUILayout.Toggle("Is Learner", cData.isLearner);
            cData.isTutorial = EditorGUILayout.Toggle("Is Tutorial", cData.isTutorial);

            cData.startTime = EditorGUILayout.DoubleField(cData.startTime);
            cData.duration = EditorGUILayout.DoubleField(cData.duration);

            if (GUILayout.Button("Remove"))
            {
                removableData.Add(cData);
            }
            EditorGUILayout.EndHorizontal();
            if (cData.character == null && cData.isLearner == false)
                EditorGUILayout.LabelField("        Character missing!", errorStyle);
            else if (cData.character != null && cData.character.GetComponent<FaceAnim>() == null)
                EditorGUILayout.LabelField("        Is this character?", errorStyle);
        }
        EditorGUILayout.EndScrollView();
        //EditorGUILayout.EndVertical();

        foreach (DialogClipData cData in removableData)
        {
            if (dialogClipDatas.Contains(cData))
                dialogClipDatas.Remove(cData);
        }

        GUILayout.Space(5);
        if (GUILayout.Button("Add"))
        {
            DialogClipData cd = new DialogClipData();
            if (dialogClipDatas.Count > 0)
            {
                cd.startTime = dialogClipDatas[dialogClipDatas.Count - 1].duration;
            }
            dialogClipDatas.Add(cd);
        }

        GUILayout.Space(10);

        TimelineAsset asset = TimelineEditor.inspectedAsset;
        if (dialogEventManager != null && dialogClipDatas.Count > 0 && asset != null && GUILayout.Button("Generate Clips"))
        {
            string trackName = "Conversation Track";

            DialogTrack newTrack = asset.CreateTrack<DialogTrack>(trackName);
            UndoExtensions.RegisterTrack(newTrack, "Dialog track created");

            for (int i = 0; i < dialogClipDatas.Count; i++)
            {
                DialogClipData cd = dialogClipDatas[i];

                TimelineClip tClip = newTrack.CreateClip<DialogAsset>();
                DialogAsset dialogClip = tClip.asset as DialogAsset;
                tClip.start = cd.startTime;
                tClip.duration = cd.duration;
                tClip.displayName = cd.subtitle;

                dialogClip.character.exposedName = GUID.Generate().ToString();
                if(cd.character != null)
                    TimelineEditor.inspectedDirector.SetReferenceValue(dialogClip.character.exposedName, cd.character);

                dialogClip.animationClipName = cd.animationClipName;
                dialogClip.audio = cd.audioClip;
                dialogClip.subtitle = cd.subtitle;
                dialogClip.isLearner = cd.isLearner;
                dialogClip.isTutorial = cd.isTutorial;
            }

            TimelineEditor.inspectedDirector.SetGenericBinding(newTrack, dialogEventManager);

            TimelineEditor.Refresh(RefreshReason.ContentsAddedOrRemoved);

            EditorUtility.DisplayDialog("Timeline Generator", "Conversation Track Created", "Ok");
        }
    }

    void OnDialogTrackLoadClick()
    {
        if (!EditorPrefs.HasKey(DialogManager.KEY_PATH))
        {
            Debug.Log("No Path Found to load Dialog Track");
            return;
        }

        dialogClipDatas.Clear();
        ListDialogClipData clipDatas = JsonImporter.LoadDialogTrack(EditorPrefs.GetString(DialogManager.KEY_PATH));

        if (clipDatas == null)
        {
            Debug.LogError("No File Found to load Dialog Track");
            return;
        }

        //GameObject g = GameObject.Find(clipDatas.targetObject);
        //if (g == null)
        //{
        //    Debug.LogError("Unable to find the GameObject named " + clipDatas.targetObject);
        //    return;
        //}

        //faceLookAt = g.GetComponent<FaceLookAt>();
        //if (faceLookAt == null)
        //{
        //    Debug.LogError("There is no FaceLookAt attached to the GameObject " + clipDatas.targetObject);
        //    return;
        //}

        string none = "null";
        for (int i = 0; i < clipDatas.characterNames.Count; i++)
        {
            DialogClipData clip = new DialogClipData();

            if(clipDatas.characterNames[i] != none)
            {
                GameObject character = GameObject.Find(clipDatas.characterNames[i]);
                if (character != null)
                    clip.character = character;
            }

            if(clipDatas.animationClipNames[i] != none)
            {
                clip.animationClipName = clipDatas.animationClipNames[i];
            }

            if(clipDatas.audioClipGuids[i] != none)
            {
                clip.audioClip = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(clipDatas.audioClipGuids[i])) as AudioClip;
            }

            clip.subtitle = clipDatas.subtitles[i];
            clip.isLearner = clipDatas.isLearners[i];
            clip.isTutorial = clipDatas.isTutorials[i];
            clip.startTime = clipDatas.startTimes[i];
            clip.duration = clipDatas.durations[i];

            dialogClipDatas.Add(clip);
        }
    }
    #endregion

    [MenuItem("Tools/Print AudioClip GUID's")]
    static void FindAudioClip()
    {
        string[] guids = AssetDatabase.FindAssets("t:audioclip", new[] { "Assets/Assets/Missions/Level2/Course1/Scenario1/Mission1/Audio" });
        Debug.Log("guids length : " + guids.Length);
        foreach(string guid in guids)
        {
            string clipName = "R2S1M1_Lisa_02";

            AudioClip clip = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(guid)) as AudioClip;
            if(clip != null && clipName == clip.name)
            {
                Debug.Log("Clip named " + clipName + " found : " + guid);
            }
        }
    }

    enum TrackType
    {
        Animation,
        Tween,
        LookAt,
        Dialog
    }

    class AnimClipData
    {
        public AnimationClip clip;
        public double startTime;
        public double duration;
        public string stateName;
    }

    class TweenClipData
    {
        public Vector3 startPosition, startRotation;
        public Vector3 endPosition, endRotation;
        public TweenBehaviour.TranslateType translateType = TweenBehaviour.TranslateType.HoldNewPosition;
        public double startTime;
        public double duration;
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
        public string targetName;
    }

    class DialogClipData
    {
        public bool isTutorial;
        public bool isLearner;
        public GameObject character;
        public string animationClipName;
        public AudioClip audioClip;
        public string subtitle;
        public double startTime;
        public double duration;
    }
}
