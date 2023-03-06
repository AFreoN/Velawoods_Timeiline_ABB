using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.Timeline;
using UnityEngine.Timeline;
using System;
using CustomExtensions;
using CustomTracks;

[CanEditMultipleObjects]
[CustomEditor(typeof(TweenAsset))]
public class TweenSceneDraw : Editor
{
    TweenAsset asset;
    TweenBehaviour behaviour;

    static GUIStyle style;
    static Texture2D texture;
    static bool showHandles = true;

    SerializedProperty timeCurve;

    private void OnEnable()
    {
        SceneView.duringSceneGui += SceneGUI;

        asset = (TweenAsset)target;
        behaviour = asset.behaviour;

        if(style == null)
        {
            style = new GUIStyle();
            if(texture == null)
            {
                texture = new Texture2D(1, 1);
                texture.SetPixel(0, 0, new Color(0.3f, 0.3f, 0.3f));
                texture.Apply();
            }
            style.normal.background = texture;
        }

        timeCurve = serializedObject.FindProperty("timeCurve");
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= SceneGUI;
        if(texture != null)
        {
            DestroyImmediate(texture);
        }
    }

    void SceneGUI(SceneView sceneView)
    {
        //Draws OnSceneGUI for all the selected tween track clips
        foreach (TweenAsset t in targets)
        {
            if (t.OnSceneSelected(showHandles))
            {
                Repaint();
                TimelineEditor.Refresh(RefreshReason.ContentsModified);
            }
        }

        if(target != null)
        {
            Event e = Event.current;
            switch (e.type)
            {
                case EventType.KeyDown:
                    if (Event.current.keyCode == KeyCode.F)
                    {
                        TweenAsset t = (TweenAsset)target;
                        //Selection.activeTransform = t.targetTransform;

                        Bounds b = new Bounds();
                        b.center = (t.behaviour.startPosition + t.behaviour.endPosition) / 2;
                        b.size = Vector3.one * 2;
                        SceneView.lastActiveSceneView.Frame(b);
                        //Debug.Log("Target position = " + target.);
                    }
                    break;
            }
        }

    }

    void showEnum(Enum e)
    {
        if(e.GetType() == typeof(TweenBehaviour.TranslateType))
        {
            TweenBehaviour.TranslateType prev = (TweenBehaviour.TranslateType)e;
            TweenBehaviour.TranslateType cur = (TweenBehaviour.TranslateType)EditorGUILayout.EnumPopup("Translate Type", behaviour.translateType);

            if(prev != cur)
            {
                UndoExtensions.RegisterPlayableAsset(asset, "Tween Behaviour Translate Type Changed");
                behaviour.translateType = cur;
            }
        }
        if(e.GetType() == typeof(TweenBehaviour.BezierType))
        {
            TweenBehaviour.BezierType prev = (TweenBehaviour.BezierType)e;
            TweenBehaviour.BezierType cur = (TweenBehaviour.BezierType)EditorGUILayout.EnumPopup("Curve Type", behaviour.curveType);

            if(prev != cur)
            {
                UndoExtensions.RegisterPlayableAsset(asset, "Tween Behaviour Curve Type Changed");
                behaviour.curveType = cur;

                if(cur == TweenBehaviour.BezierType.Quadratic)
                {
                    if (behaviour.point1 == Vector3.zero)
                        behaviour.point1 = (behaviour.startPosition + behaviour.endPosition) / 2;
                }
                else if(cur == TweenBehaviour.BezierType.Cubic)
                {
                    Vector3 diff = (behaviour.endPosition - behaviour.startPosition) / 2;
                    if (behaviour.point1 == Vector3.zero)
                        behaviour.point1 = behaviour.startPosition + diff / 3;

                    if (behaviour.point2 == Vector3.zero)
                        behaviour.point2 = behaviour.startPosition + diff * 2 / 3;
                }
            }
        }
    }

    void ShowVector(ref Vector3 v, string label, string undoName)
    {
        Vector3 s = v, e = v;

        e = EditorGUILayout.Vector3Field(label, v);

        if (s != e)
        {
            UndoExtensions.RegisterPlayableAsset(asset, undoName);
            v = e;
        }
    }

    bool isSameCurve(AnimationCurve ac1, AnimationCurve ac2)
    {
        if (ac1.keys.Length != ac2.keys.Length)
            return false;

        for(int i = 0; i < ac1.keys.Length; i++)
        {
            if (ac1.keys[i].value != ac2.keys[i].value)
                return false;

            if (ac1.keys[i].time != ac2.keys[i].time)
                return false;
        }
        return true;
    }

