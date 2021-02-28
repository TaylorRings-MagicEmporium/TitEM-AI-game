using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{

    public int GridSizeX = 6;
    public int GridSizeY = 6;

    List<List<int>> AdgenMatrix = new List<List<int>>();

    List<List<FloorNode>> FloorMatrix = new List<List<FloorNode>>();

    public List<Vector2> discoveredPoints = new List<Vector2>();

    List<GameObject> AllFloorConnectors = new List<GameObject>();
    public FloorNode Element_0_0;


    public GameObject Up_Wall;
    public GameObject Right_Wall;
    public GameObject Down_Wall;
    public GameObject Left_Wall;

    enum Dir
    {
        UP, RIGHT, DOWN, LEFT
    };

    // Start is called before the first frame update
    void Start()
    {

        AllFloorConnectors.AddRange(GameObject.FindGameObjectsWithTag("FloorCon"));

        for (int i = 0; i < AllFloorConnectors.Count; i++)
        {
            AllFloorConnectors[i].GetComponent<Collider>().enabled = false;
        }
        FloorNode Pointer = Element_0_0;

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

        FloorNode start = FloorMatrix[Random.Range(0, 6)][Random.Range(0, 6)];
        start.Used = true;

        List<FloorNode> UnvisitedNodes = new List<FloorNode>();
        for (int i = 0; i < 4; i++)
        {
            if (start.GridConnectors[i] != null)
            {
                UnvisitedNodes.Add(start.GridConnectors[i]);
                start.GridConnectors[i].addedUnvisited = true;
            }

        }

        bool Condition = true;
        int NumOfRooms = 1;
        while (Condition)
        {




            // getting random element from unvisited vertices
            int idx = Random.Range(0, UnvisitedNodes.Count);
            FloorNode NextAdded = UnvisitedNodes[idx];
            UnvisitedNodes.RemoveAt(idx);
            Debug.Log(NextAdded.GridLoc);

            // getting a used vertice to connect to unvisited
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
            int ToConnect = temp[Random.Range(0, temp.Count)];

            // connects next vertice to used vertice
            NextAdded.PathConnectors[ToConnect] = true;
            int reverse = -1;
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
            NumOfRooms++;
            for (int i = 0; i < 4; i++)
            {
                if (NextAdded.GridConnectors[i] != null && !NextAdded.GridConnectors[i].Used && !NextAdded.GridConnectors[i].addedUnvisited)
                {
                    UnvisitedNodes.Add(NextAdded.GridConnectors[i]);
                    NextAdded.GridConnectors[i].addedUnvisited = true;
                }

            }


            //while condition status
            //if(UnvisitedNodes.Count == 0)
            //{
            //    Condition = false;
            //}
            if (NumOfRooms >= (GridSizeX * GridSizeY) / 2)
            {
                Condition = false;
            }

        }

        for (int a = 0; a < GridSizeX; a++)
        {
            for(int b = 0; b < GridSizeY; b++){

                bool hide = true;
                for(int i = 0; i < 4; i++)
                {
                    if (FloorMatrix[a][b].PathConnectors[i])
                    {
                        hide = false;
                        break;
                    }
                }
                if (hide)
                {
                    FloorMatrix[a][b].ChangeFloor(false);
                    continue;
                }


                if (!FloorMatrix[a][b].PathConnectors[0])
                {
                    if (FloorMatrix[a][b].GridConnectors[0])
                    {
                        if (!FloorMatrix[a][b].GridConnectors[0].WallPlaced[2])
                        {
                            Instantiate(Up_Wall, FloorMatrix[a][b].transform.parent.position, Quaternion.identity);
                            FloorMatrix[a][b].GridConnectors[0].WallPlaced[2] = true;
                            FloorMatrix[a][b].WallPlaced[0] = true;
                        }
                    }
                    else
                    {
                        Instantiate(Up_Wall, FloorMatrix[a][b].transform.parent.position, Quaternion.identity);
                        FloorMatrix[a][b].WallPlaced[0] = true;
                    }
                    
                }
                if (!FloorMatrix[a][b].PathConnectors[1])
                {
                    if (FloorMatrix[a][b].GridConnectors[1])
                    {
                        if (!FloorMatrix[a][b].GridConnectors[1].WallPlaced[3])
                        {
                            Instantiate(Right_Wall, FloorMatrix[a][b].transform.parent.position, Quaternion.identity);
                            FloorMatrix[a][b].GridConnectors[1].WallPlaced[3] = true;
                            FloorMatrix[a][b].WallPlaced[1] = true;
                        }
                    }
                    else
                    {
                        Instantiate(Right_Wall, FloorMatrix[a][b].transform.parent.position, Quaternion.identity);
                        FloorMatrix[a][b].WallPlaced[1] = true;
                    }
                }
                if (!FloorMatrix[a][b].PathConnectors[2])
                {
                    if (FloorMatrix[a][b].GridConnectors[2])
                    {
                        if (!FloorMatrix[a][b].GridConnectors[2].WallPlaced[0])
                        {
                            Instantiate(Down_Wall, FloorMatrix[a][b].transform.parent.position, Quaternion.identity);
                            FloorMatrix[a][b].GridConnectors[2].WallPlaced[0] = true;
                            FloorMatrix[a][b].WallPlaced[2] = true;
                        }
                    }
                    else
                    {
                        Instantiate(Down_Wall, FloorMatrix[a][b].transform.parent.position, Quaternion.identity);
                        FloorMatrix[a][b].WallPlaced[2] = true;
                    }
                }
                if (!FloorMatrix[a][b].PathConnectors[3])
                {
                    if (FloorMatrix[a][b].GridConnectors[3])
                    {
                        if (!FloorMatrix[a][b].GridConnectors[3].WallPlaced[1])
                        {
                            Instantiate(Left_Wall, FloorMatrix[a][b].transform.parent.position, Quaternion.identity);
                            FloorMatrix[a][b].GridConnectors[3].WallPlaced[1] = true;
                            FloorMatrix[a][b].WallPlaced[3] = true;
                        }
                    }
                    else
                    {
                        Instantiate(Left_Wall, FloorMatrix[a][b].transform.parent.position, Quaternion.identity);
                        FloorMatrix[a][b].WallPlaced[3] = true;
                    }

                }
            }
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
