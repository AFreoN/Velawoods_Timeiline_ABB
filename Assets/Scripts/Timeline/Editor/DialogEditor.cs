using UnityEngine;
using UnityEditor;
using UnityEditor.Timeline;
using CustomTracks;

[CustomEditor(typeof(DialogAsset))]
public class DialogEditor : Editor
{
    DialogAsset asset;

    private void OnEnable()
    {
        asset = (DialogAsset)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Space(10);

        if(asset.audio != null && GUILayout.Button("Copy animation name"))
        {
            string animName = "Default_" + asset.audio.name;
            if ( asset.animationClipName != animName)
            {
                UndoExtensions.RegisterPlayableAsset(asset, "Dialog asset animation name changed");
                asset.animationClipName = animName;
            }
        }
    }
}
