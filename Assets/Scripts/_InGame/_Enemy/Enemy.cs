using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour, IInteractable
{
    [Space(20)]
    [SerializeField] private TileMovement_Controller _movement;
    public TileMovement_Controller movement => _movement;

    [SerializeField] private Animator_Controller _animator;
    public Animator_Controller animator => _animator;

    [Space(10)]
    [SerializeField] private InteractableHealth_Controller _healthController;
    public InteractableHealth_Controller healthController => _healthController;
    
    [SerializeField] private ActionClock _actionClock;


    private EnemyData _data;
    public EnemyData data => _data;


    private EventBus_Controller _preMovementActionBus = new();
    public EventBus_Controller preMovementActionBus => _preMovementActionBus;

    private EventBus_Controller _afterMovementActionBus = new();
    public EventBus_Controller afterMovementActionBus => _afterMovementActionBus;

    private bool _actionsRunning;
    public bool actionsRunning => _actionsRunning;


    // IInteractable
    public InteractionData interactionData => _data.currentData;


    // MonoBehaviour
    private void OnDestroy()
    {
        // from Set_Data
        _healthController.AfterDeathUpdate -= Remove_Data;
    }


    // Data
    public void Set_Data(Enemy_ScrObj setEnemy)
    {
        _data = new(setEnemy);
        
        _healthController.Set_Data(_data.currentData);
        _healthController.AfterDeathUpdate += Remove_Data;

        _actionClock.Toggle(false);
    }

    private void Remove_Data()
    {
        GameManager.instance.enemyManager.spawnedEnemies.Remove(this);
        Destroy(gameObject);
    }


    // Movement
    private Tile Hero_TargetTile()
    {
        GameManager manager = GameManager.instance;
        
        Hero currentHero = manager.heroManager.currentHero;
        if (currentHero == null) return null;
        
        Tile targetTile = manager.tileManager.ClosestAvailable_SurroundingTile(_movement.currentTile, currentHero.movement.currentTile);
        if (targetTile == null) return null;

        return targetTile;
    }
    private Tile TauntCard_TargetTile()
    {
        Tile currentTile = _movement.currentTile;

        GameManager manager = GameManager.instance;

        List<Card> closestCards = manager.cardManager.TileClosest_PlacedCards(currentTile);
        if (closestCards.Count <= 0) return null;

        for (int i = 0; i < closestCards.Count; i++)
        {
            Card card = closestCards[i];
            
            if (card.data.currentData.abilities.Contains(InteractableAbility.Taunt) == false) continue;
            return manager.tileManager.ClosestAvailable_SurroundingTile(currentTile, card.placedTile);
        }
        return null;
    }

    private void Moveto_TargetTile()
    {
        Tile currentTile = _movement.currentTile;
        Tile destinationTile = TauntCard_TargetTile() ?? Hero_TargetTile();

        if (destinationTile == null || currentTile == destinationTile) return;

        List<Tile> routeTiles = GameManager.instance.tileManager.PathFind_RouteTiles(currentTile, destinationTile);
        if (routeTiles.Count <= 0) return;

        Tile routeTile = routeTiles[0];

        _movement.Direction_FlipUpdate(routeTile);
        _movement.Moveto_Tile(routeTiles[0], _data.enemyScrObj.spawnOffset); // set routTile index value relative to movement range ?
    }
    
    
    // Damage
    private InteractionData DamageTarget_InteractionData()
    {
        GameManager manager = GameManager.instance;
        CardManager cardManager = manager.cardManager;

        Tile currentTile = _movement.currentTile;
        Vector2 currentTilePos = currentTile.data.position;

        int interactRange = _data.currentData.interactRange;

        // taunt card
        Card tauntCard = cardManager.TileClosest_PlacedCard(currentTile, cardManager.TileClosest_PlacedCards(currentTile, InteractableAbility.Taunt));
        bool tauntCardDamageable = tauntCard != null && Utility.Chebyshev_Distance(currentTilePos, tauntCard.placedTile.data.position) <= interactRange;

        if (tauntCardDamageable) return tauntCard.interactionData;
        if (tauntCard != null) return null; // restrict damaging if taunt cards are not in range

        // hero
        Hero currentHero = manager.heroManager.currentHero;
        if (currentHero != null && Utility.Chebyshev_Distance(currentTile.data.position, currentHero.movement.currentTile.data.position) <= interactRange)
        {
            return currentHero.interactionData;
        }

        // interact range card
        Card damageCard = cardManager.TileClosest_PlacedCard(currentTile);
        if (damageCard == null) return null;

        int distanceToCard = Utility.Chebyshev_Distance(currentTile.data.position, damageCard.placedTile.data.position);
        if (distanceToCard > interactRange) return null;

        return damageCard.interactionData;
    }
    private InteractionData Damage_RangedInteractable()
    {
        InteractionData damageTargetData = DamageTarget_InteractionData();
        if (damageTargetData == null) return null;

        int damageUpdateValue = damageTargetData.currentHealth + _data.currentData.healthModifyValue;
        damageTargetData.Update_CurrentHealth(damageUpdateValue);

        return damageTargetData;
    }


    // End Turn
    public IEnumerator Run_EndTurnActions()
    {
        _actionsRunning = true;
        _actionClock.Toggle(_actionsRunning);

        yield return _preMovementActionBus.RunSequential_DelayBusEvents();

        int movementRange = _data.movementRange; // movement
        for (int i = 0; i < movementRange; i++)
        {
            Moveto_TargetTile();
            while (_movement.movementCoroutine != null) yield return null;
        }

        InteractionData damageData = Damage_RangedInteractable(); // damage interactable
        if (damageData != null)
        {
            yield return null;
            while (damageData.healthUpdating) yield return null;
        }

        yield return _afterMovementActionBus.RunSequential_DelayBusEvents();

        _actionsRunning = false;
        _actionClock.Toggle(_actionsRunning);

        yield break;
    }
}