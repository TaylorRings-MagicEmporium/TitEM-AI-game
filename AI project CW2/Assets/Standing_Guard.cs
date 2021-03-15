using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Standing_Guard : Guard
{
    public Vector3 StandingPoint;

    public void BeginTurning()
    {
        Current_Behaviour_Enum = StartCoroutine(WaypointTurning());
    }

    IEnumerator WaypointTurning()
    {
        while (true)
        {
            //while (closeEnough())
            //{
            //    agent.SetDestination(StandingPoint);
            //    yield return new WaitForSeconds(0.5f);
            //}

            turning = true;

            rotateCounter = 0.0f;
            current = transform.rotation;
            target = current * Quaternion.Euler(0, 90f, 0);

            yield return new WaitForSeconds(3f);
            //turning = false;
        }
    }

    protected override void Begin_Patrol()
    {
        Debug.Log("PATROLLING");
        StopCoroutine(Current_Behaviour_Enum);
        Current_Behaviour_Enum = StartCoroutine(WaypointTurning());
    }

    bool closeEnough()
    {
        if(Vector3.Distance(transform.position, StandingPoint) < 0.1f)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
}
