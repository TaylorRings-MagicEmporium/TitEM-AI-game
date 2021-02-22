using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GridState : MonoBehaviour
{

    public bool NotWalkable = false;
    public GameObject wall;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (NotWalkable)
        {
            GetComponent<OffMeshLink>().area = 1;
            if (wall)
            {
                wall.SetActive(true);
            }
        }
        else if (!NotWalkable)
        {
            GetComponent<OffMeshLink>().area = 0;
            if (wall)
            {
                wall.SetActive(false);
            }
        }
    }
}
