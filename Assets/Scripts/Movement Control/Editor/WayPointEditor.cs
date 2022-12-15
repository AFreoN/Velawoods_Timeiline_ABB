using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WaypointMovement))]
public class WayPointEditor : Editor
{
    WaypointMovement wm;

    private void OnEnable()
    {
        wm = (WaypointMovement)target;
    }

    private void OnSceneGUI()
    {
        Draw();
    }

    void Draw() //Draw position handles for all the waypoints
    {
        Handles.color = new Color(1,1,1, .1f);
        for (int i = 0; i < wm.WayPoints.Count; i++)
        {
            //Gizmos.DrawSphere(waypoints[i].position, 1f);
            //Vector3 final = Handles.FreeMoveHandle(wm.WayPoints[i].position, Quaternion.identity, .2f, Vector3.zero, Handles.SphereHandleCap);
            Vector3 final = Handles.PositionHandle(wm.WayPoints[i].position, Quaternion.identity);
            Handles.DrawSolidDisc(final, Vector3.up, .1f);
            if(final != wm.WayPoints[i].position)
            {
                Undo.RecordObject(wm, "WayPoint Position is changed using Handles");
            }
            wm.WayPoints[i].position = final;
        }
        Handles.color = Color.white;
    }
}
