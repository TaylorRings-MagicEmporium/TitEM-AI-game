using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Treasure_Info : MonoBehaviour
{
    public int value = 500;
    bool TreasureTaken = false;
    public FloorNode NodeLoc;
    public string TresName;

    public void TreasureTakenUpdate()
    {
        TreasureTaken = true;
        Destroy(this.transform.GetChild(0).gameObject);
    }

    public bool IsTreasureTaken()
    {
        return TreasureTaken;
    }


}
