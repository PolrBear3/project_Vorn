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


    private EnemyData _data;
    public EnemyData data => _data;

    private Card _targetCard;
    public Card targetCard => _targetCard;


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
    }

    private void Remove_Data()
    {
        GameManager.instance.enemyManager.spawnedEnemies.Remove(this);
    }


    // End Turn Action
    private void Update_TargetCard()
    {
        GameManager manager = GameManager.instance;

        Tile currentTile = _movement.currentTile;
        List<Card> updateCards = manager.cardManager.TileClosest_PlacedCards(currentTile);

        _targetCard = null;
        if (updateCards.Count <= 0) return;

        TileManager tileManager = manager.tileManager;
        int interactRange = Mathf.Max(1, _data.currentData.interactRange);

        for (int i = 0; i < updateCards.Count; i++)
        {
            Card updateCard = updateCards[i];

            Tile cardTile = updateCard.placedTile;
            List<Tile> interactRangeTiles = tileManager.Distance_Tiles(cardTile, interactRange);

            bool hasEmptyTile = false;

            for (int j = 0; j < interactRangeTiles.Count; j++)
            {
                Tile rangeTile = interactRangeTiles[j];
                if (rangeTile != currentTile && rangeTile.currentOccupant != null) continue;

                hasEmptyTile = true;
                break;
            }
            if (hasEmptyTile == false) continue;

            _targetCard = updateCard;
            break;
        }
    }
    private Tile TargetCard_DestinationTile(Tile pivotTile)
    {
        if (_targetCard == null) return null;

        TileManager tileManager = GameManager.instance.tileManager;
        int interactRange = Mathf.Max(1, _data.currentData.interactRange);

        for (int i = 0; i < interactRange; i++)
        {
            int checkRange = i + 1;

            List<Tile> rangeTiles = tileManager.Distance_Tiles(_targetCard.placedTile, checkRange);
            List<Tile> closeSortedTiles = tileManager.CloseSorted_Tiles(pivotTile, rangeTiles);

            for (int j = 0; j < closeSortedTiles.Count; j++)
            {
                Tile destinationTile = closeSortedTiles[j];

                if (pivotTile == destinationTile) return destinationTile;
                if (destinationTile.currentOccupant != null) continue;

                return destinationTile;
            }
        }
        return null;
    }

    private void Moveto_TargetCard()
    {
        if (_targetCard == null) return;

        Tile currentTile = _movement.currentTile;
        Tile destinationTile = TargetCard_DestinationTile(currentTile);

        if (currentTile == destinationTile) return;

        List<Tile> routeTiles = GameManager.instance.tileManager.PathFind_RouteTiles(currentTile, destinationTile);
        if (routeTiles.Count <= 0) return;

        _movement.Moveto_Tile(routeTiles[0], _data.enemyScrObj.spawnOffset);
    }
    private Card Damage_RangedCard()
    {
        Card damageCard = GameManager.instance.cardManager.TileClosest_PlacedCard(_movement.currentTile);
        if (damageCard == null) return null;

        int distanceToCard = Utility.Chebyshev_Distance(_movement.currentTile.data.position, damageCard.placedTile.data.position);
        if (distanceToCard > _data.currentData.interactRange) return null;

        InteractionData cardInteractionData = damageCard.data.currentData;

        int damageUpdateValue = cardInteractionData.currentHealth + _data.currentData.healthModifyValue;
        cardInteractionData.Update_CurrentHealth(damageUpdateValue);

        return damageCard;
    }

    public IEnumerator Run_EndTurnActions()
    {
        _actionsRunning = true;

        yield return _preMovementActionBus.RunSequential_DelayBusEvents();

        Update_TargetCard();
        Moveto_TargetCard();
        while (_movement.movementCoroutine != null) yield return null;

        Card damageCard = Damage_RangedCard();
        if (damageCard != null)
        {
            InteractionData cardData = damageCard.data.currentData;

            yield return null;
            while (cardData.healthUpdating) yield return null;
        }

        yield return _afterMovementActionBus.RunSequential_DelayBusEvents();

        _actionsRunning = false;
        yield break;
    }
}
