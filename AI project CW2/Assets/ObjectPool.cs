using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public GameObject ObjectToPool;
    [SerializeField]
    int InitialAmount;
    List<GameObject> objects = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        for(int i = 0; i < InitialAmount; i++)
        {
            objects.Add(Instantiate(ObjectToPool,transform));
            objects[i].SetActive(false);
        }
    }

    public GameObject RequestObject()
    {
        foreach (GameObject g in objects)
        {
            if (!g.activeInHierarchy)
            {
                g.SetActive(true);
                return g;
            }
        }

        // no objects avaliable
        objects.Add(Instantiate(ObjectToPool, transform));
        return objects[objects.Count - 1];
    }

    public void ReturnObject(GameObject g)
    {
        if (objects.Contains(g))
        {
            g.SetActive(false);
        }
    }

    public void ResetObjectsState()
    {
        foreach(GameObject g in objects)
        {
            g.SetActive(false);
        }
    }

    public List<GameObject> GetActiveObjectsList()
    {
        List<GameObject> result = new List<GameObject>();
        foreach(GameObject g in objects)
        {
            if(g.activeInHierarchy) result.Add(g);
        }
        return result;
    }
}
