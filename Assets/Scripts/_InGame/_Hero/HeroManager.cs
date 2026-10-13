using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroManager : MonoBehaviour, ISaveLoadable, ISubscriptionHandler
{
    [Space(20)]
    [SerializeField] private HeroSelect_Manager _heroSelectManager;
    public HeroSelect_Manager heroSelectManager => _heroSelectManager;

    [Space(20)]
    [SerializeField] private Hero_StatPanel _healthPanel;
    [SerializeField] private Hero_StatPanel _manaPanel;

    [Space(10)]
    [SerializeField][Range(0, 1000)] private float _statPanelsSpacingValue;

    [Space(20)]
    [SerializeField] private ToolTip _heroHoverToolTip;


    private HeroManager_Data _data;
    public HeroManager_Data data => _data;

    private Hero _spawnedHero;
    public Hero spawnedHero => _spawnedHero;

    private int _recentMovementManaCost;


    private EventBus_Controller _heroDeathEventBus = new();
    public EventBus_Controller heroDeathEventBus => _heroDeathEventBus;



    // MonoBehaviour
    private void Awake()
    {
        EventBus_GlobalController.Register(this);
        EventBus_GlobalController.Register(EventBus.AwakeLoad, Subscribe_All);
    }

    private void OnDestroy()
    {
        UnSubscribe_All();

        EventBus_GlobalController.UnRegister(this);
        EventBus_GlobalController.UnRegister(Subscribe_All);
    }


    // ISubscriptionHandler
    public void Subscribe_All()
    {
        GameManager manager = GameManager.instance;

        manager.deckBuildManager.OnCardPoolLoad += Load_ProprietaryCards_toCardPool;

        StageManager stageManager = manager.stageManager;
        EventBus_Controller stageSetBus = stageManager.setStageEventBus;

        stageSetBus.Register(1, Add_SpawnCard_toTopDeck);

        stageSetBus.Register(0, Update_HealthPanel);
        stageSetBus.Register(0, Update_ManaPanel);

        EventBus_Controller stageEndBus = stageManager.endStageEventBus;

        stageEndBus.Register(0, Update_HealthPanel);
        stageEndBus.Register(0, Update_ManaPanel);

        EventBus_Controller endTurnBus = stageManager.endTurnEventBus;

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
        tileManager.tileTargeting.OnTargetTile += UpdateMana_OnTileMovementTarget;

        HandInventory handInventory = manager.handInventory;
        handInventory.OnPlatformWidthUpdate += Update_StatPanelPositions;

        EventBus_Controller placeCardEventBus = handInventory.placeCardEventBus;

        placeCardEventBus.Register(0, Cancel_TileMovementTargeting_OnRouteBlocked);
        placeCardEventBus.Register(0, UpdateMana_OnTileMovementTarget);

        tileHoverEventBus.Register(0, Hover_CurrentHero);
        endTurnBus.OnSequentialDelayFinish += Hover_CurrentHero;

        endTurnBus.Register(0, _heroHoverToolTip.UnToggle);
    }

    public void UnSubscribe_All()
    {
        GameManager manager = GameManager.instance;

        manager.deckBuildManager.OnCardPoolLoad -= Load_ProprietaryCards_toCardPool;

        StageManager stageManager = manager.stageManager;
        EventBus_Controller stageSetBus = stageManager.setStageEventBus;

        stageSetBus.UnRegister(Add_SpawnCard_toTopDeck);

        stageSetBus.UnRegister(Update_HealthPanel);
        stageSetBus.UnRegister(Update_ManaPanel);

        EventBus_Controller stageEndBus = stageManager.endStageEventBus;

        stageEndBus.UnRegister(Update_HealthPanel);
        stageEndBus.UnRegister(Update_ManaPanel);

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
        tileManager.tileTargeting.OnTargetTile -= UpdateMana_OnTileMovementTarget;

        HandInventory handInventory = manager.handInventory;
        handInventory.OnPlatformWidthUpdate -= Update_StatPanelPositions;

        EventBus_Controller placeCardEventBus = handInventory.placeCardEventBus;

        placeCardEventBus.UnRegister(Cancel_TileMovementTargeting_OnRouteBlocked);
        placeCardEventBus.UnRegister(UpdateMana_OnTileMovementTarget);

        tileHoverEventBus.UnRegister(Hover_CurrentHero);
        endTurnBus.OnSequentialDelayFinish -= Hover_CurrentHero;

        endTurnBus.UnRegister(_heroHoverToolTip.UnToggle);

        // from Track_CurrentHero
        if (_spawnedHero == null) return;

        _spawnedHero.interactionData.OnHealthUpdate -= _healthPanel.Update_ValueText;
        _spawnedHero.data.OnManaUpdate -= _manaPanel.Update_ValueText;
    }


    // ISaveLoadable
    public void Save_Data()
    {
        ES3.Save("HeroManager/HeroManager_Data", _data, SaveKeys.GamePlayFile);
    }

    public void Load_Data()
    {
        _data = ES3.Load("HeroManager/HeroManager_Data", SaveKeys.GamePlayFile, new HeroManager_Data(_heroSelectManager.New_SelectableHeroes()));
    }


    // Data
    private bool Hero_Unavailable()
    {
        return _spawnedHero == null && GameManager.instance.cardManager.placedCards.Count <= 0;
    }

    private void Load_ProprietaryCards_toCardPool()
    {
        Hero_ScrObj selectedHero = _data.selectedHero;
        if (selectedHero == null) return;

        DeckBuild_Manager deckBuildManager = GameManager.instance.deckBuildManager;
        Card_ScrObj[] proprietaryCards = selectedHero.proprietaryCards;

        for (int i = 0; i < proprietaryCards.Length; i++)
        {
            deckBuildManager.cardPool.Add(proprietaryCards[i]);
        }
    }
    private void Add_SpawnCard_toTopDeck()
    {
        Hero_ScrObj selectedHero = _data.selectedHero;
        if (selectedHero == null) return;

        GameManager.instance.handInventory.AddCard_toTopDeck(new(selectedHero.spawnCard));
    }

    public void Track_SpawnedHero(Hero heroToTrack)
    {
        if (heroToTrack == null) return;

        heroToTrack.transform.SetParent(transform);

        _spawnedHero = heroToTrack;
        _spawnedHero.interactionData.OnHealthUpdate += _healthPanel.Update_ValueText;
        _spawnedHero.data.OnManaUpdate += _manaPanel.Update_ValueText;

        Update_HealthPanel();
        Update_ManaPanel();
    }


    // Mana
    public int Current_ManaCount()
    {
        if (_spawnedHero == null) return 0;
        return _spawnedHero.data.currentManaCount;
    }
    public void Modify_CurrentManaCount(int modifyCount)
    {
        if (_spawnedHero == null || modifyCount == 0) return;

        int currentManaCount = _spawnedHero != null ? _spawnedHero.data.currentManaCount : 0;
        _spawnedHero.data.Update_CurrentManaCount(currentManaCount + modifyCount);
    }


    // Movement Targeting
    private List<Tile> RouteTiles_toDestionation()
    {
        if (_spawnedHero == null) return null;

        List<Tile> routeTiles = _spawnedHero.tileTargeting.targetingTiles;
        if (routeTiles.Count <= 0) return routeTiles;

        Tile destinationTile = routeTiles[0];
        return GameManager.instance.tileManager.PathFind_RouteTiles(_spawnedHero.movement.currentTile, destinationTile);
    }

    private void Toggle_TileMovementTargeting()
    {
        GameManager manager = GameManager.instance;
        Tile selectedTile = manager.tileManager.hoveringTile;

        if (selectedTile == null) return;
        if (_spawnedHero == null || _spawnedHero.movement.currentTile != selectedTile) return;

        if (manager.tileManager.tileTargeting.Toggle_Targeting(_spawnedHero) == false) return;

        HeroData heroData = _spawnedHero.data;

        heroData.Update_CurrentManaCount(heroData.currentManaCount + _recentMovementManaCost);
        _recentMovementManaCost = 0;
    }

    private void Cancel_TileMovementTargeting()
    {
        if (_spawnedHero == null) return;

        TileTargeting_Data targetingData = _spawnedHero.tileTargeting;

        targetingData.targetingTiles.Clear();
        targetingData.recentTargetingTiles.Clear();

        Refund_MovementManaCost();
    }
    private void Cancel_TileMovementTargeting_OnRouteBlocked()
    {
        if (_spawnedHero == null) return;

        List<Tile> targetingTiles = _spawnedHero.tileTargeting.targetingTiles;
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
        if (_spawnedHero == null) return;

        int totalManaCost = RouteTiles_toDestionation().Count;
        int manaCostDifference = totalManaCost - _recentMovementManaCost;

        if (manaCostDifference == 0) return;

        HeroData heroData = _spawnedHero.data;

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
        if (_spawnedHero == null) return;
        if (heroSource is not Hero hero || _spawnedHero != hero) return;

        UpdateMana_OnTileMovementTarget();
    }

    private void Refund_MovementManaCost()
    {
        if (_spawnedHero == null) return;
        HeroData heroData = _spawnedHero.data;

        heroData.Update_CurrentManaCount(heroData.currentManaCount + _recentMovementManaCost);
        _recentMovementManaCost = 0;
    }

    private void Update_MovementRoute_OnTileTargeting()
    {
        if (_spawnedHero == null) return;

        GameManager manager = GameManager.instance;
        if (manager.tileManager.tileTargeting.toggledSource is not Hero hero || _spawnedHero != hero) return;

        TileManager tileManager = manager.tileManager;

        Tile hoveringTile = tileManager.hoveringTile;
        if (hoveringTile == null) return;

        if (_spawnedHero.Targeting_Available(hoveringTile) == false)
        {
            hoveringTile.indicatorAnimController.Play_State(UIAnimation.Restricted);
            return;
        }

        List<Tile> routeTiles = tileManager.PathFind_RouteTiles(_spawnedHero.movement.currentTile, hoveringTile);

        for (int i = 0; i < routeTiles.Count; i++)
        {
            Tile routeTile = routeTiles[i];

            if (routeTile == hoveringTile) continue; // destination tile
            routeTile.indicatorAnimController.Play_State(UIAnimation.Available);
        }
    }
    private void Update_MovementRoute_OnHeroHover()
    {
        if (_spawnedHero == null) return;

        GameManager manager = GameManager.instance;

        if (manager.stageManager.endTurnEventBus.DelayBus_Running()) return;
        if (manager.tileManager.tileTargeting.toggledSource != null) return;

        TileManager tileManager = manager.tileManager;
        Tile hoveringTile = tileManager.hoveringTile;

        if (hoveringTile == null || hoveringTile != _spawnedHero.movement.currentTile)
        {
            tileManager.Reset_TileIndicators();
            return;
        }

        List<Tile> routeTiles = _spawnedHero.tileTargeting.targetingTiles;
        if (routeTiles.Count <= 0) return;

        Tile destinationTile = routeTiles[0];
        routeTiles = tileManager.PathFind_RouteTiles(_spawnedHero.movement.currentTile, destinationTile);

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
        if (_spawnedHero == null || _spawnedHero.data.currentData.currentHealth <= 0) yield break;

        StartCoroutine(_spawnedHero.Run_EndTurnActions());
        while (_spawnedHero.actionsRunning) yield return null;

        yield break;
    }
    private IEnumerator Refill_CurrentManaCount()
    {
        if (_spawnedHero == null || _spawnedHero.data.currentData.currentHealth <= 0) yield break;

        HeroData heroData = _spawnedHero.data;
        if (heroData.currentData.currentHealth <= 0) yield break;

        heroData.Update_CurrentManaCount(heroData.maxManaCount);
    }


    // Game Over
    private IEnumerator EndStage_OnHeroDeath()
    {
        if (_spawnedHero == null || _spawnedHero.data.currentData.currentHealth > 0) yield break;

        _heroDeathEventBus.RunSequential_DelayBusEvents();
        if (_heroDeathEventBus.DelayBus_Running()) yield return null;

        GameManager.instance.Reset_GamePlay();
    }


    // Current Hero
    private void Hover_CurrentHero()
    {
        GameManager manager = GameManager.instance;
        if (manager.stageManager.endTurnEventBus.DelayBus_Running()) return;

        Tile hoveringTile = manager.tileManager.hoveringTile;
        bool toggleToolTip = _spawnedHero != null && hoveringTile == _spawnedHero.movement.currentTile;

        _heroHoverToolTip.Toggle(toggleToolTip);
        if (toggleToolTip == false) return;

        // ToolTip
        CharacterScrObj heroScrObj = _spawnedHero.data.heroScrObj;

        _heroHoverToolTip.ToggleOn_CursorPoint(true);
        _heroHoverToolTip.Update_Contents(heroScrObj.toolTipBaseSprite, null, heroScrObj.characterName, heroScrObj.characterDescription);
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
        if (_spawnedHero == null)
        {
            _healthPanel.Update_ValueText(0, 0);
            return;
        }

        InteractionData currentHeroData = _spawnedHero.data.currentData;
        _healthPanel.Update_ValueText(currentHeroData.currentHealth, currentHeroData.maxHealth);
    }
    private void Update_ManaPanel()
    {
        if (_spawnedHero == null)
        {
            _manaPanel.Update_ValueText(0, 0);
            return;
        }

        HeroData data = _spawnedHero.data;
        _manaPanel.Update_ValueText(data.currentManaCount, data.maxManaCount);
    }
}