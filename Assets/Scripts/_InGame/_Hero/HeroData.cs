using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HeroData
{
    private Hero_ScrObj _heroScrObj;
    public Hero_ScrObj heroScrObj => _heroScrObj;

    private InteractionData _currentData;
    public InteractionData currentData => _currentData;


    private int _maxManaCount;
    public int maxManaCount => _maxManaCount;

    private int _currentManaCount;
    public int currentManaCount => _currentManaCount;


    private int _movementManaMultiplyValue;
    public int movementManaMultiplyValue => _movementManaMultiplyValue;

    private int _recentMovementManaCost;
    public int recentMovementManaCost => _recentMovementManaCost;


    public Action<int, int> OnManaUpdate; // currentManaCount, maxManaCount


    // New
    public HeroData(Hero_ScrObj setHero)
    {
        _heroScrObj = setHero;
        _currentData = new(setHero.interactionData);

        _maxManaCount = setHero.maxManaCount;
        _currentManaCount = _maxManaCount;

        _movementManaMultiplyValue = _heroScrObj.movementManaCost;
    }


    // Data
    public void Update_MaxManaCount(int updateCount)
    {
        updateCount = Mathf.Max(0, updateCount);
        if (updateCount == _maxManaCount) return;

        _maxManaCount = updateCount;
        OnManaUpdate?.Invoke(_currentManaCount, _maxManaCount);
    }
    public void Update_CurrentManaCount(int updateCount)
    {
        updateCount = Mathf.Clamp(updateCount, 0, _maxManaCount);
        if (updateCount == _currentManaCount) return;

        _currentManaCount = updateCount;
        OnManaUpdate?.Invoke(_currentManaCount, _maxManaCount);
    }

    public int Modify_MovementMana_MultiplyValue(int modifyValue)
    {
        _movementManaMultiplyValue += modifyValue;
        return _movementManaMultiplyValue;
    }
    public void Reset_MovementMana_MultiplyValue()
    {
        _movementManaMultiplyValue = _heroScrObj.movementManaCost;
    }

    public void Update_RecentMovementManaCost(int updateValue)
    {
        _recentMovementManaCost = Mathf.Max(0, updateValue);
    }
}
