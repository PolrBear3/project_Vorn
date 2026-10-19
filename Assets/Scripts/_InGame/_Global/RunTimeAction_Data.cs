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

    private int _healthModifySuccessCount;
    public int healthModifySuccessCount => _healthModifySuccessCount;


    public void Toggle_RunAction(bool toggle)
    {
        _actionRunning = toggle;

        if (toggle) return;

        _targetingTile = null;
        _healthModifySuccessCount = 0;
    }

    public void Update_TargetingTile(Tile targetingTile)
    {
        _targetingTile = targetingTile;
    }

    private void CountUpdate_HealthModifySuccess()
    {
        _healthModifySuccessCount++;
    }
    public void CountUpdate_TargetingHealthModify(bool healthModified)
    {
        if (healthModified == false) return;

        CountUpdate_HealthModifySuccess();
    }
}