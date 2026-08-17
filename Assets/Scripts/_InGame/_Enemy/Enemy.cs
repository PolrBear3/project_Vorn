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


    private EnemyData _data;
    public EnemyData data => _data;

    private Card _targetCard;
    public Card targetCard => _targetCard;


    private bool _actionRunning;
    public bool actionRunning => _actionRunning;

    private EventBus_Controller _preMovementActionBus = new();
    public EventBus_Controller preMovementActionBus => _preMovementActionBus;

    private EventBus_Controller _afterMovementActionBus = new();
    public EventBus_Controller afterMovementActionBus => _afterMovementActionBus;

    private EventBus_Controller _healthUpdateActionBus = new();
    public EventBus_Controller healthUpdateActionBus => _healthUpdateActionBus;

    private EventBus_Controller _deathUpdateActionBus = new();
    public EventBus_Controller deathUpdateActionBus => _deathUpdateActionBus;


    public InteractionData interactionData => _data.currentData;

    private bool _healthUpdating;
    public bool healthUpdating => _healthUpdating;


    // MonoBehaviour
    private void OnDestroy()
    {
        // from Set_Data
        _data.currentData.OnCurrentHealthUpdate -= Handle_HealthUpdate;
    }


    // Data
    public void Set_Data(Enemy_ScrObj setEnemy)
    {
        _data = new(setEnemy);
        _data.currentData.OnCurrentHealthUpdate += Handle_HealthUpdate;
    }


    // Actions
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

    public IEnumerator Run_EndTurnActions()
    {
        _actionRunning = true;

        yield return _preMovementActionBus.SequentialDelayBus_RunUpdate();

        Update_TargetCard();
        Moveto_TargetCard();
        while (_movement.movementCoroutine != null) yield return null;

        yield return _afterMovementActionBus.SequentialDelayBus_RunUpdate();

        _actionRunning = false;
        yield break;
    }


    // Interaction
    private void Handle_HealthUpdate(int healthUpdateValue)
    {
        string animState = healthUpdateValue < 0 ? EnemyAnimation.Damage : EnemyAnimation.Heal;
        _animator.Play_State(animState);

        _healthUpdating = true;
        StartCoroutine(HealthUpdate_HandleDelay());
    }
    private bool Handle_Death()
    {
        if (_data.currentData.currentHealth > 0) return false;

        _movement.currentTile.Set_Occupant(null);
        _animator.Play_State(EnemyAnimation.Death);

        return true;
    }

    private IEnumerator HealthUpdate_HandleDelay()
    {
        yield return null;
        while (_animator.CurrentState_Playing()) yield return null;

        yield return _healthUpdateActionBus.SequentialDelayBus_RunUpdate();

        if (Handle_Death())
        {
            yield return null;
            while (_animator.CurrentState_Playing()) yield return null;

            yield return _deathUpdateActionBus.SequentialDelayBus_RunUpdate();

            _actionRunning = false;
            _healthUpdating = false;

            GameManager.instance.enemyManager.spawnedEnemies.Remove(this);
            Destroy(gameObject);
        }

        _healthUpdating = false;
        yield break;
    }
}
