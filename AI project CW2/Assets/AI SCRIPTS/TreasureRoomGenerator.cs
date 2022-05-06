using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// transforms active rooms into treasure rooms, which contain a treasure. Part of the Maze generation sequence.
/// </summary>
public class TreasureRoomGenerator : MonoBehaviour
{
    public GameObject Treasure;
    public int NoTreasureRooms = 3;
    public List<FloorNode> TreasureNodes = new List<FloorNode>();
    public FloorNodeList treasureRooms;
    public FloorNodeList cornerRoomsList;
    List<GameObject> TreasureItems = new List<GameObject>();
    public List<GameObject> TreasureModels = new List<GameObject>();

    public int MinTreasureAmount = 0;
    public int MaxTreasureAmount = 0;

    public int MinLimit = 0;
    public int MaxLimit = 0;

    /// <summary>
    /// Resets and removes treasures from treasure rooms.
    /// </summary>
    public void ResetTreasureRooms()
    {
        foreach (GameObject g in TreasureItems)
        {
            Destroy(g);
        }
        TreasureItems.Clear();
        TreasureNodes.Clear();

        treasureRooms.ClearList();
    }

    /// <summary>
    /// converts floors to be treasure rooms by selecting random corner rooms.
    /// </summary>
    public void AddTreasureRooms()
    {

        List<FloorNode> cornerRooms = cornerRoomsList.GetList();
        for (int i = 0; i < NoTreasureRooms; i++)
        {
            if(cornerRooms.Count == 0)
            {
                Debug.Log("No corner rooms avaliable");
                break;
            }

            int idx = Random.Range(0, cornerRooms.Count);

            TreasureNodes.Add(cornerRooms[idx]);
            TreasureNodes[i].IsTreasureRoom = true; // modifies the floor node to be treasure node

            GameObject g = Instantiate(Treasure, TreasureNodes[i].transform.position, Quaternion.identity);
            g.GetComponent<Treasure_Info>().NodeLoc = TreasureNodes[i];

            int randModel = Random.Range(0, TreasureModels.Count); // chooses a random model to present
            Instantiate(TreasureModels[randModel], g.GetComponent<Treasure_Info>().Treasure_Holder.transform);
            g.GetComponent<Treasure_Info>().TresName = TreasureModels[randModel].name;
            g.GetComponent<Treasure_Info>().value = Random.Range(MinTreasureAmount, MaxTreasureAmount); // assigns a value based on game-manager's conditions
            TreasureItems.Add(g);
            cornerRooms.RemoveAt(idx); // removes the rooms ro prevent duplicate
        }

        treasureRooms.AddList(TreasureNodes);
    }

    /// <summary>
    /// returns the number of Active treasure rooms in the level.
    /// </summary>
    /// <returns> The amount of active treasure rooms in the level</returns>
    public int ActiveTreasureRooms()
    {
        return treasureRooms.GetList().Count;
    }

    /// <summary>
    /// Sets the treasure value limits.
    /// </summary>
    /// <param name="newDifficultyScale"> The new difficulty value to adjust the treasure value amount.</param>
    public void SetTreasureRange(float newDifficultyScale)
    {
        MinTreasureAmount = (int)(20.0f * newDifficultyScale);
        MaxTreasureAmount = (int)(50.0f * newDifficultyScale);

        MinTreasureAmount = Mathf.Clamp(MinTreasureAmount, 0, 450);
        MaxTreasureAmount = Mathf.Clamp(MaxTreasureAmount, 0, 500);
    }
}
