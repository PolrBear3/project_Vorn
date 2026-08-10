using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventBus_Controller
{
    private readonly Dictionary<int, Action> _eventSequentialBus = new();


    // Sequential Bus
    public void Register(int sequentialIndex, Action targetAction)
    {
        sequentialIndex = Mathf.Max(0, sequentialIndex);

        if (_eventSequentialBus.ContainsKey(sequentialIndex) == false)
        {
            _eventSequentialBus.Add(sequentialIndex, targetAction);
            return;
        }
        _eventSequentialBus[sequentialIndex] += targetAction;
    }
    public void UnRegister(Action removeAction)
    {
        List<int> keys = new(_eventSequentialBus.Keys);

        for (int i = 0; i < keys.Count; i++)
        {
            int key = keys[i];
            _eventSequentialBus[key] -= removeAction;

            if (_eventSequentialBus[key] != null) continue;
            _eventSequentialBus.Remove(key);

        }
    }

    public void RunSequential_BusEvents()
    {
        if (_eventSequentialBus.Count <= 0) return;

        int highestIndex = 0;

        foreach (var eventBus in _eventSequentialBus)
        {
            if (eventBus.Key <= highestIndex) continue;
            highestIndex = eventBus.Key;
        }

        for (int i = 0; i <= highestIndex; i++)
        {
            if (_eventSequentialBus.TryGetValue(i, out Action runAction) == false) continue;
            runAction?.Invoke();
        }
    }
}
