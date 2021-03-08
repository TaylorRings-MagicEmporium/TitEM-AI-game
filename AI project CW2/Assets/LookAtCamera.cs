using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    public Transform look;
    void Update()
    {
        transform.LookAt(look);
    }
}
