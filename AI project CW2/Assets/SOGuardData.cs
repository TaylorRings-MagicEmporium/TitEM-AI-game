using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Guard Data", menuName = "Guard Data")]
public class SOGuardData : ScriptableObject
{
    [Header("Limits of changing states")]
    public float LimitToPatrol = 0;
    public float LimitToInvestigate = 0;

    [Space]
    [Header("The Min and Max of the suspicion meter")]
    public float MinimumLimit = 0;
    public float MaximumLimit = 0;

    [Space]
    [Header("sense distance limits")]
    public float MaxSightDistance = 0;
    public float MaxFeelDistance = 0;

    [Space]
    [Tooltip("The eye range of the guard when they look. Consider the angle to be how much they look to one side from their center view")]
    public float AngleLimit = 45.0f;

    [Space]
    [Header("The range of Sight in three levels")]
    public float FirstZoneLimit = 7.5f; // 30% 0-6
    public float SecondZoneLimit = 12.0f; // 70% 6-14
    public float ThirdZoneLimit = 15.0f; // 100% 14-20

}
