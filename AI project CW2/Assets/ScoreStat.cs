using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SerializeField]
public class ScoreStat
{
    public ScoreStat(int cashAmount, int floorNum)
    {
        this.cashAmount = cashAmount;
        this.floorNum = floorNum;
    }
    public int cashAmount;
    public int floorNum;
}
