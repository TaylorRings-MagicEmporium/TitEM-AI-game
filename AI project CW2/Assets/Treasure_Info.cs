using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Treasure_Info : MonoBehaviour
{
    public int value = 500;
    bool TreasureTaken = false;
    public FloorNode NodeLoc;
    public string TresName;
    public Light l;

    public GameObject Treasure_Holder;

    public void TreasureTakenUpdate()
    {
        TreasureTaken = true;
        l.color = Color.red;
        Destroy(Treasure_Holder);
    }

    public bool IsTreasureTaken()
    {
        return TreasureTaken;
    }


}
