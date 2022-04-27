using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Guard Data", menuName = "Guard Data")]
public class SOGuardData : ScriptableObject
{
    [Header("Limits of states")]
    public float LimitToPatrol = 0;
    public float LimitToInvestigate = 0;

    [Space]
    public float MinimumLimit = 0;
    public float MaximumLimit = 0;

}
