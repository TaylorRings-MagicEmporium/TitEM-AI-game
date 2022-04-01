using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorNode : MonoBehaviour
{
    // position based on the grid location
    public Vector2 GridLoc;

    // boolean array to determine if a path is connected to itself and in what direction.
    public bool[] PathConnectors = new bool[] { false, false, false, false };

    // initially, records what floorNodes are connected to itself and in what direction.
    public FloorNode[] GridConnectors = new FloorNode[4];

    // records whether a wall have been placed in what direction.
    public bool[] WallPlaced = new bool[] { false, false, false, false };

    // used to hide the floor.
    MeshRenderer floor;
    // bool states to make finding specific rooms quicker.
    public bool addedUnvisited = false;
    public bool Used = false;
    public bool IsTreasureRoom = false;
    //public bool TresureTaken = false;
    public bool IsStartingRoom = false;

    // locates which floorNodes are "linked" to itself based on a rectangle of floorNodes.
    private void Awake()
    {

        floor = transform.GetComponent<MeshRenderer>();

        //// raycast is used to explicitly find rooms next to eachother.
        //RaycastHit hit;
        //if(Physics.Raycast(transform.position,new Vector3(0,0,1),out hit))
        //{
        //    GridConnectors[0] = hit.transform.GetComponent<FloorNode>();
        //}
        //if (Physics.Raycast(transform.position, new Vector3(1, 0, 0), out hit))
        //{
        //    GridConnectors[1] = hit.transform.GetComponent<FloorNode>();
        //}
        //if (Physics.Raycast(transform.position, new Vector3(0, 0, -1), out hit))
        //{
        //    GridConnectors[2] = hit.transform.GetComponent<FloorNode>();
        //}
        //if (Physics.Raycast(transform.position, new Vector3(-1, 0, 0), out hit))
        //{
        //    GridConnectors[3] = hit.transform.GetComponent<FloorNode>();
        //}
    }

    // changes the state of the floor depending if it's connected or not.
    public void ChangeFloor(bool ch)
    {
        floor.enabled = ch;
    }

    //resets variables linked to single floor data like paths and walls. however, it does not delete grid connectors as that does not change.
    public void ResetFloor()
    {
        PathConnectors = new bool[] { false, false, false, false };
        WallPlaced = new bool[] { false, false, false, false };
        addedUnvisited = false;
        Used = false;
        IsTreasureRoom = false;
        IsStartingRoom = false;
        ChangeFloor(true);
    }
}
