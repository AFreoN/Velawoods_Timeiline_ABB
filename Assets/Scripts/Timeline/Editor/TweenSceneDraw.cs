using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.Timeline;
using UnityEngine.Timeline;

[CanEditMultipleObjects]
[CustomEditor(typeof(TweenAsset))]
public class TweenSceneDraw : Editor
{
    TweenAsset asset;
    TweenBehaviour behaviour;

    private void OnEnable()
    {
        SceneView.duringSceneGui += SceneGUI;

        asset = (TweenAsset)target;
        behaviour = asset.behaviour;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= SceneGUI;
    }

    void SceneGUI(SceneView sceneView)
    {
        //Draws OnSceneGUI for all the selected tween track clips
        foreach (TweenAsset t in targets)
        {
            if (t.OnSceneSelected())
            {
                Repaint();
                TimelineEditor.Refresh(RefreshReason.ContentsModified);
            }
        }
    }

    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();

        behaviour.translateType = (TweenBehaviour.TranslateType)EditorGUILayout.EnumPopup("Translate Type", behaviour.translateType);

        if(behaviour.translateType != TweenBehaviour.TranslateType.FromPreviousClip)
        {
            GUILayout.Space(10);

            Vector3 sp = behaviour.startPosition;
            Vector3 sr = behaviour.startRotation;

            behaviour.startPosition = EditorGUILayout.Vector3Field("Start Position", behaviour.startPosition);
            behaviour.startRotation = EditorGUILayout.Vector3Field("Start Rotation", behaviour.startRotation);

            if (sp != behaviour.startPosition || sr != behaviour.startRotation)
                UndoExtensions.RegisterPlayableAsset(asset, "start position changed");
        }

        if(behaviour.translateType != TweenBehaviour.TranslateType.HoldNewPosition)
        {
            GUILayout.Space(10);

            Vector3 ep = behaviour.endPosition;
            Vector3 er = behaviour.endRotation;

            behaviour.endPosition = EditorGUILayout.Vector3Field("End Position", behaviour.endPosition);
            behaviour.endRotation = EditorGUILayout.Vector3Field("End Rotation", behaviour.endRotation);

            if (ep != behaviour.endPosition || er != behaviour.endRotation)
                UndoExtensions.RegisterPlayableAsset(asset, "end position changed");
        }

        if(asset.targetTransform != null && behaviour.translateType != TweenBehaviour.TranslateType.FromPreviousClip)
        {
            GUILayout.Space(10);
            if(GUILayout.Button("Copy Start Values"))
            {
                UndoExtensions.RegisterPlayableAsset(asset, "start position modified");
                behaviour.startPosition = asset.targetTransform.position;
                behaviour.startRotation = asset.targetTransform.eulerAngles;
                TimelineEditor.Refresh(RefreshReason.ContentsModified);
            }
        }

        if(asset.targetTransform != null && behaviour.translateType != TweenBehaviour.TranslateType.HoldNewPosition)
        {
            GUILayout.Space(10);
            if(GUILayout.Button("Copy End Values"))
            {
                UndoExtensions.RegisterPlayableAsset(asset, "end position modified");
                behaviour.endPosition = asset.targetTransform.position;
                behaviour.endRotation = asset.targetTransform.eulerAngles;
                TimelineEditor.Refresh(RefreshReason.ContentsModified);
            }
        }
    }
}
