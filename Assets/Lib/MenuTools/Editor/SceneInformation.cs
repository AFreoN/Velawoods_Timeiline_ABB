using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class SceneInformation : MonoBehaviour
{
    [MenuItem("VELA/Scene/Models/Get Poly Counts")]
    public static void GetModelPolyCounts()
    {
        string[] guids = AssetDatabase.FindAssets("t:Mesh", null);

        HashSet<string> meshes = new HashSet<string>();
        foreach (string guid in guids)
        {
            meshes.Add(AssetDatabase.GUIDToAssetPath(guid));
        }

        List<string> meshData = new List<string>();
        foreach (string meshFileName in meshes)
        {
            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshFileName);

            string tempMeshData = "";

            string[] fileNameSplit = meshFileName.Split(new char[] { '/' });
            tempMeshData += fileNameSplit[fileNameSplit.Length - 1] + ", ";

            tempMeshData += mesh.triangles.Length / 3;

            meshData.Add(tempMeshData);
        }

        System.IO.File.WriteAllLines(Application.dataPath + "/MeshPolyCount.csv", meshData.ToArray());

        Debug.Log("Found poly count for " + meshes.Count + " models, list can be found at: " + Application.dataPath + "/MeshPolyCount.csv");
    }

/*    [MenuItem("VELA/Scene/Animations/Get list used for characters")]
    static void GetAListOfAllAnimations()
    {
        HashSet<string> allClips = new HashSet<string>();

        foreach (WellFired.USTimelineAnimation timeline in Resources.FindObjectsOfTypeAll(typeof(WellFired.USTimelineAnimation)))
        {
            foreach (WellFired.AnimationTrack track in timeline.AnimationTracks)
            {
                if (timeline.AffectedObject.GetComponent<Animator>().runtimeAnimatorController.name == "ACTR_Universal")
                {
                    foreach (WellFired.AnimationClipData clip in track.TrackClips)
                    {
                        allClips.Add(clip.StateName);
                    }
                }
            }
        }

        EditorGUIUtility.systemCopyBuffer = string.Join(System.Environment.NewLine, allClips.ToArray());

        Debug.Log("A list of " + allClips.Count + " animation clips have been copied to the clipboard");
    }*/

/*    [MenuItem("VELA/Scene/Events/Get List of all used")]
    static void GetAListOfAllEvents()
    {
        HashSet<string> eventNames = new HashSet<string>();

        foreach (WellFired.USTimelineEvent timeline in Resources.FindObjectsOfTypeAll(typeof(WellFired.USTimelineEvent)))
        {
            foreach (WellFired.USEventBase evt in timeline.Events)
            {
                eventNames.Add(evt.name);
            }
        }

        EditorGUIUtility.systemCopyBuffer = string.Join(System.Environment.NewLine, eventNames.ToArray());

        Debug.Log("A list of " + eventNames.Count + " events have been copied to the clipboard");
    }*/
}
