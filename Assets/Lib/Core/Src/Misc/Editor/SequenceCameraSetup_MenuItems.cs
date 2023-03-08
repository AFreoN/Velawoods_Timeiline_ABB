using UnityEngine;
using System.Collections;
using UnityEditor;


public class SequenceCameraSetup_MenuItems : MonoBehaviour
{
    [MenuItem("VELA/Sequence Camera Setup/Zero Transform")]
    public static void ZeroTransform()
    {
        if (Camera.main == null)
        {
            Debug.LogError("SequenceCameraSetup: Camera.main is null!");
            return;
        }

        foreach (SequenceCameraSetup scs in Resources.FindObjectsOfTypeAll(typeof(SequenceCameraSetup)))
        {
            Undo.RecordObject(scs, "Sequence Camera Setup / Zero Transform");
            scs.initialCameraPosition = Vector3.zero;
            scs.initialCameraRotation = Vector3.zero;

        }
    }

    [MenuItem("VELA/Sequence Camera Setup/Set from Main Camera")]
    public static void SetFromMainCamera()
    {
        if (Camera.main == null)
        {
            Debug.LogError("SequenceCameraSetup: Camera.main is null!");
            return;
        }

        foreach (SequenceCameraSetup scs in Resources.FindObjectsOfTypeAll(typeof(SequenceCameraSetup)))
        {
            Undo.RecordObject(scs, "Sequence Camera Setup / Set from Main Camera");
            scs.initialCameraPosition = Camera.main.transform.position;
            scs.initialCameraRotation = Camera.main.transform.eulerAngles;
        }
    }


    [MenuItem("VELA/Sequence Camera Setup/Set from Current Viewport")]
    public static void SetFromCurrentViewport()
    {
        if (!SceneView.lastActiveSceneView)
        {
            Debug.LogWarning("SequenceCameraSetup: No Last Active SceneView to get the current viewport");
            return;
        }

        if (Camera.main == null)
        {
            Debug.LogError("SequenceCameraSetup: Camera.main is null!");
            return;
        }

        foreach (SequenceCameraSetup scs in Resources.FindObjectsOfTypeAll(typeof(SequenceCameraSetup)))
        {
            Undo.RecordObject(scs, "Sequence Camera Setup / Set from Current Viewport");
            scs.initialCameraPosition = SceneView.lastActiveSceneView.camera.transform.position;
            scs.initialCameraRotation = SceneView.lastActiveSceneView.camera.transform.eulerAngles;
        }
    }
}
