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


    // New
    public HeroData(Hero_ScrObj setHero)
    {
        _heroScrObj = setHero;
        _currentData = new(setHero.interactionData);
    }
}