    public override void OnInspectorGUI()
    {
        if (behaviour.translateType == TweenBehaviour.TranslateType.Hold)
        {
            showEnum(behaviour.translateType);
            return;
        }

        EditorGUI.BeginChangeCheck();

        if (showHandles) { if (GUILayout.Button("Hide Handles")) showHandles = false; }
        else if (GUILayout.Button("Show Handles")) showHandles = true;

        if(behaviour.translateType != TweenBehaviour.TranslateType.HoldNewPosition)
        {
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            bool u1 = behaviour.useCurveRotation, u2 = behaviour.useCurveRotation;
            u2 = EditorGUILayout.Toggle("Use Curve Rotation", behaviour.useCurveRotation);
            if (u1 != u2)
            {
                UndoExtensions.RegisterPlayableAsset(asset, "Tween boolean use curve rotation changed");
                behaviour.useCurveRotation = u2;
            }

            bool t1 = behaviour.useTimeCurve, t2 = behaviour.useTimeCurve;
            t2 = EditorGUILayout.Toggle("Use Time Curve", behaviour.useTimeCurve);
            if (t1 != t2)
            {
                UndoExtensions.RegisterPlayableAsset(asset, "Tween boolean use time curve changed");
                behaviour.useTimeCurve = t2;
            }
            GUILayout.EndHorizontal();

            if (behaviour.useCurveRotation)
            {
                float r1 = behaviour.rotationOffset, r2 = behaviour.rotationOffset;
                r1 = EditorGUILayout.FloatField("Rotation Offset", r1);
                if (r1 != r2)
                {
                    UndoExtensions.RegisterPlayableAsset(asset, "Tween track rotation offset changed");
                    behaviour.rotationOffset = r1;
                }
            }

            if (behaviour.useTimeCurve)
            {
                GUILayout.Space(10);
                EditorGUILayout.PropertyField(timeCurve);
                //AnimationCurve curve1 = behaviour.timeCurve.clone();
                //curve1 = EditorGUILayout.CurveField("Time Curve", curve1);
                //if(!isSameCurve(curve1, behaviour.timeCurve))
                //{
                //    Debug.Log("Curve Changed");
                //    UndoExtensions.RegisterPlayableAsset(asset, "Tween time curve changed");
                //    behaviour.timeCurve = curve1;
                //}
            }
        }

        GUILayout.Space(10);

        EditorGUILayout.BeginVertical(style);
        showEnum(behaviour.translateType);

        if (behaviour.translateType != TweenBehaviour.TranslateType.FromPreviousClip)
        {
            GUILayout.Space(10);
            ShowVector(ref behaviour.startPosition, "Start Position", "tween start position changed");

            if (behaviour.useCurveRotation == false)
            {
                ShowVector(ref behaviour.startRotation, "Start Rotation", "tween start rotation changed");
            }
        }

        if (behaviour.translateType != TweenBehaviour.TranslateType.HoldNewPosition)
        {
            GUILayout.Space(10);
            ShowVector(ref behaviour.endPosition, "End Position", "tween end position changed");

            if (behaviour.useCurveRotation == false)
            {
                ShowVector(ref behaviour.endRotation, "End Rotation", "tween end rotation changed");
            }
        }
        EditorGUILayout.EndVertical();
        //DrawLine(boxRect);

        if(behaviour.translateType != TweenBehaviour.TranslateType.HoldNewPosition)
        {
            GUILayout.Space(30);
            EditorGUILayout.BeginVertical(style);
            showEnum(behaviour.curveType);

            if (behaviour.curveType == TweenBehaviour.BezierType.Quadratic)
            {
                GUILayout.Space(10);
                ShowVector(ref behaviour.point1, "Control Point", "tween control point 1 changed");
            }
            else if (behaviour.curveType == TweenBehaviour.BezierType.Cubic)
            {
                GUILayout.Space(10);

                ShowVector(ref behaviour.point1, "Control Point 1", "tween control point 1 changed");
                ShowVector(ref behaviour.point2, "Control Point 2", "tween control point 2 changed");
            }
            EditorGUILayout.EndVertical();
        }

        if (EditorGUI.EndChangeCheck())
        {
            
        }
        serializedObject.ApplyModifiedProperties();
        //DrawLine(boxRect);

        GUILayout.Space(20);
        GUILayout.BeginHorizontal();
        if(asset.targetTransform != null && behaviour.translateType != TweenBehaviour.TranslateType.FromPreviousClip)
        {
            if(GUILayout.Button("Copy start values from transform"))
            {
                UndoExtensions.RegisterPlayableAsset(asset, "start position modified");
                behaviour.startPosition = asset.targetTransform.position;
                behaviour.startRotation = asset.targetTransform.eulerAngles;
                TimelineEditor.Refresh(RefreshReason.ContentsModified);
            }
        }

        if(asset.targetTransform != null && behaviour.translateType != TweenBehaviour.TranslateType.HoldNewPosition)
        {
            if(GUILayout.Button("Copy end values from transform"))
            {
                UndoExtensions.RegisterPlayableAsset(asset, "end position modified");
                behaviour.endPosition = asset.targetTransform.position;
                behaviour.endRotation = asset.targetTransform.eulerAngles;
                TimelineEditor.Refresh(RefreshReason.ContentsModified);
            }
        }
        GUILayout.EndHorizontal();
    }

    void DrawLine(Rect boxRect)
    {
        boxRect.y -= 5;
        GUI.BeginClip(boxRect);
        Handles.color = Color.white;
        //Vector3[] points = new Vector3[] {}
        Handles.DrawAAPolyLine(Texture2D.whiteTexture, 2, new Vector3[2] { new Vector3(0, 0, 0), new Vector3(EditorGUIUtility.currentViewWidth, 1, 0) });
        GUI.EndClip();
    }

    Color getColor(float x, float y, float z)
    {
        return new Color(x / 255f, y / 255f, z / 255);
    }
}
