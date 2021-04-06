using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Position_Status : MonoBehaviour
{
    Queue<Vector3> PrevPositions = new Queue<Vector3>();

    private void Start()
    {
        StartCoroutine(Update_Queue());
    }

    IEnumerator Update_Queue()
    {
        while (true)
        {
            if(PrevPositions.Count == 3)
            {
                PrevPositions.Dequeue();
            }
            PrevPositions.Enqueue(transform.position);

            yield return new WaitForSeconds(1f);
        }
    }

    public Vector3 GetPos()
    {
        return PrevPositions.Peek();
    }

}
