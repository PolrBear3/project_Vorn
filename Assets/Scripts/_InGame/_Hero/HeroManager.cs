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
        EventBus_GlobalController.Register(EventBus.AwakeLoad, Set_Data);
    }

    private void OnDestroy()
    {
        EventBus_GlobalController.UnRegister(EventBus.AwakeLoad, Set_Data);


        GameManager.instance.stageManager.endTurnEventBus.UnRegister(Hero_Unavailable);
    }


    // Data
    private void Set_Data()
    {
        GameManager.instance.stageManager.endTurnEventBus.Register(Hero_Unavailable);
    }


    public void Track_CurrentHero(Hero heroToTrack)
    {
        if (heroToTrack == null) return;

        heroToTrack.transform.SetParent(transform);
        _currentHero = heroToTrack;
    }

    private bool Hero_Unavailable()
    {
        return _currentHero == null && GameManager.instance.cardManager.placedCards.Count <= 0;
    }
}
