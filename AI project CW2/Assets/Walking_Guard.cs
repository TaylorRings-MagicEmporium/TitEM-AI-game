using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Walking_Guard : Guard
{
    public List<Vector3> waypoints = new List<Vector3>();
    public int waypoint_index = 0;

    public void BeginWalking()
    {
        //Debug.Log(transform.position);
        //Debug.Log("1");
        agent.enabled = false;
        transform.position = waypoints[0];
        //Debug.Log(waypoints[0]);

        transform.rotation = Quaternion.identity;
        agent.enabled = true;
        Current_Behaviour_Enum = StartCoroutine(WaypointMovement());
    }



    IEnumerator WaypointMovement()
    {
        bool reverse = false;
        while (true)
        {
            //Debug.Log("2");
            turning = true;

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
            turning = true;
            yield return new WaitForSeconds(1f);
            turning = false;

            if (waypoint_index == waypoints.Count - 1 && !reverse)
            {
                reverse = true;
            }
            if (waypoint_index == 0 && reverse)
            {
                reverse = false;
            }

            if (reverse)
            {
                waypoint_index--;
            }
            else
            {
                waypoint_index++;
            }

            agent.SetDestination(waypoints[waypoint_index]);
            //agent.destination

            bool closeEnough = false;
            AP.Guard_ani.SetBool("IsMoving", true);
            while (!closeEnough)
            {
                //Debug.Log((transform.position - waypoints[waypoint_index]).magnitude);
                if ((transform.position - waypoints[waypoint_index]).magnitude < 1.0f)
                {
                    closeEnough = true;
                }
                yield return new WaitForSeconds(0.5f);
            }
            AP.Guard_ani.SetBool("IsMoving", false);
        }
    }

    protected override void Begin_Patrol()
    {
        Debug.Log("PATROLLING");

        StopCoroutine(Current_Behaviour_Enum);
        Current_Behaviour_Enum = StartCoroutine(WaypointMovement());
    }
}
