using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorNode : MonoBehaviour
{
    public Vector2 GridLoc;

    public bool[] PathConnectors = new bool[] { false, false, false, false }; // 
    public FloorNode[] GridConnectors = new FloorNode[4];

    public bool[] WallPlaced = new bool[] { false, false, false, false };

    MeshRenderer floor;

    public bool addedUnvisited = false;
    public bool Used = false;

    private void Awake()
    {

        floor = transform.parent.GetComponent<MeshRenderer>();

        RaycastHit hit;
        if(Physics.Raycast(transform.position,new Vector3(0,0,1),out hit))
        {
            GridConnectors[0] = hit.transform.GetComponent<FloorNode>();
        }
        if (Physics.Raycast(transform.position, new Vector3(1, 0, 0), out hit))
        {
            GridConnectors[1] = hit.transform.GetComponent<FloorNode>();
        }
        if (Physics.Raycast(transform.position, new Vector3(0, 0, -1), out hit))
        {
            GridConnectors[2] = hit.transform.GetComponent<FloorNode>();
        }
        if (Physics.Raycast(transform.position, new Vector3(-1, 0, 0), out hit))
        {
            GridConnectors[3] = hit.transform.GetComponent<FloorNode>();
        }
    }

    public void ChangeFloor(bool ch)
    {
        floor.enabled = ch;
    }

    public void ResetFloor()
    {
        PathConnectors = new bool[] { false, false, false, false };
        WallPlaced = new bool[] { false, false, false, false };
        addedUnvisited = false;
        Used = false;
        ChangeFloor(true);
    }
}
