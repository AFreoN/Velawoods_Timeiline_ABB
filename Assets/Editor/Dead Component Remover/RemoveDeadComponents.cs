using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class RemoveDeadComponents
{
    [MenuItem("GameObject/Remove Missing Components")]
    public static void RemoveComps()
    {
        GameObject[] allObjects = Object.FindObjectsOfType<GameObject>();

        int objCount = 0, scriptCount = 0;

        foreach(GameObject g in allObjects)
        {
            int count = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(g);
            if (count > 0)
            {
                objCount++;
                scriptCount += count;
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(g);
            }
        }
        Debug.Log("Removed " + scriptCount + " missing scripts on " + objCount + " objects");
    }

    //[MenuItem("GameObject/Remove Missing Scripts")]
    static void RemoveMissingScritps()
    {
        if (Selection.activeGameObject == null) return;

        int count = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(Selection.activeGameObject);
        Debug.Log("Missing Count : " + count);

        if(count > 0)
        {
            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(Selection.activeGameObject);
        }
    }
}
