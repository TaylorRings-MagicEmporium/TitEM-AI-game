using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A GameEvent is a structure to send a nodification to a listener, effectively communicating without having a direct link.
/// </summary>
[CreateAssetMenu(fileName = " New Game Object", menuName = "SO Event")]
public class GameEvent : ScriptableObject
{

    private List<GameEventListener> listeners = new List<GameEventListener>();

    /// <summary>
    /// On Raise, will go through each listener and acctivate it's unity event.
    /// </summary>
    public void Raise()
    {
        for(int i = listeners.Count - 1; i>= 0; i--)
        {
            listeners[i].OnEventRaised();
        }
    }

    /// <summary>
    /// Registers the Listener to the event.
    /// </summary>
    /// <param name="listener"></param>
    public void RegisterListener(GameEventListener listener)
    {
        if(!listeners.Contains(listener)) listeners.Add(listener);
    }

    /// <summary>
    /// Deregisters the Listener the event.
    /// </summary>
    /// <param name="listener"></param>
    public void UnregisterListener(GameEventListener listener)
    {
        if(listeners.Contains(listener)) listeners.Remove(listener);
    }
}
