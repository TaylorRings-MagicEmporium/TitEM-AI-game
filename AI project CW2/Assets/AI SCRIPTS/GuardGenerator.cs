using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardGenerator : MonoBehaviour
{
    public GameObject GuardObject;
    // specific guard info
    public int GuardsToStand = 1;
    public int GuardsToWalk = 2;
    public int wayPointsInPath = 3;

    public FloorNodeList allTreasureRooms;
    public FloorNodeList allPaths;

    public List<SOGuardData> allGuardData = new List<SOGuardData>();
    public void PlaceGuards()
    {
        if(allGuardData.Count == 0)
        {
            Debug.LogError("ERROR: No Guard data to use! Consider making and filling the list!", this);
            return;
        }

        // Guard deployment - Stand - guards that will be given a specific waypoint and rotate
        // these guards will only spawn in a room adjacent to a treasure room.
        List<FloorNode> PossRooms = new List<FloorNode>();

        List<FloorNode> treasureRooms = allTreasureRooms.GetList();

        // this block gets all adjacent rooms to treasure rooms. the dict is used to prevent dulplicates in the list.
        Dictionary<FloorNode, FloorNode> test = new Dictionary<FloorNode, FloorNode>(); // used for quick, unique storing
        for (int i = 0; i < treasureRooms.Count; i++)
        {
            for (int b = 0; b < 4; b++)
            {
                if (treasureRooms[i].PathConnectors[b])
                {
                    if (!treasureRooms[i].GridConnectors[b].IsStartingRoom && !treasureRooms[i].GridConnectors[b].IsTreasureRoom)
                    {
                        test[treasureRooms[i].GridConnectors[b]] = treasureRooms[i].GridConnectors[b];
                    }

                }
            }
        }

        PossRooms.AddRange(test.Values);

        GameObject g;
        for (int i = 0; i < GuardsToStand; i++)
        {
            if(PossRooms.Count == 0)
            {
                Debug.LogWarning("WARNING: not enough possible rooms avaliable, stopping standing guards at: " + i, this);
                break;
            }

            int chosen = Random.Range(0, PossRooms.Count);
            FloorNode node = PossRooms[chosen];
            // MOVE THINGS INTO GRAPH
            g = Instantiate(GuardObject, PossRooms[chosen].transform.position + new Vector3(0, 1.5f, 0), Quaternion.identity);
            g.GetComponent<Guard>().waypoints.Add(PossRooms[chosen].transform.position + new Vector3(0, 1.5f, 0)); // gives a guard a standing point for the floor
            //g.GetComponent<Guard>().Stand = true; // parent identifier on what type guard is
            g.GetComponent<Guard>().guardData = Instantiate(allGuardData[Random.Range(0, allGuardData.Count)]);
            g.GetComponent<Guard>().Start_Guard(); // initialises the guard
            PossRooms.RemoveAt(chosen); // removes the rooms so no duplicate guard.

        }

        //Guard deployment - walking - guards that will be assigned waypoints to rotate and move about.
        // these guards can spawn anywhere within the used rooms.

        List<FloorNode> FloorPaths = allPaths.GetList();

        for (int b = 0; b < GuardsToWalk; b++)
        {
            if (FloorPaths.Count < wayPointsInPath) // fail safe if there is not enough rooms to place a guard (given the amount of waypoints needed)
            {
                Debug.LogWarning("WARNING: not enough possible rooms avaliable", this);
                break;
            }

            g = Instantiate(GuardObject);
            //g.GetComponent<Guard>().Waypoint = true;
            g.GetComponent<Guard>().guardData = Instantiate(allGuardData[Random.Range(0, allGuardData.Count)]);

            for (int i = 0; i < wayPointsInPath; i++) // chooses a number of waypoints (floor nodes) to ping-pong to.
            {
                int chos = Random.Range(0, FloorPaths.Count);


                g.GetComponent<Guard>().waypoints.Add(FloorPaths[chos].transform.position + new Vector3(0, 1.5f, 0));
                FloorPaths.RemoveAt(chos);


            }
            g.GetComponent<Guard>().Start_Guard(); // initialises the guard
        }

    }

    public void ResetGuards()
    {
        // destroys all guards
        foreach (GameObject g in GameObject.FindGameObjectsWithTag("Guard"))
        {
            Destroy(g);
        }
    }
}
