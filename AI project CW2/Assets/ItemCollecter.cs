using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemCollecter : MonoBehaviour
{
    public gamemanager gm;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Treasure"))
        {
            Treasure_Info ti = other.GetComponent<Treasure_Info>();
            other.GetComponent<Collider>().enabled = false;
            gm.AddTreasureValue(ti.value);
            Destroy(other.gameObject);
        }
    }

}
