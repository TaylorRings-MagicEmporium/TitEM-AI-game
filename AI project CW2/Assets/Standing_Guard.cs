using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Standing_Guard : Guard
{
    public Vector3 StandingPoint;

    public void BeginTurning()
    {
        StartCoroutine(WaypointTurning());
    }

    IEnumerator WaypointTurning()
    {
        while (true)
        {
            //Debug.Log("2");
            turning = true;

            rotateCounter = 0.0f;
            current = transform.rotation;
            target = current * Quaternion.Euler(0, 90f, 0);

            yield return new WaitForSeconds(3f);
            //turning = false;
        }
    }
}
