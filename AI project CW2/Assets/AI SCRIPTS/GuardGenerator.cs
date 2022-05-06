using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// places guards in specific rooms of the level. Chooses different guard data for unique combinations. Part of the maze generation process.
/// </summary>
public class GuardGenerator : MonoBehaviour
{
    public GameObject GuardObject;
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

        List<FloorNode> PossibleGuardPlaces = new List<FloorNode>();
        List<FloorNode> treasureRooms = allTreasureRooms.GetList();

        Dictionary<FloorNode, FloorNode> UniqueRooms = new Dictionary<FloorNode, FloorNode>();
        for (int i = 0; i < treasureRooms.Count; i++)
        {
            for (int b = 0; b < 4; b++)
            {
                if (treasureRooms[i].PathConnectors[b])
                {
                    if (!treasureRooms[i].GridConnectors[b].IsStartingRoom && !treasureRooms[i].GridConnectors[b].IsTreasureRoom)
                    {
                        UniqueRooms[treasureRooms[i].GridConnectors[b]] = treasureRooms[i].GridConnectors[b];
                    }
                }
            }
        }

        PossibleGuardPlaces.AddRange(UniqueRooms.Values);

        GameObject guardObject;
        for (int i = 0; i < GuardsToStand; i++)
        {
            if(PossibleGuardPlaces.Count == 0)
            {
                Debug.LogWarning("WARNING: not enough possible rooms avaliable, stopping standing guards at: " + i, this);
                break;
            }

            int chosen = Random.Range(0, PossibleGuardPlaces.Count);
            FloorNode node = PossibleGuardPlaces[chosen];
            // MOVE THINGS INTO GRAPH
            guardObject = Instantiate(GuardObject, PossibleGuardPlaces[chosen].transform.position + new Vector3(0, 1.5f, 0), Quaternion.identity);
            guardObject.GetComponent<Guard>().waypoints.Add(PossibleGuardPlaces[chosen].transform.position + new Vector3(0, 1.5f, 0));
            guardObject.GetComponent<Guard>().guardData = Instantiate(allGuardData[Random.Range(0, allGuardData.Count)]);
            guardObject.GetComponent<Guard>().Start_Guard();
            PossibleGuardPlaces.RemoveAt(chosen);

        }

        List<FloorNode> FloorPaths = allPaths.GetList();

        for (int b = 0; b < GuardsToWalk; b++)
        {
            if (FloorPaths.Count < wayPointsInPath) // fail safe if there is not enough rooms to place a guard (given the amount of waypoints needed)
            {
                Debug.LogWarning("WARNING: not enough possible rooms avaliable", this);
                break;
            }

            guardObject = Instantiate(GuardObject);
            guardObject.GetComponent<Guard>().guardData = Instantiate(allGuardData[Random.Range(0, allGuardData.Count)]);

            for (int i = 0; i < wayPointsInPath; i++)
            {
                int chos = Random.Range(0, FloorPaths.Count);
                guardObject.GetComponent<Guard>().waypoints.Add(FloorPaths[chos].transform.position + new Vector3(0, 1.5f, 0));
                FloorPaths.RemoveAt(chos);
            }
            guardObject.GetComponent<Guard>().Start_Guard();
        }

    }

    /// <summary>
    /// Removes the guards from the level. Needs reforming
    /// </summary>
    public void ResetGuards()
    {
        // destroys all guards
        foreach (GameObject g in GameObject.FindGameObjectsWithTag("Guard"))
        {
            Destroy(g);
        }
    }
}
