using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventList : MonoBehaviour
{
    public UnityEvent[] events;
    private int _last_index = -1;

    public void InvokeEvent(int index)
    {
        _last_index = index;
        events[index].Invoke();
    }

    public void InvokeNextEvent()
    {
        _last_index++;
        events[_last_index].Invoke();
    }
}
