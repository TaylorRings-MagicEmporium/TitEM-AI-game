using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericListSO<T> : ScriptableObject
{
    [SerializeField]
    List<T> objects = new List<T>();

    public void Add(T item)
    {
        objects.Add(item);
    }

    public bool Remove(T item)
    {
        return objects.Remove(item);
    }

    public void RemoveAtIndex(int index)
    {
        if(index < objects.Count)
        {
            objects.RemoveAt(index);
        }
        else
        {
            Debug.LogWarning("WARNING: Index out of range, ignore function!", this);
        }
    }

    public void ClearList()
    {
        objects.Clear();
    }

    private void OnEnable()
    {
        hideFlags = HideFlags.DontUnloadUnusedAsset;
        ClearList();
    }

    public void AddList(List<T> addedObjects)
    {
        objects.AddRange(addedObjects);
    }

    public List<T> GetList()
    {
        return objects;
    }

}
