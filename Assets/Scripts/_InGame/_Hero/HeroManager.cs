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
        GameManager manager = GameManager.instance;
        EventBus_Controller endTurnBus = manager.stageManager.endTurnEventBus;
        
        endTurnBus.UnRegister(EndStage_OnHeroDeath);
        endTurnBus.UnRegister(Hero_Unavailable);
        endTurnBus.UnRegister(_heroDeathEventBus.DelayBus_Running);

        TileManager tileManager = manager.tileManager;
        EventBus_Controller tileHoverEventBus = tileManager.tileHoverEventBus;

        tileHoverEventBus.UnRegister(Update_MovementRoute_OnHeroHover);
        tileHoverEventBus.UnRegister(Update_MovementRoute_OnTileTargeting);

        tileManager.tileSelectEventBus.UnRegister(Toggle_TileMovementTargeting);
    }


    // Data
    private void Set_Data()
    {
        GameManager manager = GameManager.instance;
        EventBus_Controller endTurnBus = manager.stageManager.endTurnEventBus;

        endTurnBus.Register(3, EndStage_OnHeroDeath);
        endTurnBus.Register(Hero_Unavailable);
        endTurnBus.Register(_heroDeathEventBus.DelayBus_Running);

        TileManager tileManager = manager.tileManager;
        EventBus_Controller tileHoverEventBus = tileManager.tileHoverEventBus;

        tileHoverEventBus.Register(0, Update_MovementRoute_OnHeroHover);
        tileHoverEventBus.Register(1, Update_MovementRoute_OnTileTargeting);

        tileManager.tileSelectEventBus.Register(0, Toggle_TileMovementTargeting);
    }

    private bool Hero_Unavailable()
    {
        return _currentHero == null && GameManager.instance.cardManager.placedCards.Count <= 0;
    }
    public void Track_CurrentHero(Hero heroToTrack)
    {
        if (heroToTrack == null) return;

        heroToTrack.transform.SetParent(transform);
        _currentHero = heroToTrack;
    }


    // Movement Targeting
    private void Toggle_TileMovementTargeting()
    {
        GameManager manager = GameManager.instance;
        Tile selectedTile = manager.tileManager.hoveringTile;

        if (selectedTile == null) return;
        if (_currentHero == null || _currentHero.movement.currentTile != selectedTile) return;

        manager.tileTargeting.Toggle_Targeting(_currentHero);
    }
    
    private void Update_MovementRoute_OnTileTargeting()
    {
        if (_currentHero == null) return;

        GameManager manager = GameManager.instance;
        if (manager.tileTargeting.toggledSource is not Hero hero || _currentHero != hero) return;

        TileManager tileManager = manager.tileManager;

        Tile hoveringTile = tileManager.hoveringTile;
        if (hoveringTile == null) return;

        List<Tile> routeTiles = tileManager.PathFind_RouteTiles(_currentHero.movement.currentTile, hoveringTile);

        for (int i = 0; i < routeTiles.Count; i++)
        {
            Tile routeTile = routeTiles[i];

            if (routeTile == hoveringTile) continue;
            routeTile.indicatorAnimController.Play_State(UIAnimation.Available);
        }
    }
    private void Update_MovementRoute_OnHeroHover()
    {
        if (_currentHero == null) return;

        GameManager manager = GameManager.instance;

        if (manager.stageManager.endTurnEventBus.DelayBus_Running()) return;
        if (manager.tileTargeting.toggledSource != null) return;

        TileManager tileManager = manager.tileManager;
        Tile hoveringTile = tileManager.hoveringTile;

        if (hoveringTile == null || hoveringTile != _currentHero.movement.currentTile)
        {
            tileManager.Reset_TileIndicators();
            return;
        }

        List<Tile> routeTiles = _currentHero.tileTargeting.targetingTiles;
        if (routeTiles.Count <= 0) return;
        
        Tile destinationTile = routeTiles[0];
        routeTiles = tileManager.PathFind_RouteTiles(_currentHero.movement.currentTile, destinationTile);

        for (int i = 0; i < routeTiles.Count; i++)
        {
            Tile routeTile = routeTiles[i];

            if (routeTile == hoveringTile) continue;
            routeTile.indicatorAnimController.Play_State(UIAnimation.Available);
        }
    }


    // Game Over
    private IEnumerator EndStage_OnHeroDeath()
    {
        if (_currentHero == null || _currentHero.data.currentData.currentHealth > 0) yield break;

        _heroDeathEventBus.RunSequential_DelayBusEvents();
    }
}
