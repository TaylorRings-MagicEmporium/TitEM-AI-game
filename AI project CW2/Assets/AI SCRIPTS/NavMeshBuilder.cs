using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshBuilder : MonoBehaviour
{
    public void UpdateNavMesh()
    {
        GetComponent<NavMeshSurface>().BuildNavMesh();
    }
}
