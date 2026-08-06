using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class Enemy : MonoBehaviour
{
    [Space(20)]
    [SerializeField] private TileMovement_Controller _movement;
    public TileMovement_Controller movement => _movement;


    private EnemyData _data;
    public EnemyData data => _data;

    private Card _targetCard;
    public Card targetCard => _targetCard;

    public Action OnEffectActivation;


    // MonoBehaviour
    private void OnDestroy()
    {
        // from Set_Data
        EnemyManager enemyManager = GameManager.instance.enemyManager;

        enemyManager.OnEnemyTurn -= Update_TargetCard;
        enemyManager.OnEnemyTurn -= Moveto_TargetCard;
        enemyManager.OnEnemyTurn -= Activate_Effects;
    }


    // Data
    public void Set_Data(Enemy_ScrObj setEnemy)
    {
        _data = new(setEnemy);


        EnemyManager enemyManager = GameManager.instance.enemyManager;

        enemyManager.OnEnemyTurn += Update_TargetCard;
        enemyManager.OnEnemyTurn += Moveto_TargetCard;
        enemyManager.OnEnemyTurn += Activate_Effects;
    }

    private List<Tile> MovementRange_Tiles()
    {
        Tile currentTile = _movement.currentTile;
        int movementRange = _data.enemyScrObj.movementRange;

        List<Tile> rangeTiles = GameManager.instance.tileManager.Distance_Tiles(currentTile, movementRange);

        for (int i = rangeTiles.Count - 1; i >= 0; i--)
        {
            if (rangeTiles[i].currentOccupant == null) continue;
            rangeTiles.RemoveAt(i);
        }
        return rangeTiles;
    }


    // Target Card
    private void Update_TargetCard()
    {
        GameManager manager = GameManager.instance;

        Tile currentTile = _movement.currentTile;
        List<Card> updateCards = manager.cardManager.TileClosest_PlacedCards(currentTile);

        _targetCard = null;
        if (updateCards.Count <= 0) return;

        TileManager tileManager = manager.tileManager;
        int interactRange = _data.enemyScrObj.interactionData.interactRange;

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

    private List<Tile> MovementRoute_Tiles(Tile startingTile)
    {
        return null;
    }
    private List<Tile> MovementRoute_Tiles()
    {
        List<Tile> routeTiles = new();
        if (_targetCard == null) return routeTiles;

        Tile targetCardTile = _targetCard.placedTile;
        Vector2 targetCardPos = targetCardTile.data.position;

        TileManager tileManager = GameManager.instance.tileManager;
        int maxRouteCount = tileManager.tiles.Count;

        Tile routeTile = _movement.currentTile;

        for (int i = 0; i < maxRouteCount; i++)
        {
            int shortestDistance = int.MaxValue;
            // int shortestRouteCount = routeTiles.Count

            List<Tile> moveTiles = tileManager.Distance_Tiles(routeTile, 1);
            if (moveTiles.Contains(targetCardTile)) break;

            for (int j = 0; j < moveTiles.Count; j++)
            {
                Tile moveTile = moveTiles[j];

                if (routeTiles.Contains(moveTile)) continue;
                if (moveTile.currentOccupant != null) continue;

                int distance = Utility.Chebyshev_Distance(moveTile.data.position, targetCardPos);
                if (distance >= shortestDistance) continue;

                shortestDistance = distance;
                routeTile = moveTile;
            }

            if (routeTile == null) return routeTiles;
            routeTiles.Add(routeTile);
        }
        return routeTiles;
    }

    private void Moveto_TargetCard()
    {
        if (_targetCard == null) return;

        List<Tile> routeTiles = MovementRoute_Tiles();
        if (routeTiles.Count <= 0) return;

        if (GameManager.instance.enemyManager.enemyMoved) return;
        GameManager.instance.enemyManager.enemyMoved = true;

        Debug.Log(routeTiles.Count);

        Tile moveTile = routeTiles[0];
        _movement.Moveto_Tile(moveTile, _data.enemyScrObj.spawnOffset);
    }


    // Effect
    private void Activate_Effects()
    {
        OnEffectActivation?.Invoke();
    }
}
