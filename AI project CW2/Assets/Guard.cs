using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class Guard : MonoBehaviour
{
    public bool Stand = false;
    public bool Waypoint = false;

    public List<Vector3> waypoints = new List<Vector3>();
    public int waypoint_index = 0;

    NavMeshAgent agent;

    public void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public void BeginWalking()
    {
        //Debug.Log(transform.position);
        //Debug.Log("1");
        agent.enabled = false;
        transform.position = waypoints[0];
        Debug.Log(waypoints[0]);
        
        transform.rotation = Quaternion.identity;
        agent.enabled = true;
        StartCoroutine(WaypointMovement());
    }

    IEnumerator WaypointMovement()
    {
        bool reverse = false;
        while (true)
        {
            //Debug.Log("2");
            yield return new WaitForSeconds(3f);

            if(waypoint_index == waypoints.Count - 1 && !reverse)
            {
                reverse = true;
            }
            if(waypoint_index == 0 && reverse)
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
            while (!closeEnough)
            {
                //Debug.Log((transform.position - waypoints[waypoint_index]).magnitude);
                if ((transform.position - waypoints[waypoint_index]).magnitude < 1.0f)
                {
                    closeEnough = true;
                }
                yield return new WaitForSeconds(0.5f);
            }
        }


    }
}
