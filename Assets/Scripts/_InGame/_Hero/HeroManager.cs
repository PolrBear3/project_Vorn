using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroManager : MonoBehaviour
{
    private Hero _currentHero;
    public Hero currentHero => _currentHero;


    // MonoBehaviour
    private void Awake()
    {

    }

    private void OnDestroy()
    {

    }


    // Data
    public void Track_CurrentHero(Hero heroToTrack)
    {
        if (heroToTrack == null) return;

        heroToTrack.transform.SetParent(transform);
        _currentHero = heroToTrack;
    }
}
