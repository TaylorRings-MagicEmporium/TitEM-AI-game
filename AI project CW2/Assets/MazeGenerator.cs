using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    // grid size
    public int GridSizeX = 6;
    public int GridSizeY = 6;

    // 2D metrix of all floors.
    List<List<FloorNode>> FloorMatrix = new List<List<FloorNode>>();

    // indecates the starting node of the grid
    public FloorNode Element_0_0;

    // wall data types to record walls and put under one parent.
    List<GameObject> AllWalls = new List<GameObject>();
    public GameObject AllWallPoint;

    // prefabs of walls based on the direction/
    public GameObject Up_Wall;
    public GameObject Right_Wall;
    public GameObject Down_Wall;
    public GameObject Left_Wall;
    public GameObject Treasure;

    // indicates the starting node of the floor
    FloorNode CurrentStartNode;

    // the player
    public GameObject Player;
    public GameObject GuardObject;
    public GameObject Grate;
    GameObject Current_Grate_Object;

    // treasure rooms contain treasure. might move to a manager for this maybe?
    public int TreasureRooms = 3;
    public List<FloorNode> TreasureNodes = new List<FloorNode>();
    List<GameObject> TreasureItems = new List<GameObject>();

    public int GuardsToStand = 1;
    public int GuardsToWalk = 2;
    public int wayPointsInPath = 3;

    public int RoomsInFloor = 24;

    enum Dir
    {
        UP, RIGHT, DOWN, LEFT
    };

    List<FloorNode> AllFloorPaths = new List<FloorNode>();

    // used to setup the floor once the scene is built
    void SetupFloor()
    {
        List<GameObject> AllFloorConnectors = new List<GameObject>();
        // right now, the rooms know which rooms are connected via the FloorNode awake function.
        // however, they don't know their grid positions.
        AllFloorConnectors.AddRange(GameObject.FindGameObjectsWithTag("FloorCon"));

        // remove colliders once grid setup is done.
        for (int i = 0; i < AllFloorConnectors.Count; i++)
        {
            AllFloorConnectors[i].GetComponent<Collider>().enabled = false;
        }
        FloorNode Pointer = Element_0_0;

        // constructs a data-esque representation of the grid.
        for (int y = 0; y < GridSizeY; y++)
        {
            FloorNode RowStart = Pointer;
            List<FloorNode> row = new List<FloorNode>();
            for (int x = 0; x < GridSizeX; x++)
            {
                row.Add(Pointer);
                Pointer.GridLoc = new Vector2(x, y);
                Pointer = Pointer.GridConnectors[(int)Dir.RIGHT];
            }

            Pointer = RowStart.GridConnectors[(int)Dir.UP];
            FloorMatrix.Add(row);
        }
    }

    void GenerateFloor()
    {
        //starts a a random room
        FloorNode start = FloorMatrix[Random.Range(0, 6)][Random.Range(0, 6)];
        //CurrentStartNode = start;
        start.Used = true;


        // initialise the unvisited nodes by looking at start location.
        List<FloorNode> UnvisitedNodes = new List<FloorNode>();
        List<FloorNode> UsedNodes = new List<FloorNode>();
        UsedNodes.Add(start);
        for (int i = 0; i < 4; i++)
        {
            if (start.GridConnectors[i] != null)
            {
                UnvisitedNodes.Add(start.GridConnectors[i]);
                start.GridConnectors[i].addedUnvisited = true;
            }

        }

        //randomised Prim's algorithm
        bool Condition = true;
        int NumOfRooms = 1;
        while (Condition)
        {

            // getting random element from unvisited nodes
            int idx = Random.Range(0, UnvisitedNodes.Count);
            FloorNode NextAdded = UnvisitedNodes[idx];
            UnvisitedNodes.RemoveAt(idx);
            //Debug.Log(NextAdded.GridLoc);

            // records the direction of used nodes of the next to be added
            List<int> temp = new List<int>();
            for (int i = 0; i < 4; i++)
            {
                if (NextAdded.GridConnectors[i])
                {
                    if (NextAdded.GridConnectors[i].Used)
                    {
                        temp.Add(i);
                    }
                }

            }
            // choose random direction that the node would connect to.
            int ToConnect = temp[Random.Range(0, temp.Count)];

            //communicates to both nodes which direction the other node is
            NextAdded.PathConnectors[ToConnect] = true;
            int reverse = -1;
            // gets the direction of the node relative to the "used" node  
            switch (ToConnect)
            {
                case 0:
                    reverse = 2;
                    break;

                case 1:
                    reverse = 3;
                    break;

                case 2:
                    reverse = 0;
                    break;

                case 3:
                    reverse = 1;
                    break;
            }
            NextAdded.GridConnectors[ToConnect].PathConnectors[reverse] = true;
            NextAdded.Used = true;
            UsedNodes.Add(NextAdded);
            NumOfRooms++;

            //finds rooms that have are connected to the new node, but not added to the list of unvisited nodes
            for (int i = 0; i < 4; i++)
            {
                if (NextAdded.GridConnectors[i] != null && !NextAdded.GridConnectors[i].Used && !NextAdded.GridConnectors[i].addedUnvisited)
                {
                    UnvisitedNodes.Add(NextAdded.GridConnectors[i]);
                    NextAdded.GridConnectors[i].addedUnvisited = true;
                }

            }

            if (NumOfRooms >= RoomsInFloor)
            {
                Condition = false;
            }

        }

        List<FloorNode> PossiblePlayerStarts = new List<FloorNode>();

        // wall and floor management
        for (int a = 0; a < GridSizeX; a++)
        {
            for (int b = 0; b < GridSizeY; b++)
            {

                bool hide = true;
                for (int i = 0; i < 4; i++)
                {
                    if (FloorMatrix[a][b].PathConnectors[i])
                    {
                        hide = false;
                        break;
                    }
                }
                if (hide)
                {
                    // if there is no paths connected to the room, hide the floor and don't spawn wall.
                    FloorMatrix[a][b].ChangeFloor(false);
                    continue;
                }

                int count = 4;

                GameObject g = null;
                // if there is no path connected to the UP direction of the node...
                if (!FloorMatrix[a][b].PathConnectors[0])
                {
                    count--;
                    // if there is a room in the UP direction...
                    if (FloorMatrix[a][b].GridConnectors[0])
                    {
                        // if the room in the UP direction doesn't have a wall...   
                        if (!FloorMatrix[a][b].GridConnectors[0].WallPlaced[2])
                        {
                            // then spawn a wall and alert both rooms that there is a wall there. this is to prevent to walls spawning in the same place and reduces resources.
                            g = Instantiate(Up_Wall, FloorMatrix[a][b].transform.parent.position, Quaternion.identity);

                            FloorMatrix[a][b].GridConnectors[0].WallPlaced[2] = true;
                            FloorMatrix[a][b].WallPlaced[0] = true;

                            g.transform.parent = AllWallPoint.transform;
                            AllWalls.Add(g);
                        }
                    }
                    else
                    {
                        //spawn a wall but only alert the current room. this is a boundry room, meaning that there is no rooms beyond that point in that direction.
                        g = Instantiate(Up_Wall, FloorMatrix[a][b].transform.parent.position, Quaternion.identity);
                        FloorMatrix[a][b].WallPlaced[0] = true;

                        g.transform.parent = AllWallPoint.transform;
                        AllWalls.Add(g);
                    }
                }
                // if there is no path connected to the RIGHT direction of the node...
                if (!FloorMatrix[a][b].PathConnectors[1])
                {
                    count--;
                    // if there is a room in the RIGHT direction...
                    if (FloorMatrix[a][b].GridConnectors[1])
                    {
                        // if the room in the RIGHT direction doesn't have a wall...   
                        if (!FloorMatrix[a][b].GridConnectors[1].WallPlaced[3])
                        {
                            // then spawn a wall and alert both rooms that there is a wall there. this is to prevent to walls spawning in the same place and reduces resources.
                            g = Instantiate(Right_Wall, FloorMatrix[a][b].transform.parent.position, Quaternion.identity);
                            FloorMatrix[a][b].GridConnectors[1].WallPlaced[3] = true;
                            FloorMatrix[a][b].WallPlaced[1] = true;

                            g.transform.parent = AllWallPoint.transform;
                            AllWalls.Add(g);
                        }
                    }
                    else
                    {
                        //spawn a wall but only alert the current room. this is a boundry room, meaning that there is no rooms beyond that point in that direction.
                        g = Instantiate(Right_Wall, FloorMatrix[a][b].transform.parent.position, Quaternion.identity);
                        FloorMatrix[a][b].WallPlaced[1] = true;

                        g.transform.parent = AllWallPoint.transform;
                        AllWalls.Add(g);
                    }

                }
                // if there is no path connected to the DOWN direction of the node...
                if (!FloorMatrix[a][b].PathConnectors[2])
                {
                    count--;
                    // if there is a room in the DOWN direction...
                    if (FloorMatrix[a][b].GridConnectors[2])
                    {
                        // if the room in the DOWN direction doesn't have a wall...   
                        if (!FloorMatrix[a][b].GridConnectors[2].WallPlaced[0])
                        {
                            // then spawn a wall and alert both rooms that there is a wall there. this is to prevent to walls spawning in the same place and reduces resources.
                            g = Instantiate(Down_Wall, FloorMatrix[a][b].transform.parent.position, Quaternion.identity);
                            FloorMatrix[a][b].GridConnectors[2].WallPlaced[0] = true;
                            FloorMatrix[a][b].WallPlaced[2] = true;

                            g.transform.parent = AllWallPoint.transform;
                            AllWalls.Add(g);
                        }
                    }
                    else
                    {
                        //spawn a wall but only alert the current room. this is a boundry room, meaning that there is no rooms beyond that point in that direction.
                        g = Instantiate(Down_Wall, FloorMatrix[a][b].transform.parent.position, Quaternion.identity);
                        FloorMatrix[a][b].WallPlaced[2] = true;

                        g.transform.parent = AllWallPoint.transform;
                        AllWalls.Add(g);
                    }
                }
                // if there is no path connected to the LEFT direction of the node...
                if (!FloorMatrix[a][b].PathConnectors[3])
                {
                    count--;
                    // if there is a room in the LEFT direction...
                    if (FloorMatrix[a][b].GridConnectors[3])
                    {
                        // if the room in the LEFT direction doesn't have a wall...   
                        if (!FloorMatrix[a][b].GridConnectors[3].WallPlaced[1])
                        {
                            // then spawn a wall and alert both rooms that there is a wall there. this is to prevent to walls spawning in the same place and reduces resources.
                            g = Instantiate(Left_Wall, FloorMatrix[a][b].transform.parent.position, Quaternion.identity);
                            FloorMatrix[a][b].GridConnectors[3].WallPlaced[1] = true;
                            FloorMatrix[a][b].WallPlaced[3] = true;

                            g.transform.parent = AllWallPoint.transform;
                            AllWalls.Add(g);
                        }
                    }
                    else
                    {
                        //spawn a wall but only alert the current room. this is a boundry room, meaning that there is no rooms beyond that point in that direction.
                        g = Instantiate(Left_Wall, FloorMatrix[a][b].transform.parent.position, Quaternion.identity);
                        FloorMatrix[a][b].WallPlaced[3] = true;

                        g.transform.parent = AllWallPoint.transform;
                        AllWalls.Add(g);
                    }
                }

                // if the room only contains 1 pathway, then it is a dead end and a possible place for the player
                if(count == 1)
                {
                    PossiblePlayerStarts.Add(FloorMatrix[a][b]);
                }
            }
        }

        AllFloorPaths = new List<FloorNode>(UsedNodes);

        // the start location is randomised.
        int ranStartPos = Random.Range(0, PossiblePlayerStarts.Count);
        CurrentStartNode = PossiblePlayerStarts[ranStartPos];

        AllFloorPaths.Remove(CurrentStartNode);
        PossiblePlayerStarts.RemoveAt(ranStartPos);

        CurrentStartNode.IsStartingRoom = true;
        // deploying treasures by picking a random room
        for(int i = 0; i < TreasureRooms; i++)
        {

            int idx = Random.Range(0, PossiblePlayerStarts.Count);

            //int idx = Random.Range(0, UsedNodes.Count);
            //if(UsedNodes[idx] == CurrentStartNode)
            //{
            //    UsedNodes.RemoveAt(idx);
            //    idx = Random.Range(0, UsedNodes.Count);
            //}

            Debug.Log(PossiblePlayerStarts[idx].GridLoc);
            TreasureNodes.Add(PossiblePlayerStarts[idx]);
            TreasureNodes[i].IsTreasureRoom = true;

            GameObject g = Instantiate(Treasure, TreasureNodes[i].transform.parent.position, Quaternion.identity);
            g.GetComponent<Treasure_Info>().NodeLoc = TreasureNodes[i];
            TreasureItems.Add(g);
            PossiblePlayerStarts.RemoveAt(idx);
        }

    }
    
    // resets fllod data like the state of rooms, ALL walls and treasures. but it does not delete the original grid data.
    void ResetFloor()
    {
        for(int a = 0; a < GridSizeX; a++)
        {
            for(int b = 0; b < GridSizeY; b++)
            {
                FloorMatrix[a][b].ResetFloor();
            }
        }

        foreach(GameObject g in AllWalls)
        {
            Destroy(g);
        }
        AllWalls.Clear();

        foreach(GameObject g in TreasureItems)
        {
            Destroy(g);
        }
        TreasureItems.Clear();
        TreasureNodes.Clear();

        foreach(GameObject g in GameObject.FindGameObjectsWithTag("Guard"))
        {
            Destroy(g);
        }

        Destroy(Current_Grate_Object);
        Current_Grate_Object = null;
        
    }

    // places player at the current start node with a Y offset.
    void PlacePlayer()
    {
        Player.transform.position = CurrentStartNode.transform.parent.position + new Vector3(0, 1.5f);
        Player.GetComponent<Player_Transition>().Player_Disabled();
        Current_Grate_Object = Instantiate(Grate, CurrentStartNode.transform.parent.position, Quaternion.identity);
    }

    void PlaceGuards()
    {
        // Guard deployment - Stand
        List<FloorNode> PossRooms = new List<FloorNode>();
        Dictionary<FloorNode, FloorNode> test = new Dictionary<FloorNode, FloorNode>();
        for (int i = 0; i < TreasureNodes.Count; i++)
        {
            for (int b = 0; b < 4; b++)
            {
                if (TreasureNodes[i].PathConnectors[b])
                {
                    if(!TreasureNodes[i].GridConnectors[b].IsStartingRoom && !TreasureNodes[i].GridConnectors[b].IsTreasureRoom)
                    {
                        test[TreasureNodes[i].GridConnectors[b]] = TreasureNodes[i].GridConnectors[b];
                    }

                }
            }
        }

        PossRooms.AddRange(test.Values);

        GameObject g;
        for (int i = 0; i < GuardsToStand; i++)
        {
            int chosen = Random.Range(0, PossRooms.Count);
            FloorNode node = PossRooms[chosen];
            g = Instantiate(GuardObject, PossRooms[chosen].transform.parent.position + new Vector3(0, 1.5f, 0), Quaternion.identity);
            g.AddComponent<Standing_Guard>();
            g.GetComponent<Standing_Guard>().Start();
            g.GetComponent<Standing_Guard>().StandingPoint = PossRooms[chosen].transform.parent.position + new Vector3(0, 1.5f, 0);
            g.GetComponent<Standing_Guard>().Stand = true;
            g.GetComponent<Standing_Guard>().BeginTurning();
            g.GetComponent<Standing_Guard>().StartSuspision();
            PossRooms.RemoveAt(chosen);

        }

        //Guard deployment - walking

        List<FloorNode> FloorPaths = new List<FloorNode>(AllFloorPaths);

        for(int b = 0; b < GuardsToWalk; b++)
        {
            g = Instantiate(GuardObject);
            g.AddComponent<Walking_Guard>();
            g.GetComponent<Walking_Guard>().Waypoint = true;

            for (int i = 0; i < wayPointsInPath; i++)
            {
                int chos = Random.Range(0, FloorPaths.Count);


                g.GetComponent<Walking_Guard>().waypoints.Add(FloorPaths[chos].transform.parent.position + new Vector3(0, 1.5f, 0));
                FloorPaths.RemoveAt(chos);


            }
            //g.transform.position = g.GetComponent<Guard>().waypoints[0];
            //g.transform.rotation = Quaternion.identity;
            g.GetComponent<Walking_Guard>().Start();
            g.GetComponent<Walking_Guard>().BeginWalking();
            g.GetComponent<Walking_Guard>().StartSuspision();
        }


       
    }

    // Start is called before the first frame update
    void Start()
    {
        SetupFloor();
    }

    public void Create_Floor_Level()
    {
        ResetFloor();
        GenerateFloor();
        PlacePlayer();
        PlaceGuards();
    }
}
