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

    public Action OnEffectActivation;


    // IInteractable
    public InteractionData interactionData => _data.currentData;

    private bool _healthUpdating;
    public bool healthUpdating => _healthUpdating;


    // MonoBehaviour
    private void OnDestroy()
    {
        // from Set_Data
        _data.currentData.OnCurrentHealthUpdate -= Update_OnDamaged;

        EnemyManager enemyManager = GameManager.instance.enemyManager;
        enemyManager.enemyActionBus.UnRegister(TargetCard_MovementUpdate);
    }


    // Data
    public void Set_Data(Enemy_ScrObj setEnemy)
    {
        _data = new(setEnemy);
        _data.currentData.OnCurrentHealthUpdate += Update_OnDamaged;

        EnemyManager enemyManager = GameManager.instance.enemyManager;
        enemyManager.enemyActionBus.Register(0, TargetCard_MovementUpdate);
    }


    // Movement
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
    private IEnumerator TargetCard_MovementUpdate()
    {
        Update_TargetCard();
        Moveto_TargetCard();

        while (_movement.movementCoroutine != null) yield return null;
        yield break;
    }


    // Interaction
    private void Update_OnDamaged(int healthUpdateValue)
    {
        if (healthUpdateValue >= 0) return;

        _animator.Play_State(1);

        _healthUpdating = true;
        StartCoroutine(OnDamaged_AnimationUpdate());
    }
    private IEnumerator OnDamaged_AnimationUpdate()
    {
        yield return null;
        while (_animator.CurrentState_Playing()) yield return null;

        _healthUpdating = false;
        yield break;
    }
}
