using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class gamemanager : MonoBehaviour
{

    public Text treasureValues;
    public int TotalTreasureValue;
    // Start is called before the first frame update
    void Start()
    {
        treasureValues.text = "treasures:\n$000";

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddTreasureValue(int amount)
    {
        Debug.Log(amount);
        TotalTreasureValue += amount;
        treasureValues.text = "treasures:\n$" + TotalTreasureValue.ToString();
    }
}
