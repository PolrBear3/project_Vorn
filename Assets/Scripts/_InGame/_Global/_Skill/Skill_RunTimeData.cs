using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Skill_RunTimeData
{
    private int _activationCount;
    public int activationCount => _activationCount;


    // Data Updates
    public void Track_ActivationCount()
    {
        _activationCount++;
    }
}
