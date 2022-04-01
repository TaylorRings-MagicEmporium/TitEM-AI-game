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

    // treasure rooms contain treasure.
    public int TreasureRooms = 3;
    public List<FloorNode> TreasureNodes = new List<FloorNode>();
    List<GameObject> TreasureItems = new List<GameObject>();
    public List<GameObject> TreasureModels = new List<GameObject>();

    public int MinTreasureAmount = 0;
    public int MaxTreasureAmount = 0;

    // specific guard info
    public int GuardsToStand = 1;
    public int GuardsToWalk = 2;
    public int wayPointsInPath = 3;

    // current rooms used for floor
    public int RoomsInFloor = 24;

    // mini-map variables for automatic scaling
    public Camera Mini_map_renderer;
    public Vector2 FullRadius = new Vector2(70,70);
    public float FullSize = 35;

    Vector2 minRadius = new Vector2(0, 0);
    Vector2 maxRadius = new Vector2(0, 0);

    // direction enum when referencing array as a 2D grid
    enum Dir
    {
        UP, RIGHT, DOWN, LEFT
    };

    // all room nodes used for the floor
    List<FloorNode> AllFloorPaths = new List<FloorNode>();




    // used to setup the floor once the scene is built
    void SetupFloor()
    {

        for (int y = 0; y < GridSizeY; y++)
        {
            List<FloorNode> row = new List<FloorNode>();

            for (int x = 0; x < GridSizeX; x++)  // going by rows first...
            {
                FloorNode ptr = Instantiate(floor).GetComponent<FloorNode>();
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


        //List<GameObject> AllFloorConnectors = new List<GameObject>();
        //// right now, the rooms know which rooms are connected via the FloorNode awake function.
        //// however, they don't know their grid positions.
        //AllFloorConnectors.AddRange(GameObject.FindGameObjectsWithTag("FloorCon"));

        //// remove colliders once grid setup is done.
        //for (int i = 0; i < AllFloorConnectors.Count; i++)
        //{
        //    AllFloorConnectors[i].GetComponent<Collider>().enabled = false;
        //}

        //// a floor node is nominated as the start and therefore the grid is based on it's origin
        //FloorNode Pointer = Element_0_0;

        //// constructs a data-esque representation of the grid.
        //for (int y = 0; y < GridSizeY; y++)
        //{
        //    FloorNode RowStart = Pointer;
        //    List<FloorNode> row = new List<FloorNode>();
        //    for (int x = 0; x < GridSizeX; x++)  // going by rows first...
        //    {
        //        row.Add(Pointer);
        //        Pointer.GridLoc = new Vector2(x, y);
        //        Pointer = Pointer.GridConnectors[(int)Dir.RIGHT];
        //    }

        //    Pointer = RowStart.GridConnectors[(int)Dir.UP]; // then increase the pointer to the next start of row
        //    FloorMatrix.Add(row);
        //}
    }

    void GenerateFloor()
    {
        // generateFloor recieves data on the condition of the floor
        Debug.Log("creating floor with:\n" + RoomsInFloor + " rooms\n" + GuardsToStand + " guards to stand\n" + GuardsToWalk + " guards to walk\ntreasure values between " + MinTreasureAmount + " and " + MaxTreasureAmount + "\n");

        //starts a a random room
        FloorNode start = FloorMatrix[Random.Range(0, GridSizeX-1)][Random.Range(0, GridSizeY-1)];
        CurrentStartNode = start;
        CurrentStartNode.IsStartingRoom = true;
        start.Used = true;


        // initialise the unvisited nodes by looking at start location.
        List<FloorNode> UnvisitedNodes = new List<FloorNode>();
        List<FloorNode> UsedNodes = new List<FloorNode>();
        UsedNodes.Add(start);
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
            int ToConnect = temp[Random.Range(0, temp.Count)];

            //communicates to both nodes which direction the other node is
            NextAdded.PathConnectors[ToConnect] = true;
            int reverse = -1;
            // gets the direction of the connected node relative to the itself  
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
            // modifies the connected node to know which directions it is being connected to in relation to NextAdded.
            NextAdded.GridConnectors[ToConnect].PathConnectors[reverse] = true;
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

        List<FloorNode> CornerRooms = new List<FloorNode>();

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
                // if there is no path connected to the UP direction of the node...
                if (!FloorMatrix[a][b].PathConnectors[(int)Dir.UP])
                {
                    count--;
                    // if there is a room in the UP direction...
                    if (FloorMatrix[a][b].GridConnectors[(int)Dir.UP])
                    {
                        // if the room in the UP direction doesn't have a wall...   
                        if (!FloorMatrix[a][b].GridConnectors[(int)Dir.UP].WallPlaced[(int)Dir.DOWN])
                        {
                            // then spawn a wall and alert both rooms that there is a wall there. this is to prevent to walls spawning in the same place and reduces resources.
                            g = Instantiate(Up_Wall, FloorMatrix[a][b].transform.parent.position, Quaternion.identity);

                            FloorMatrix[a][b].GridConnectors[(int)Dir.UP].WallPlaced[(int)Dir.DOWN] = true;
                            FloorMatrix[a][b].WallPlaced[(int)Dir.UP] = true;

                            g.transform.parent = AllWallPoint.transform;
                            AllWalls.Add(g);
                        }
                    }
                    else
                    {
                        //spawn a wall but only alert the current room. this is a boundry room, meaning that there is no rooms beyond that point in that direction.
                        g = Instantiate(Up_Wall, FloorMatrix[a][b].transform.parent.position, Quaternion.identity);
                        FloorMatrix[a][b].WallPlaced[(int)Dir.UP] = true;

                        g.transform.parent = AllWallPoint.transform;
                        AllWalls.Add(g);
                    }
                }
                // if there is no path connected to the RIGHT direction of the node...
                if (!FloorMatrix[a][b].PathConnectors[(int)Dir.RIGHT])
                {
                    count--;
                    // if there is a room in the RIGHT direction...
                    if (FloorMatrix[a][b].GridConnectors[(int)Dir.RIGHT])
                    {
                        // if the room in the RIGHT direction doesn't have a wall...   
                        if (!FloorMatrix[a][b].GridConnectors[(int)Dir.RIGHT].WallPlaced[(int)Dir.LEFT])
                        {
                            // then spawn a wall and alert both rooms that there is a wall there. this is to prevent to walls spawning in the same place and reduces resources.
                            g = Instantiate(Right_Wall, FloorMatrix[a][b].transform.parent.position, Quaternion.identity);
                            FloorMatrix[a][b].GridConnectors[(int)Dir.RIGHT].WallPlaced[(int)Dir.LEFT] = true;
                            FloorMatrix[a][b].WallPlaced[(int)Dir.RIGHT] = true;

                            g.transform.parent = AllWallPoint.transform;
                            AllWalls.Add(g);
                        }
                    }
                    else
                    {
                        //spawn a wall but only alert the current room. this is a boundry room, meaning that there is no rooms beyond that point in that direction.
                        g = Instantiate(Right_Wall, FloorMatrix[a][b].transform.parent.position, Quaternion.identity);
                        FloorMatrix[a][b].WallPlaced[(int)Dir.RIGHT] = true;

                        g.transform.parent = AllWallPoint.transform;
                        AllWalls.Add(g);
                    }

                }
                // if there is no path connected to the DOWN direction of the node...
                if (!FloorMatrix[a][b].PathConnectors[(int)Dir.DOWN])
                {
                    count--;
                    // if there is a room in the DOWN direction...
                    if (FloorMatrix[a][b].GridConnectors[(int)Dir.DOWN])
                    {
                        // if the room in the DOWN direction doesn't have a wall...   
                        if (!FloorMatrix[a][b].GridConnectors[(int)Dir.DOWN].WallPlaced[(int)Dir.UP])
                        {
                            // then spawn a wall and alert both rooms that there is a wall there. this is to prevent to walls spawning in the same place and reduces resources.
                            g = Instantiate(Down_Wall, FloorMatrix[a][b].transform.parent.position, Quaternion.identity);
                            FloorMatrix[a][b].GridConnectors[(int)Dir.DOWN].WallPlaced[(int)Dir.UP] = true;
                            FloorMatrix[a][b].WallPlaced[(int)Dir.DOWN] = true;

                            g.transform.parent = AllWallPoint.transform;
                            AllWalls.Add(g);
                        }
                    }
                    else
                    {
                        //spawn a wall but only alert the current room. this is a boundry room, meaning that there is no rooms beyond that point in that direction.
                        g = Instantiate(Down_Wall, FloorMatrix[a][b].transform.parent.position, Quaternion.identity);
                        FloorMatrix[a][b].WallPlaced[(int)Dir.DOWN] = true;

                        g.transform.parent = AllWallPoint.transform;
                        AllWalls.Add(g);
                    }
                }
                // if there is no path connected to the LEFT direction of the node...
                if (!FloorMatrix[a][b].PathConnectors[(int)Dir.LEFT])
                {
                    count--;
                    // if there is a room in the LEFT direction...
                    if (FloorMatrix[a][b].GridConnectors[(int)Dir.LEFT])
                    {
                        // if the room in the LEFT direction doesn't have a wall...   
                        if (!FloorMatrix[a][b].GridConnectors[(int)Dir.LEFT].WallPlaced[(int)Dir.RIGHT])
                        {
                            // then spawn a wall and alert both rooms that there is a wall there. this is to prevent to walls spawning in the same place and reduces resources.
                            g = Instantiate(Left_Wall, FloorMatrix[a][b].transform.parent.position, Quaternion.identity);
                            FloorMatrix[a][b].GridConnectors[(int)Dir.LEFT].WallPlaced[(int)Dir.RIGHT] = true;
                            FloorMatrix[a][b].WallPlaced[(int)Dir.LEFT] = true;

                            g.transform.parent = AllWallPoint.transform;
                            AllWalls.Add(g);
                        }
                    }
                    else
                    {
                        //spawn a wall but only alert the current room. this is a boundry room, meaning that there is no rooms beyond that point in that direction.
                        g = Instantiate(Left_Wall, FloorMatrix[a][b].transform.parent.position, Quaternion.identity);
                        FloorMatrix[a][b].WallPlaced[(int)Dir.LEFT] = true;

                        g.transform.parent = AllWallPoint.transform;
                        AllWalls.Add(g);
                    }
                }

                // if the room only contains 1 pathway, then it is a dead end and a possible place for the treasure
                if(count == 1)
                {
                    CornerRooms.Add(FloorMatrix[a][b]);
                }
            }
        }

        AllFloorPaths = new List<FloorNode>(UsedNodes); // updates AllFloorPaths of floors being used.

        AddTreasureRooms(CornerRooms);
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
        foreach (GameObject g in AllWalls)
        {
            Destroy(g); 
        }
        AllWalls.Clear();

        //destroy all treaure items (combo of light, treasure model and collider)
        foreach(GameObject g in TreasureItems)
        {
            Destroy(g);
        }
        TreasureItems.Clear();
        TreasureNodes.Clear();

        // destroys all guards
        foreach(GameObject g in GameObject.FindGameObjectsWithTag("Guard"))
        {
            Destroy(g);
        }

        //destroys grate object for player to begin in.
        Destroy(Current_Grate_Object);
        Current_Grate_Object = null;
        
    }

    // places player at the current start node with a Y offset and instantiate a grate for the player to begin in.
    void PlacePlayer()
    {
        Player.transform.position = CurrentStartNode.transform.parent.position + new Vector3(0, 1.5f);
        Player.GetComponent<Player_Transition>().Player_Disabled();
        Current_Grate_Object = Instantiate(Grate, CurrentStartNode.transform.parent.position, Quaternion.identity);
    }

    // places guards for the floor
    void PlaceGuards()
    {
        // Guard deployment - Stand - guards that will be given a specific waypoint and rotate
        // these guards will only spawn in a room adjacent to a treasure room.
        List<FloorNode> PossRooms = new List<FloorNode>();
        Dictionary<FloorNode, FloorNode> test = new Dictionary<FloorNode, FloorNode>(); // used for quick, unique storing
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
            g.GetComponent<Standing_Guard>().Start_Guard(); // initialises the guard
            g.GetComponent<Standing_Guard>().StandingPoint = PossRooms[chosen].transform.parent.position + new Vector3(0, 1.5f, 0); // gives a guard a standing point for the floor
            g.GetComponent<Standing_Guard>().Stand = true; // parent identifier on what type guard is
            g.GetComponent<Standing_Guard>().BeginTurning(); // starts the guard's AI movement
            g.GetComponent<Standing_Guard>().StartSuspicion(); // starting the guard's AI behaviour
            PossRooms.RemoveAt(chosen); // removes the rooms so no duplicate guard.

        }

        //Guard deployment - walking - guards that will be assigned waypoints to rotate and move about.
        // these guards can spawn anywhere within the used rooms.

        List<FloorNode> FloorPaths = new List<FloorNode>(AllFloorPaths);

        for(int b = 0; b < GuardsToWalk; b++)
        {
            g = Instantiate(GuardObject);
            g.AddComponent<Walking_Guard>();
            g.GetComponent<Walking_Guard>().Waypoint = true;

            if(FloorPaths.Count < wayPointsInPath) // fail safe if there is not enough rooms to place a guard (given the amount of waypoints needed)
            {
                break;
            }

            for (int i = 0; i < wayPointsInPath; i++) // chooses a number of waypoints (floor nodes) to ping-pong to.
            {
                int chos = Random.Range(0, FloorPaths.Count);


                g.GetComponent<Walking_Guard>().waypoints.Add(FloorPaths[chos].transform.parent.position + new Vector3(0, 1.5f, 0));
                FloorPaths.RemoveAt(chos);


            }
            g.GetComponent<Walking_Guard>().Start_Guard(); // initialises the guard
            g.GetComponent<Walking_Guard>().BeginWalking(); // start's the guard's AI movement
            g.GetComponent<Walking_Guard>().StartSuspicion(); // start's the guard's AI behaviour
        }

    }

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

    // adds treasure rooms that will contain a treasure to collect
    public void AddTreasureRooms(List<FloorNode> rooms)
    {
        for (int i = 0; i < TreasureRooms; i++)
        {

            int idx = Random.Range(0, rooms.Count);

            TreasureNodes.Add(rooms[idx]);
            TreasureNodes[i].IsTreasureRoom = true; // modifies the floor node to be treasure node

            GameObject g = Instantiate(Treasure, TreasureNodes[i].transform.parent.position, Quaternion.identity);
            g.GetComponent<Treasure_Info>().NodeLoc = TreasureNodes[i];

            int randModel = Random.Range(0, TreasureModels.Count); // chooses a random model to present
            Instantiate(TreasureModels[randModel], g.GetComponent<Treasure_Info>().Treasure_Holder.transform);
            g.GetComponent<Treasure_Info>().TresName = TreasureModels[randModel].name;
            g.GetComponent<Treasure_Info>().value = Random.Range(MinTreasureAmount, MaxTreasureAmount); // assigns a value based on game-manager's conditions
            TreasureItems.Add(g);
            rooms.RemoveAt(idx); // removes the rooms ro prevent duplicate
        }
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
        PlacePlayer();
        PlaceGuards();
    }
}
