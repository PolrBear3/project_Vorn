using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroManager : MonoBehaviour
{
    private Hero _currentHero;
    public Hero currentHero => _currentHero;

    private EventBus_Controller _heroDeathEventBus = new();
    public EventBus_Controller heroDeathEventBus => _heroDeathEventBus;


    // MonoBehaviour
    private void Awake()
    {
        EventBus_GlobalController.Register(EventBus.AwakeLoad, Set_Data);
    }

    private void OnDestroy()
    {
        EventBus_Controller endTurnBus = GameManager.instance.stageManager.endTurnEventBus;
        
        endTurnBus.UnRegister(EndStage_OnHeroDeath);
        endTurnBus.UnRegister(Hero_Unavailable);
        endTurnBus.UnRegister(_heroDeathEventBus.DelayBus_Running);
    }


    // Data
    private void Set_Data()
    {
        EventBus_Controller endTurnBus = GameManager.instance.stageManager.endTurnEventBus;

        endTurnBus.Register(3, EndStage_OnHeroDeath);
        endTurnBus.Register(Hero_Unavailable);
        endTurnBus.Register(_heroDeathEventBus.DelayBus_Running);
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


    // Game Over
    private IEnumerator EndStage_OnHeroDeath()
    {
        if (_currentHero == null || _currentHero.data.currentData.currentHealth > 0) yield break;

        _heroDeathEventBus.RunSequential_DelayBusEvents();
    }
}
