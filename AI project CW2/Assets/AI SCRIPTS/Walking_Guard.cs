using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Walking_Guard : Guard
{
    public List<Vector3> waypoints = new List<Vector3>();
    public int waypoint_index = 0;

    // begins the guard's movement for walking (for PATROL state)
    public void BeginWalking()
    {
        agent.enabled = false;
        transform.position = waypoints[0];

        transform.rotation = Quaternion.identity;
        agent.enabled = true;
        Current_Behaviour_Enum = StartCoroutine(WaypointMovement());
    }

    // a continuous function for the guard's patrolling actions
    IEnumerator WaypointMovement()
    {
        bool reverse = false; // depicts whether to go backwards in the list of waypoints
        while (true)
        {
            turning = true;

            // begin looking in 3 directions to check space

            rotateCounter = 0.0f;
            current = transform.rotation;
            target = current * Quaternion.Euler(0, 90f, 0);
            yield return new WaitForSeconds(1f);

            rotateCounter = 0.0f;
            current = transform.rotation;
            target = current * Quaternion.Euler(0, 90f, 0);

            yield return new WaitForSeconds(1f);
            rotateCounter = 0.0f;
            current = transform.rotation;
            target = current * Quaternion.Euler(0, 90f, 0);
            yield return new WaitForSeconds(1f);

            turning = false;

            // determine whether the waypoint list should go forwards or backwards
            if (waypoint_index == waypoints.Count - 1 && !reverse)
            {
                reverse = true;
            }
            if (waypoint_index == 0 && reverse)
            {
                reverse = false;
            }

            // the next waypoint to go
            if (reverse)
            {
                waypoint_index--;
            }
            else
            {
                waypoint_index++;
            }

            agent.SetDestination(waypoints[waypoint_index]); // sets the new postition to go to.

            bool closeEnough = false;
            AP.Guard_ani.SetBool("IsMoving", true);
            while (!closeEnough) // if close enough then stop walking and repeat turning
            {
                if ((transform.position - waypoints[waypoint_index]).magnitude < 1.0f)
                {
                    closeEnough = true;
                }
                yield return new WaitForSeconds(0.5f);
            }
            AP.Guard_ani.SetBool("IsMoving", false);
        }
    }

    // begins the walking guard's configuration for patrolling.
    protected override void Begin_Patrol()
    {
        Debug.Log("PATROLLING");

        StopCoroutine(Current_Behaviour_Enum);
        Current_Behaviour_Enum = StartCoroutine(WaypointMovement());
    }

    public override void Start_Guard()
    {
        base.Start_Guard();

        BeginWalking();
        StartSuspicion();
    }
}
