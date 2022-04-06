using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    //NEW
    public GameObject floor;

    // grid size
    public int GridSizeX = 6;
    public int GridSizeY = 6;

    // 2D metrix of all floors.
    List<List<FloorNode>> FloorMatrix = new List<List<FloorNode>>();

    public ObjectPool wallObjectPool;


    // indicates the starting node of the floor
    FloorNode CurrentStartNode;

    // the player
    public GameObject Player;

    public GameObject Grate;
    GameObject Current_Grate_Object;

    public GameObject floorHolderParent;


    // current rooms used for floor
    public int RoomsInFloor = 24;

    // mini-map variables for automatic scaling
    public Camera Mini_map_renderer;
    public Vector2 FullRadius = new Vector2(70,70);
    public float FullSize = 35;

    Vector2 minRadius = new Vector2(0, 0);
    Vector2 maxRadius = new Vector2(0, 0);

    // all room nodes used in Prim's algorithm
    public FloorNodeList allPaths;
    public FloorNodeList cornerRooms;
    //List<FloorNode> AllFloorPaths = new List<FloorNode>();

    // used to setup the floor once the scene is built
    void SetupFloor()
    {

        for (int y = 0; y < GridSizeY; y++)
        {
            List<FloorNode> row = new List<FloorNode>();

            for (int x = 0; x < GridSizeX; x++)  // going by rows first...
            {
                FloorNode ptr = Instantiate(floor, floorHolderParent.transform).GetComponent<FloorNode>();
                row.Add(ptr);
                ptr.transform.position = transform.position + new Vector3(10 * x, 0, -10 * y);
                ptr.GridLoc = new Vector2(x, y);
            }

            FloorMatrix.Add(row);
        }

        for(int y = 0; y < GridSizeY; y++)
        {
            for(int x = 0; x < GridSizeX; x++)
            {
                //UP
                if(y != 0)
                {
                    FloorMatrix[y][x].GridConnectors[0] = FloorMatrix[y - 1][x];
                }
                //RIGHT
                if (x != GridSizeX-1)
                {
                    FloorMatrix[y][x].GridConnectors[1] = FloorMatrix[y][x+1];
                }
                //DOWN
                if (y != GridSizeY-1)
                {
                    FloorMatrix[y][x].GridConnectors[2] = FloorMatrix[y + 1][x];
                }
                //LEFT
                if (x != 0)
                {
                    FloorMatrix[y][x].GridConnectors[3] = FloorMatrix[y][x-1];
                }

            }
        }
    }

    void RandomisedPrimsAlgorithm()
    {
        //starts a a random room
        FloorNode start = FloorMatrix[Random.Range(0, GridSizeX - 1)][Random.Range(0, GridSizeY - 1)];
        CurrentStartNode = start;
        CurrentStartNode.IsStartingRoom = true;
        start.Used = true;


        // initialise the unvisited nodes by looking at start location.
        List<FloorNode> UnvisitedNodes = new List<FloorNode>();
        List<FloorNode> UsedNodes = new List<FloorNode>();
        UsedNodes.Add(start);

        // mini-map camera positions (NEED TO CHANGED)
        minRadius = new Vector2(start.transform.position.x, start.transform.position.z);
        maxRadius = new Vector2(start.transform.position.x, start.transform.position.z);

        //randomised Prim's algorithm.
        // the process starts at a node, adds surrounding nodes as unvisited and chooses one of them randomly.
        // the typical algorithm has been extended to connecting paths with it's surrounding and adding wall awareness

        // pre-starts Prim's by adding starting node's surrounding rooms

        for (int i = 0; i < 4; i++)
        {
            if (start.GridConnectors[i] != null)
            {
                UnvisitedNodes.Add(start.GridConnectors[i]);
                start.GridConnectors[i].addedUnvisited = true;
            }

        }

        //start conditions
        bool Condition = true;
        int NumOfRooms = 1;

        while (Condition)
        {

            // getting random element from unvisited nodes
            int idx = Random.Range(0, UnvisitedNodes.Count);
            FloorNode NextAdded = UnvisitedNodes[idx];
            UnvisitedNodes.RemoveAt(idx);

            // for connecting rooms together, all rooms that are used and neighbours are stored
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
            int ExistingFloorDirection = temp[Random.Range(0, temp.Count)];

            //communicates to both nodes which direction the other node is
            NextAdded.PathConnectors[ExistingFloorDirection] = true;

            int reverse = (ExistingFloorDirection + 2) % 4;


            // modifies the connected node to know which directions it is being connected to in relation to NextAdded.
            NextAdded.GridConnectors[ExistingFloorDirection].PathConnectors[reverse] = true;
            NextAdded.Used = true;
            UsedNodes.Add(NextAdded);
            NumOfRooms++;

            //used for camera placement for the minimap
            // checking the minimum and maximum positions of the next added floor node
            if (NextAdded.transform.position.x > maxRadius.x)
            {
                maxRadius.x = NextAdded.transform.position.x;
            }
            if (NextAdded.transform.position.z > maxRadius.y)
            {
                maxRadius.y = NextAdded.transform.position.z;
            }
            if (NextAdded.transform.position.x < minRadius.x)
            {
                minRadius.x = NextAdded.transform.position.x;
            }
            if (NextAdded.transform.position.z < minRadius.y)
            {
                minRadius.y = NextAdded.transform.position.z;
            }


            //find new rooms that are connected to the new node, but not added to the list of unvisited nodes or are used already
            for (int i = 0; i < 4; i++)
            {
                if (NextAdded.GridConnectors[i] != null && !NextAdded.GridConnectors[i].Used && !NextAdded.GridConnectors[i].addedUnvisited)
                {
                    UnvisitedNodes.Add(NextAdded.GridConnectors[i]);
                    NextAdded.GridConnectors[i].addedUnvisited = true;
                }

            }

            // algorithm continues until the roomsInFloor have been met
            if (NumOfRooms == RoomsInFloor)
            {
                Condition = false;
            }
        }
        allPaths.AddList(UsedNodes); // updates AllFloorPaths of floors being used.
    }

    void GenerateFloor()
    {
        // generateFloor recieves data on the condition of the floor
        //Debug.Log("creating floor with:\n" + RoomsInFloor + " rooms\n" + GuardsToStand + " guards to stand\n" + GuardsToWalk + " guards to walk\ntreasure values between " + MinTreasureAmount + " and " + MaxTreasureAmount + "\n");

        RandomisedPrimsAlgorithm();



        // wall and floor management
        // updates the floors in whether they will be used or not
        // instantiates walls for unique AI pathway, while making sure that only one instance of wall exists in a direction between two rooms.

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
                    // if there is no paths connected to the floor node, hide the floor and don't spawn wall.
                    FloorMatrix[a][b].ChangeFloor(false);
                    continue;
                }

                int count = 4;

                GameObject g = null;

                for(int i = 0; i < 4; i++)
                {
                    bool pathNotExist = !FloorMatrix[a][b].PathConnectors[i];
                    bool aRoomIsAdjacent = FloorMatrix[a][b].GridConnectors[i];

                    // if there is no path connected to a specific direction of the node...
                    if (pathNotExist)
                    {
                        count--;
                        // if there is a room in the specific direction...
                        if (aRoomIsAdjacent)
                        {
                            bool wallNotPlaced = !FloorMatrix[a][b].GridConnectors[i].WallPlaced[(i + 2) % 4];
                            // if the room in the UP direction doesn't have a wall...   
                            if (wallNotPlaced)
                            {
                                // then spawn a wall and alert both rooms that there is a wall there. this is to prevent two walls spawning in the same place and reduces resources.
                                g = wallObjectPool.RequestObject();
                                g.transform.position = FloorMatrix[a][b].transform.position;
                                g.transform.Rotate(0, i * 90, 0);
                                FloorMatrix[a][b].GridConnectors[i].WallPlaced[(i + 2) % 4] = true;
                                FloorMatrix[a][b].WallPlaced[i] = true;
                            }
                        }
                        else
                        {
                            //spawn a wall but only alert the current room. this is a boundry room, meaning that there is no rooms beyond that point in that direction.
                            g = wallObjectPool.RequestObject();
                            g.transform.position = FloorMatrix[a][b].transform.position;
                            g.transform.Rotate(0, i * 90, 0);

                            FloorMatrix[a][b].WallPlaced[i] = true;
                        }
                    }
                }

                // if the room only contains 1 pathway, then it is a dead end and a possible place for the treasure
                if(count == 1)
                {
                    cornerRooms.Add(FloorMatrix[a][b]);
                }
            }
        }

    }
    
    // resets floor data like the state of rooms, ALL walls and treasures. but it does not delete the original grid data. as it can be reused
    void ResetFloor()
    {
        for(int a = 0; a < GridSizeX; a++)
        {
            for(int b = 0; b < GridSizeY; b++)
            {
                FloorMatrix[a][b].ResetFloor(); // resets the booleans and contents of the floor
            }
        }

        // destroys all walls
        wallObjectPool.ResetObjectsState();

        allPaths.ClearList();
        cornerRooms.ClearList();

        // destroys all guards
        //foreach(GameObject g in GameObject.FindGameObjectsWithTag("Guard"))
        //{
        //    Destroy(g);
        //}

        //destroys grate object for player to begin in.
        //Destroy(Current_Grate_Object);
        //Current_Grate_Object = null;
        
    }

    // places player at the current start node with a Y offset and instantiate a grate for the player to begin in.
    void PlacePlayer()
    {
        Player.transform.position = CurrentStartNode.transform.position + new Vector3(0, 1.5f);
        Player.GetComponent<Player_Transition>().Player_Disabled();
        Current_Grate_Object = Instantiate(Grate, CurrentStartNode.transform.position, Quaternion.identity);
    }

    // places guards for the floor
    //void PlaceGuards()
    //{
    //    // Guard deployment - Stand - guards that will be given a specific waypoint and rotate
    //    // these guards will only spawn in a room adjacent to a treasure room.
    //    List<FloorNode> PossRooms = new List<FloorNode>();

    //    // this block gets all adjacent rooms to treasure rooms. the dict is used to prevent dulplicates in the list.
    //    Dictionary<FloorNode, FloorNode> test = new Dictionary<FloorNode, FloorNode>(); // used for quick, unique storing
    //    //for (int i = 0; i < TreasureNodes.Count; i++)
    //    //{
    //    //    for (int b = 0; b < 4; b++)
    //    //    {
    //    //        if (TreasureNodes[i].PathConnectors[b])
    //    //        {
    //    //            if(!TreasureNodes[i].GridConnectors[b].IsStartingRoom && !TreasureNodes[i].GridConnectors[b].IsTreasureRoom)
    //    //            {
    //    //                test[TreasureNodes[i].GridConnectors[b]] = TreasureNodes[i].GridConnectors[b];
    //    //            }

    //    //        }
    //    //    }
    //    //}

    //    PossRooms.AddRange(test.Values);

    //    GameObject g;
    //    for (int i = 0; i < GuardsToStand; i++)
    //    {
    //        int chosen = Random.Range(0, PossRooms.Count);
    //        FloorNode node = PossRooms[chosen];
    //        // MOVE THINGS INTO GRAPH
    //        g = Instantiate(GuardObject, PossRooms[chosen].transform.position + new Vector3(0, 1.5f, 0), Quaternion.identity);
    //        g.AddComponent<Standing_Guard>();
    //        g.GetComponent<Standing_Guard>().Start_Guard(); // initialises the guard
    //        g.GetComponent<Standing_Guard>().StandingPoint = PossRooms[chosen].transform.position + new Vector3(0, 1.5f, 0); // gives a guard a standing point for the floor
    //        g.GetComponent<Standing_Guard>().Stand = true; // parent identifier on what type guard is
    //        g.GetComponent<Standing_Guard>().BeginTurning(); // starts the guard's AI movement
    //        g.GetComponent<Standing_Guard>().StartSuspicion(); // starting the guard's AI behaviour
    //        PossRooms.RemoveAt(chosen); // removes the rooms so no duplicate guard.

    //    }

    //    //Guard deployment - walking - guards that will be assigned waypoints to rotate and move about.
    //    // these guards can spawn anywhere within the used rooms.

    //    List<FloorNode> FloorPaths = allPaths.GetList();

    //    for(int b = 0; b < GuardsToWalk; b++)
    //    {
    //        g = Instantiate(GuardObject);
    //        g.AddComponent<Walking_Guard>();
    //        g.GetComponent<Walking_Guard>().Waypoint = true;

    //        if(FloorPaths.Count < wayPointsInPath) // fail safe if there is not enough rooms to place a guard (given the amount of waypoints needed)
    //        {
    //            break;
    //        }

    //        for (int i = 0; i < wayPointsInPath; i++) // chooses a number of waypoints (floor nodes) to ping-pong to.
    //        {
    //            int chos = Random.Range(0, FloorPaths.Count);


    //            g.GetComponent<Walking_Guard>().waypoints.Add(FloorPaths[chos].transform.position + new Vector3(0, 1.5f, 0));
    //            FloorPaths.RemoveAt(chos);


    //        }
    //        g.GetComponent<Walking_Guard>().Start_Guard(); // initialises the guard
    //        g.GetComponent<Walking_Guard>().BeginWalking(); // start's the guard's AI movement
    //        g.GetComponent<Walking_Guard>().StartSuspicion(); // start's the guard's AI behaviour
    //    }

    //}

    // adjusts the ortho camera to fit all used floors into the render texture (mini-map)
    // maths is based on manual values of floor node radius, max floor size and max camera size
    public void AdjustMapRenderer()
    {

        FullRadius = new Vector2(GridSizeX, GridSizeY) * 10;
        
        // newPos is the new position of the camera
        Vector2 newPos = (maxRadius - minRadius) / 2;

        // diff calculates the size needed for the render texture
        Vector2 diff = maxRadius - minRadius + new Vector2(10,10);
        diff /= FullRadius;

        Debug.Log("scale camera by: " + Mathf.Max(diff.x, diff.y));
        
        Mini_map_renderer.orthographicSize = FullSize * Mathf.Max(diff.x, diff.y) + 1; //affect fullSize by the max percentage of difference and add 1 for border
        Mini_map_renderer.transform.position = new Vector3(newPos.x+minRadius.x, 60.0f, newPos.y+minRadius.y); // places camera in middle of new floor
    }

    // Start is called before the first frame update
    void Start()
    {
        SetupFloor();
    }

    // resets the floor back to it's initial state
    public void Reset_Floor_Level()
    {
        ResetFloor();
    }

    // creates a new floor with walls, guards and treasure
    public void Create_Floor_Level()
    {
        GenerateFloor();
        AdjustMapRenderer();
        //AddTreasureRooms(currentCornerRooms);
        //PlacePlayer();
        //PlaceGuards();
    }
}
