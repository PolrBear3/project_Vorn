using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HeroManager_Data
{
    [ES3Serializable] private List<Hero_ScrObj> _selectableHeroes = new();
    public List<Hero_ScrObj> selectableHeros => _selectableHeroes;

    [ES3Serializable] private Hero_ScrObj _selectedHero;
    public Hero_ScrObj selectedHero => _selectedHero;


    // New
    public HeroManager_Data(List<Hero_ScrObj> selectableHeroes)
    {
        _selectableHeroes = selectableHeroes;
    }


    // Data
    public void Update_SelectedHero(Hero_ScrObj updateHero)
    {
        _selectedHero = updateHero;
    }
}
