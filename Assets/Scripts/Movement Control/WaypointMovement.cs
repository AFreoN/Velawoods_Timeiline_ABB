using UnityEngine;
using System.Collections.Generic;
using CustomExtensions;
using System.Collections;

public class WaypointMovement : MonoBehaviour
{
    [HideInInspector] public GameObject originalGameobject;   //Reference to the original gameobject this one is cloned from, If not this is null

    Animator anim = null;
    bool canMove = false, holdMovement = false;

    [Header("Waypoints"), SerializeField, Space(10)]
    List<WayPoint> waypoints = new List<WayPoint>();        //Holds list of target position, animation to play and duration to reach the target point
    public List<WayPoint> WayPoints => waypoints;

    int currentPoint = 0;
    string currentAnimName = "";

    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        if (canMove)
        {
            //handleRotation();

            if (holdMovement == false)
                moveToNextPoint();

        }
    }

    /// <summary>
    /// Move the character from the current position to the target position, rotate the character to face forward to the movement direction
    /// </summary>
    void moveToNextPoint()
    {
        if (currentPoint < waypoints.Count)
        {
            holdMovement = true;

            if(anim != null && anim.GetCurrentAnimatorStateInfo(1).IsName(waypoints[currentPoint].animName) == false)
            {
                anim.Play(waypoints[currentPoint].animName);
            }

            Vector3 v = waypoints[currentPoint].position;
            Vector3 rot = transform.getLookRotationInEuler(v);
            float targetDuration = waypoints[currentPoint].duration;

            iTween.MoveTo(gameObject, iTween.Hash("x",v.x,"y", v.y, "z", v.z, "time", targetDuration, "easeType", "linear"));
            iTween.RotateTo(gameObject, iTween.Hash("x", rot.x, "y", rot.y, "z", rot.z, "time", .3f, "easeType", "linear"));

            IEnumerator routine = stopHoldingMovement(true, targetDuration);

            currentPoint++;
            if (currentPoint >= waypoints.Count)
            {
                //FunctionTimer.Create(() => canMove = false, targetDuration);
                routine = stopHoldingMovement(false, targetDuration);
                StartCoroutine(routine);
            }
            else
                StartCoroutine(routine);
        }
    }

    IEnumerator stopHoldingMovement(bool _temporary,float _time)    //Holds changing the target point for specified duration
    {
        if (_temporary)
        {
            yield return new WaitForSeconds(_time);
            holdMovement = false;
        }
        else
        {
            yield return new WaitForSeconds(_time);
            canMove = false;
        }
    }

    //Initialize movement once this component is added
    public void startMoving()
    {
        canMove = true;
    }

    [ContextMenu("Copy Current Position")]
    public void copyCurrentPosition()   //Copy the current position of the tranform in the last point of the waypoint list
    {
        if (waypoints.Count <= 0) return;

        UnityEditor.Undo.RecordObject(this, "Waypoint position change");

        waypoints[waypoints.Count - 1].position = transform.position;
    }
}

[System.Serializable]
public class WayPoint
{
    public Vector3 position;

    public string animName;

    [Tooltip("Duration to reach the destination"), Range(0,20)]
    public float duration;
}
