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

    public Action<int, int> OnManaUpdate; // currentManaCount, maxManaCount


    // New
    public HeroData(Hero_ScrObj setHero)
    {
        _heroScrObj = setHero;
        _currentData = new(setHero.interactionData);

        _maxManaCount = setHero.maxManaCount;
        _currentManaCount = _maxManaCount;
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
}
