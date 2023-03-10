using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class RemoveDeadComponents
{
    [MenuItem("GameObject/Remove Missing Components")]
    public static void RemoveComps()
    {
        GameObject[] allObjects = Object.FindObjectsOfType<GameObject>(true);

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

    [MenuItem("Assets/Remove Missing Components")]
    public static void RemoveMissingCompsInPrefabs()
    {
        Object[] selections = Selection.objects;

        int objCount = 0, scriptCount = 0;

        foreach (Object o in selections)
        {
            GameObject g = o as GameObject;

            if (g == null) continue;

            int thisCount = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(g);

            if(thisCount > 0)
            {
                objCount++;
                scriptCount += thisCount;
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(g);
            }

            foreach(Transform t in g.transform.GetComponentsInChildren<Transform>(true))
            {
                thisCount = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject);

                if(thisCount > 0)
                {
                    objCount++;
                    scriptCount += thisCount;
                    GameObjectUtility.RemoveMonoBehavioursWithMissingScript(t.gameObject);
                }
            }
        }

        Debug.Log("Total object selected : " + selections.Length);
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
