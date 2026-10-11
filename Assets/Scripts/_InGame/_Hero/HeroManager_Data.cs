using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HeroManager_Data
{
    [ES3Serializable] private Hero_ScrObj _selectedHero;
    public Hero_ScrObj selectedHero => _selectedHero;


    // New
    public HeroManager_Data(Hero_ScrObj selectHero)
    {
        _selectedHero = selectHero;
    }
}
