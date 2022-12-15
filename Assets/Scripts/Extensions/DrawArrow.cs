using UnityEngine;
using System.Collections;
using UnityEditor;
using CustomExtensions;

public static class DrawArrow
{
    public static void ForGizmo(Vector3 pos, Vector3 direction, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
    {
        //Gizmos.DrawRay(pos, direction);
        Handles.DrawAAPolyLine(new Vector3[] { pos, pos + direction });

        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Gizmos.DrawRay(pos + direction, right * arrowHeadLength);
        Gizmos.DrawRay(pos + direction, left * arrowHeadLength);
    }

    public static void ForGizmo(Vector3 pos, Vector3 direction, Color color, float thickness = 2.5f, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
    {
        Gizmos.color = color;
        //Gizmos.DrawRay(pos, direction);

        Handles.color = color;
        //Handles.DrawAAPolyLine(new Vector3[] { pos, pos + direction });
        DrawThickLine(pos, pos + direction, color, 5);

        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Vector3 up = Quaternion.LookRotation(direction) * Quaternion.Euler(180 + arrowHeadAngle, 0, 0) * new Vector3(0, 0, 1);
        Vector3 down = Quaternion.LookRotation(direction) * Quaternion.Euler(180 - arrowHeadAngle, 0, 0) * new Vector3(0, 0, 1);

        //Gizmos.DrawRay(pos + (direction/2), right * arrowHeadLength);
        //Gizmos.DrawRay(pos + (direction/2), left * arrowHeadLength);
        //Gizmos.DrawRay(pos + (direction/2), up * arrowHeadLength);
        //Gizmos.DrawRay(pos + (direction/2), down * arrowHeadLength);

        Vector3 p1 = pos + direction;
        DrawThickLine(p1, right * arrowHeadLength + p1, color, thickness);
        DrawThickLine(p1, left * arrowHeadLength + p1, color, thickness);
        //DrawThickLine(p1, up * arrowHeadLength + p1, color, 5);
        //DrawThickLine(p1, down * arrowHeadLength + p1, color, 5);
    }

    public static void ForGizmoMiddle(Vector3 pos, Vector3 direction, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
    {
        Gizmos.color = color;
        //Gizmos.DrawRay(pos, direction);

        Handles.color = color;
        //Handles.DrawAAPolyLine(new Vector3[] { pos, pos + direction });
        DrawThickLine(pos, pos + direction, color, 5);

        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Vector3 up = Quaternion.LookRotation(direction) * Quaternion.Euler(180 + arrowHeadAngle, 0, 0) * new Vector3(0, 0, 1);
        Vector3 down = Quaternion.LookRotation(direction) * Quaternion.Euler(180 - arrowHeadAngle, 0, 0) * new Vector3(0, 0, 1);

        //Gizmos.DrawRay(pos + (direction/2), right * arrowHeadLength);
        //Gizmos.DrawRay(pos + (direction/2), left * arrowHeadLength);
        //Gizmos.DrawRay(pos + (direction/2), up * arrowHeadLength);
        //Gizmos.DrawRay(pos + (direction/2), down * arrowHeadLength);

        Vector3 p1 = pos + (direction / 2);
        DrawThickLine(p1, right * arrowHeadLength + p1, color, 5);
        DrawThickLine(p1, left * arrowHeadLength + p1, color, 5);
        //DrawThickLine(p1, up * arrowHeadLength + p1, color, 5);
        //DrawThickLine(p1, down * arrowHeadLength + p1, color, 5);
    }

    static void DrawThickLine(Vector3 p1, Vector3 p2, Color color, float thickness)
    {
        Handles.DrawBezier(p1, p2, p1, p2, color, null, thickness);
    }

    public static void ForQuadraticBezierMiddle(Vector3 startPoint, Vector3 endPoint, Vector3 controlPoint, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
    {
        Gizmos.color = color;
        //Gizmos.DrawRay(pos, direction);

        Handles.color = color;
        DrawCubicBezier(startPoint, endPoint, controlPoint, color, 5);

        Vector3 sample1 = Bezier.getQuadraticPoint(startPoint, endPoint, controlPoint, .51f);
        Vector3 sample2 = Bezier.getQuadraticPoint(startPoint, endPoint, controlPoint, .49f);

        Vector3 direction = sample1 - sample2;

        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);

        Vector3 p1 = (sample1 + sample2)/2;
        DrawThickLine(p1, right * arrowHeadLength + p1, color, 5);
        DrawThickLine(p1, left * arrowHeadLength + p1, color, 5);
    }

    public static void DrawCubicBezier(Vector3 p1, Vector3 p2, Vector3 p3, Color color, float thickness)
    {
        Handles.color = color;

        Vector3 c1 = p1 + 2f / 3f * (p3 - p1);
        Vector3 c2 = p2 + 2f / 3f * (p3 - p2);

        Handles.DrawBezier(p1, p2, c1, c2, color, null, thickness);
    }

    public static void ForCubicBezierMiddle(Vector3 startPoint, Vector3 endPoint, Vector3 p1, Vector3 p2, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
    {
        Gizmos.color = color;
        //Gizmos.DrawRay(pos, direction);

        Handles.color = color;
        Handles.DrawBezier(startPoint, endPoint, p1, p2, color.linear,null, 5);

        Vector3 sample1 = Bezier.getCubicPoint(startPoint, endPoint, p1, p2, .51f);
        Vector3 sample2 = Bezier.getCubicPoint(startPoint, endPoint, p1, p2, .49f);

        Vector3 direction = sample1 - sample2;

        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);

        Vector3 m = (sample1 + sample2) / 2;
        DrawThickLine(m, right * arrowHeadLength + m, color, 5);
        DrawThickLine(m, left * arrowHeadLength + m, color, 5);
    }

    public static void ForDebug(Vector3 pos, Vector3 direction, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
    {
        Debug.DrawRay(pos, direction);

        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Debug.DrawRay(pos + direction, right * arrowHeadLength);
        Debug.DrawRay(pos + direction, left * arrowHeadLength);
    }
    public static void ForDebug(Vector3 pos, Vector3 direction, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
    {
        Debug.DrawRay(pos, direction, color);

        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
        Debug.DrawRay(pos + direction, right * arrowHeadLength, color);
        Debug.DrawRay(pos + direction, left * arrowHeadLength, color);
    }
}
