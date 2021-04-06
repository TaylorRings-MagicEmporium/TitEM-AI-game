using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Standing_Guard : Guard
{
    public Vector3 StandingPoint;

    // begins the Guard's movement for standing (for PATROL state)
    public void BeginTurning()
    {
        Current_Behaviour_Enum = StartCoroutine(WaypointTurning());
    }

    // describes how the Guard rotates
    IEnumerator WaypointTurning()
    {
        while (true)
        {
            turning = true;

            rotateCounter = 0.0f;
            current = transform.rotation;
            target = current * Quaternion.Euler(0, 90f, 0);

            yield return new WaitForSeconds(3f);
        }
    }

    // begins the standing guard's configuration for patrolling.
    protected override void Begin_Patrol()
    {
        Debug.Log("PATROLLING");
        StopCoroutine(Current_Behaviour_Enum);
        Current_Behaviour_Enum = StartCoroutine(WaypointTurning());
    }
}
