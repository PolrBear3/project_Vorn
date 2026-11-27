using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Skill_ConfigurationData
{
    [Space(10)]
    [SerializeField][Range(0, 10)] private int _maxActivationCount;
    public int maxActivationCount => _maxActivationCount;

    [SerializeField][Range(0, 10)] private int _activationDelayTurnCount;
    public int activationDelayTurnCount => _activationDelayTurnCount;


    // Activation Filters
    public bool On_ActivationDelay(int turnCount)
    {
        if (_activationDelayTurnCount <= 0) return false;
        return turnCount <= _activationDelayTurnCount;
    }

    public bool MaxCount_Activated(int currentActivationCount)
    {
        if (_maxActivationCount <= 0) return false;
        return currentActivationCount >= _maxActivationCount;
    }
}