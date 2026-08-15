using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventBus_Controller
{
    private readonly Dictionary<int, Action> _eventSequentialBus = new();
    private readonly Dictionary<int, Func<IEnumerator>> _eventSequentialDelayBus = new();

    private bool _delayBusRunning;
    public bool delayBusRunning => _delayBusRunning;


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


    // Delay Bus
    public void Register(int sequentialIndex, Func<IEnumerator> targetAction)
    {
        sequentialIndex = Mathf.Max(0, sequentialIndex);

        if (_eventSequentialDelayBus.ContainsKey(sequentialIndex) == false)
        {
            _eventSequentialDelayBus.Add(sequentialIndex, targetAction);
            return;
        }

        _eventSequentialDelayBus[sequentialIndex] += targetAction;
    }
    public void UnRegister(Func<IEnumerator> removeAction)
    {
        List<int> keys = new(_eventSequentialDelayBus.Keys);

        for (int i = 0; i < keys.Count; i++)
        {
            int key = keys[i];

            _eventSequentialDelayBus[key] -= removeAction;

            if (_eventSequentialDelayBus[key] != null) continue;
            _eventSequentialDelayBus.Remove(key);
        }
    }

    public IEnumerator SequentialDelayBus_RunUpdate()
    {
        if (_eventSequentialDelayBus.Count <= 0) yield break;
        _delayBusRunning = true;

        int highestIndex = 0;

        foreach (var eventBus in _eventSequentialDelayBus)
        {
            if (eventBus.Key <= highestIndex) continue;
            highestIndex = eventBus.Key;
        }

        for (int i = 0; i <= highestIndex; i++)
        {
            if (_eventSequentialDelayBus.TryGetValue(i, out Func<IEnumerator> runAction) == false) continue;

            Delegate[] delegates = runAction.GetInvocationList();

            for (int j = 0; j < delegates.Length; j++)
            {
                Func<IEnumerator> action = (Func<IEnumerator>)delegates[j];
                yield return action();
            }
        }
        _delayBusRunning = false;
    }
}