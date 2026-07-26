using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventBus_Controller
{
    private readonly Dictionary<EventBus, Action> _eventBuses = new();


    // Register
    public void Register(EventBus eventState, Action targetAction)
    {
        if (_eventBuses.ContainsKey(eventState) == false)
        {
            _eventBuses.Add(eventState, targetAction);
            return;
        }
        _eventBuses[eventState] += targetAction;
    }

    public void UnRegister(EventBus eventState, Action targetAction)
    {
        _eventBuses[eventState] -= targetAction;
    }


    // Run
    public void Run_BusEvents()
    {
        if (_eventBuses.Count <= 0) return;

        for (int i = 0; i < _eventBuses.Count; i++)
        {
            EventBus runBus = (EventBus)i;
            _eventBuses[runBus]?.Invoke();
        }
    }
}
