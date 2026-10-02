using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EventBus
{
    DataLoad = 0,
    AwakeLoad = 1,
    StartLoad = 2,
    SubLoad = 3
}

public interface ISubscriptionHandler
{
    void Subscribe_All();
    void UnSubscribe_All();
}

public static class EventBus_GlobalController
{
    private static readonly Dictionary<EventBus, Action> _eventBuses = new();
    private static readonly List<ISubscriptionHandler> _subscriptionHandlers = new();


    // Event Bus Register
    public static void Register(EventBus eventState, Action targetAction)
    {
        if (_eventBuses.ContainsKey(eventState) == false)
        {
            _eventBuses.Add(eventState, targetAction);
            return;
        }
        _eventBuses[eventState] += targetAction;
    }
    public static void UnRegister(EventBus eventState, Action targetAction)
    {
        _eventBuses[eventState] -= targetAction;
    }


    // Subscription Handler Register
    public static void Register(ISubscriptionHandler handler)
    {
        if (_subscriptionHandlers.Contains(handler)) return;
        _subscriptionHandlers.Add(handler);
    }
    public static void UnRegister(ISubscriptionHandler handler)
    {
        _subscriptionHandlers.Remove(handler);
    }

    public static void UnSubscribeAll_SubscriptionHandlers()
    {
        for (int i = _subscriptionHandlers.Count - 1; i >= 0 ; i--)
        {
            _subscriptionHandlers[i].UnSubscribe_All();
        }
    }


    // Run
    public static void Run_BusEvents()
    {
        if (_eventBuses.Count <= 0) return;

        for (int i = 0; i < _eventBuses.Count; i++)
        {
            EventBus runBus = (EventBus)i;
            _eventBuses[runBus]?.Invoke();
        }
    }
}
