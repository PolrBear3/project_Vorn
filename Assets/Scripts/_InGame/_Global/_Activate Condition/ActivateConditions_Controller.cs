using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateConditions_Controller
{
    [Space(20)]
    [SerializeField] private ActivateCondition_ScrObj[] _activateConditions;

    public bool AllConditions_Available(Tile activatingTile)
    {
        for (int i = 0; i < _activateConditions.Length; i++)
        {
            if (_activateConditions[i].Activate_Available(activatingTile)) continue;
            return false;
        }
        return true;
    }
}