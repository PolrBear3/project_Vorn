using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroManager : MonoBehaviour
{
    private Hero _currentHero;
    public Hero currentHero => _currentHero;

    private EventBus_Controller _heroDeathEventBus = new();
    public EventBus_Controller heroDeathEventBus => _heroDeathEventBus;

    private int _recentMovementManaCost;


    [Space(20)]
    [SerializeField] private Hero_StatPanel _healthPanel;
    [SerializeField] private Hero_StatPanel _manaPanel;

    [Space(10)]
    [SerializeField][Range(0, 1000)] private float _statPanelsSpacingValue;


    // MonoBehaviour
    private void Awake()
    {
        EventBus_GlobalController.Register(EventBus.AwakeLoad, Set_Data);
    }

    private void OnDestroy()
    {
        // from Set_Data
        GameManager manager = GameManager.instance;
        EventBus_Controller endTurnBus = manager.stageManager.endTurnEventBus;

        endTurnBus.UnRegister(Hero_Unavailable);
        endTurnBus.UnRegister(_heroDeathEventBus.DelayBus_Running);

        endTurnBus.UnRegister(Run_HeroActions);
        endTurnBus.UnRegister(Refill_CurrentManaCount);
        endTurnBus.UnRegister(EndStage_OnHeroDeath);

        TileManager tileManager = manager.tileManager;
        EventBus_Controller tileHoverEventBus = tileManager.tileHoverEventBus;

        tileHoverEventBus.UnRegister(Update_MovementRoute_OnHeroHover);
        tileHoverEventBus.UnRegister(Update_MovementRoute_OnTileTargeting);

        tileManager.tileSelectEventBus.UnRegister(Toggle_TileMovementTargeting);
        manager.tileTargeting.OnTargetTile -= UpdateMana_OnTileMovementTarget;

        HandInventory handInventory = manager.handInventory;
        handInventory.OnPlatformWidthUpdate -= Update_StatPanelPositions;

        EventBus_Controller placeCardEventBus = handInventory.placeCardEventBus;

        placeCardEventBus.UnRegister(Cancel_TileMovementTargeting_OnRouteBlocked);
        placeCardEventBus.UnRegister(UpdateMana_OnTileMovementTarget);

        // from Track_CurrentHero
        if (_currentHero == null) return;
        _currentHero.interactionData.OnHealthUpdate -= _healthPanel.Update_ValueText;
        _currentHero.data.OnManaUpdate -= _manaPanel.Update_ValueText;
    }


    // Data
    private void Set_Data()
    {
        Update_HealthPanel();
        Update_ManaPanel();


        GameManager manager = GameManager.instance;
        EventBus_Controller endTurnBus = manager.stageManager.endTurnEventBus;

        endTurnBus.Register(Hero_Unavailable);
        endTurnBus.Register(_heroDeathEventBus.DelayBus_Running);

        endTurnBus.Register(1, Run_HeroActions);
        endTurnBus.Register(5, Refill_CurrentManaCount);
        endTurnBus.Register(6, EndStage_OnHeroDeath);

        TileManager tileManager = manager.tileManager;
        EventBus_Controller tileHoverEventBus = tileManager.tileHoverEventBus;

        tileHoverEventBus.Register(0, Update_MovementRoute_OnHeroHover);
        tileHoverEventBus.Register(1, Update_MovementRoute_OnTileTargeting);

        tileManager.tileSelectEventBus.Register(0, Toggle_TileMovementTargeting);
        manager.tileTargeting.OnTargetTile += UpdateMana_OnTileMovementTarget;

        HandInventory handInventory = manager.handInventory;
        handInventory.OnPlatformWidthUpdate += Update_StatPanelPositions;

        EventBus_Controller placeCardEventBus = handInventory.placeCardEventBus;

        placeCardEventBus.Register(0, Cancel_TileMovementTargeting_OnRouteBlocked);
        placeCardEventBus.Register(0, UpdateMana_OnTileMovementTarget);
    }
    public void Track_CurrentHero(Hero heroToTrack)
    {
        if (heroToTrack == null) return;

        heroToTrack.transform.SetParent(transform);

        _currentHero = heroToTrack;
        _currentHero.interactionData.OnHealthUpdate += _healthPanel.Update_ValueText;
        _currentHero.data.OnManaUpdate += _manaPanel.Update_ValueText;

        Update_HealthPanel();
        Update_ManaPanel();
    }

    private bool Hero_Unavailable()
    {
        return _currentHero == null && GameManager.instance.cardManager.placedCards.Count <= 0;
    }


    // Mana
    public int Current_ManaCount()
    {
        if (_currentHero == null) return 0;
        return _currentHero.data.currentManaCount;
    }
    public void Modify_CurrentManaCount(int modifyCount)
    {
        if (_currentHero == null || modifyCount == 0) return;

        int currentManaCount = _currentHero != null ? _currentHero.data.currentManaCount : 0;
        _currentHero.data.Update_CurrentManaCount(currentManaCount + modifyCount);
    }


    // Movement Targeting
    private List<Tile> RouteTiles_toDestionation()
    {
        if (_currentHero == null) return null;

        List<Tile> routeTiles = _currentHero.tileTargeting.targetingTiles;
        if (routeTiles.Count <= 0) return routeTiles;

        Tile destinationTile = routeTiles[0];
        return GameManager.instance.tileManager.PathFind_RouteTiles(_currentHero.movement.currentTile, destinationTile);
    }

    private void Toggle_TileMovementTargeting()
    {
        GameManager manager = GameManager.instance;
        Tile selectedTile = manager.tileManager.hoveringTile;

        if (selectedTile == null) return;
        if (_currentHero == null || _currentHero.movement.currentTile != selectedTile) return;

        if (manager.tileTargeting.Toggle_Targeting(_currentHero) == false) return;

        HeroData heroData = _currentHero.data;

        heroData.Update_CurrentManaCount(heroData.currentManaCount + _recentMovementManaCost);
        _recentMovementManaCost = 0;
    }

    private void Cancel_TileMovementTargeting()
    {
        if (_currentHero == null) return;

        TileTargeting_Data targetingData = _currentHero.tileTargeting;

        targetingData.targetingTiles.Clear();
        targetingData.recentTargetingTiles.Clear();

        Refund_MovementManaCost();
    }
    private void Cancel_TileMovementTargeting_OnRouteBlocked()
    {
        if (_currentHero == null) return;

        List<Tile> targetingTiles = _currentHero.tileTargeting.targetingTiles;
        if (targetingTiles.Count <= 0) return;

        List<Tile> routeTiles = RouteTiles_toDestionation();
        for (int i = 0; i < targetingTiles.Count; i++)
        {
            if (routeTiles.Contains(targetingTiles[i])) continue;

            Cancel_TileMovementTargeting();
            return;
        }
    }

    private void UpdateMana_OnTileMovementTarget()
    {
        if (_currentHero == null) return;

        int totalManaCost = RouteTiles_toDestionation().Count;
        int manaCostDifference = totalManaCost - _recentMovementManaCost;

        if (manaCostDifference == 0) return;

        HeroData heroData = _currentHero.data;
        
        int updatedManaCount = heroData.currentManaCount - manaCostDifference;
        if (updatedManaCount < 0)
        {
            Cancel_TileMovementTargeting();
            return;
        }

        heroData.Update_CurrentManaCount(updatedManaCount);
        _recentMovementManaCost = totalManaCost;
    }
    private void UpdateMana_OnTileMovementTarget(ITileTargeting heroSource)
    {
        if (_currentHero == null) return;
        if (heroSource is not Hero hero || _currentHero != hero) return;

        UpdateMana_OnTileMovementTarget();
    }

    private void Refund_MovementManaCost()
    {
        if (_currentHero == null) return;
        HeroData heroData = _currentHero.data;

        heroData.Update_CurrentManaCount(heroData.currentManaCount + _recentMovementManaCost);
        _recentMovementManaCost = 0;
    }

    private void Update_MovementRoute_OnTileTargeting()
    {
        if (_currentHero == null) return;

        GameManager manager = GameManager.instance;
        if (manager.tileTargeting.toggledSource is not Hero hero || _currentHero != hero) return;

        TileManager tileManager = manager.tileManager;

        Tile hoveringTile = tileManager.hoveringTile;
        if (hoveringTile == null) return;

        if (_currentHero.Targeting_Available(hoveringTile) == false)
        {
            hoveringTile.indicatorAnimController.Play_State(UIAnimation.Restricted);
            return;
        }

        List<Tile> routeTiles = tileManager.PathFind_RouteTiles(_currentHero.movement.currentTile, hoveringTile);

        for (int i = 0; i < routeTiles.Count; i++)
        {
            Tile routeTile = routeTiles[i];

            if (routeTile == hoveringTile) continue; // destination tile
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


    // End Turn Actions
    private IEnumerator Run_HeroActions()
    {
        if (_currentHero == null || _currentHero.data.currentData.currentHealth <= 0) yield break;

        StartCoroutine(_currentHero.Run_EndTurnActions());
        while (_currentHero.actionsRunning) yield return null;

        yield break;
    }
    private IEnumerator Refill_CurrentManaCount()
    {
        if (_currentHero == null || _currentHero.data.currentData.currentHealth <= 0) yield break;

        HeroData heroData = _currentHero.data;
        if (heroData.currentData.currentHealth <= 0) yield break;

        heroData.Update_CurrentManaCount(heroData.maxManaCount);
    }


    // Game Over
    private IEnumerator EndStage_OnHeroDeath()
    {
        if (_currentHero == null || _currentHero.data.currentData.currentHealth > 0) yield break;

        _heroDeathEventBus.RunSequential_DelayBusEvents();
    }


    // Stat Panels
    private float Update_Direction(float xPosition)
    {
        if (xPosition < 0f) return -1f;
        if (xPosition > 0f) return 1f;

        return 0f;
    }
    private void Update_StatPanelPositions(float cardPlatformWidth)
    {
        float halfPlatformWidth = cardPlatformWidth / 2f;

        float healthHalfWidth = _healthPanel.rectTransform.rect.width / 2f;
        float manaHalfWidth = _manaPanel.rectTransform.rect.width / 2f;

        float healthXPosition = halfPlatformWidth + _statPanelsSpacingValue + healthHalfWidth;
        float manaXPosition = halfPlatformWidth + _statPanelsSpacingValue + manaHalfWidth;

        Vector2 healthPos = _healthPanel.rectTransform.anchoredPosition;
        Vector2 manaPos = _manaPanel.rectTransform.anchoredPosition;

        healthPos.x = healthXPosition * Update_Direction(healthPos.x);
        manaPos.x = manaXPosition * Update_Direction(manaPos.x);

        _healthPanel.rectTransform.anchoredPosition = healthPos;
        _manaPanel.rectTransform.anchoredPosition = manaPos;
    }

    private void Update_HealthPanel()
    {
        if (_currentHero == null)
        {
            _healthPanel.Update_ValueText(0, 0);
            return;
        }

        InteractionData currentHeroData = _currentHero.data.currentData;
        _healthPanel.Update_ValueText(currentHeroData.currentHealth, currentHeroData.maxHealth);
    }
    private void Update_ManaPanel()
    {
        if (_currentHero == null)
        {
            _manaPanel.Update_ValueText(0, 0);
            return;
        }

        HeroData data = _currentHero.data;
        _manaPanel.Update_ValueText(data.currentManaCount, data.maxManaCount);
    }
}
