using UnityEngine;
using UnityEditor;
using System;

public class UnityCopyPaste
{
    [MenuItem("GameObject/Copy Components", false, 22)]
    public static void CopyComponents(MenuCommand command)
    {
        var go = command.context as GameObject;
        go = Selection.activeGameObject;
        var components = go.GetComponents<Component>();
        var serializedData = new string[components.Length];
        for (int i = 0; i < components.Length; i++)
            // Type-AssemblyQualifiedName : Json-Serialized-Data
            serializedData[i] = components[i].GetType().AssemblyQualifiedName + ":" + EditorJsonUtility.ToJson(components[i]);
        EditorGUIUtility.systemCopyBuffer = string.Join("\n", serializedData);
    }

    [MenuItem("GameObject/Paste Components", false, 23)]
    public static void PasteComponents(MenuCommand command)
    {
        var go = command.context as GameObject;
        go = Selection.activeGameObject;
        var serializedData = EditorGUIUtility.systemCopyBuffer.Split('\n');
        char[] splitter = { ':' };
        foreach (var data in serializedData)
        {
            var typeAndJson = data.Split(splitter, 2);
            var type = Type.GetType(typeAndJson[0]);
            if (type.FullName == "UnityEngine.Transform") // only 1 transform
                EditorJsonUtility.FromJsonOverwrite(typeAndJson[1], go.transform);
            else
                EditorJsonUtility.FromJsonOverwrite(typeAndJson[1], go.AddComponent(type));
        }
    }

    [MenuItem("GameObject/Copy Transform &c", false, 20)]
    public static void CopyTransform()
    {
        var go = Selection.activeGameObject;
        if (go == null) return;

        Transform t = go.GetComponent<Transform>();
        var serializedData = t.GetType().AssemblyQualifiedName + ":" + EditorJsonUtility.ToJson(t);
        EditorGUIUtility.systemCopyBuffer = String.Join("\n", serializedData);
    }

    [MenuItem("GameObject/Paste Transform &v", false, 21)]
    public static void PasteTransform()
    {
        GameObject g = Selection.activeGameObject;
        if (g == null) return;

        Undo.RecordObject(g.transform, "Paste Transform");

        var serializedData = EditorGUIUtility.systemCopyBuffer.Split('\n');
        char[] splitter = { ':' };
        foreach(var data in serializedData)
        {
            var typeAndJson = data.Split(splitter, 2);
            var type = Type.GetType(typeAndJson[0]);
            if (type.FullName == "UnityEngine.Transform")
                EditorJsonUtility.FromJsonOverwrite(typeAndJson[1], g.transform);
            else
                Debug.Log("Cannot paste non transform values");
        }
    }
}
