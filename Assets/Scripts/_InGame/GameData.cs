using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    [ES3Serializable] private Dictionary<Hero_ScrObj, int> _heroProgressionDatas = new();
    public Dictionary<Hero_ScrObj, int> heroProgressionDatas => _heroProgressionDatas;


    // Data
    public int Hero_ProgressionCount(Hero_ScrObj progressionHero)
    {
        if (_heroProgressionDatas.ContainsKey(progressionHero) == false) return 0;
        return _heroProgressionDatas[progressionHero];
    }

    public void Increase_HeroProgressionCount(Hero_ScrObj progressionHero)
    {
        if (progressionHero == null) return;

        if (_heroProgressionDatas.ContainsKey(progressionHero) == false)
        {
            _heroProgressionDatas.Add(progressionHero, 1);
            return;
        }
        _heroProgressionDatas[progressionHero]++;
    }
    public void Decrease_HeroProgressionCount(Hero_ScrObj progressionHero)
    {
        if (progressionHero == null) return;
        if (_heroProgressionDatas.ContainsKey(progressionHero) == false) return;

        _heroProgressionDatas[progressionHero] = Mathf.Max(0, _heroProgressionDatas[progressionHero] - 1);
    }
}
