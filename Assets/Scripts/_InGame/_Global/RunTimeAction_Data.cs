using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IRunTimeAction
{
    RunTimeAction_Data runTimeActionData { get; }
}

[System.Serializable]
public class RunTimeAction_Data
{
    private bool _actionRunning;
    public bool actionRunning => _actionRunning;

    private Tile _targetingTile;
    public Tile targetingTile => _targetingTile;


    // Data
    public void Toggle_RunAction(bool toggle)
    {
        _actionRunning = toggle;

        if (toggle) return;
        _targetingTile = null;
    }

    public void Update_TargetingTile(Tile targetingTile)
    {
        _targetingTile = targetingTile;
    }
}