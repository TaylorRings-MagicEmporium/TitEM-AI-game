using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;

/// <summary>
/// Listens to a gameEvent and executes any response function when raised.
/// </summary>
public class GameEventListener : MonoBehaviour
{
    public GameEvent Event;
    public UnityEvent Response;

    /// <summary>
    /// Adds itself to the event when enabled.
    /// </summary>
    void OnEnable()
    {
        Event.RegisterListener(this);
    }

    /// <summary>
    /// Removes itself from the event when disabled.
    /// </summary>
    void OnDisable()
    {
        Event.UnregisterListener(this);
    }

    /// <summary>
    /// Invokes the UnityEvent (delegate)
    /// </summary>
    public void OnEventRaised()
    {
        Response.Invoke();
    }
}
