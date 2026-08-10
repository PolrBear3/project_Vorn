using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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


    // Target Card
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

    private List<Tile> MovementRoute_toTargetCard(Tile startingTile)
    {
        List<Tile> routeTiles = new();

        Tile destinationTile = TargetCard_DestinationTile(startingTile);
        if (destinationTile == null) return routeTiles;

        TileManager tileManager = GameManager.instance.tileManager;
        int maxRouteCount = tileManager.tiles.Count;

        Tile routeTile = startingTile;

        for (int i = 0; i < maxRouteCount; i++)
        {
            List<Tile> moveTiles = tileManager.Distance_Tiles(routeTile, 1);

            Tile nextRouteTile = null;
            int shortestDistance = int.MaxValue;

            for (int j = 0; j < moveTiles.Count; j++)
            {
                Tile moveTile = moveTiles[j];

                if (routeTiles.Contains(moveTile)) continue;
                if (moveTile.currentOccupant != null) continue;

                int distance = Utility.Chebyshev_Distance(moveTile.data.position, destinationTile.data.position);
                if (distance >= shortestDistance) continue;

                nextRouteTile = moveTile;
                shortestDistance = distance;
            }

            if (nextRouteTile == null) break;

            routeTile = nextRouteTile;
            routeTiles.Add(routeTile);

            if (routeTile == destinationTile) break;
        }
        return routeTiles;
    }
    private List<Tile> MovementRoute_toTargetCard()
    {
        List<Tile> routeTiles = new();

        Tile destinationTile = TargetCard_DestinationTile(_movement.currentTile);
        if (destinationTile == null) return routeTiles;

        TileManager tileManager = GameManager.instance.tileManager;
        int maxRouteCount = tileManager.tiles.Count;

        Tile routeTile = _movement.currentTile;

        for (int i = 0; i < maxRouteCount; i++)
        {
            List<Tile> moveTiles = tileManager.Distance_Tiles(routeTile, 1);

            Tile nextRouteTile = null;
            int lowestScore = int.MaxValue;

            for (int j = 0; j < moveTiles.Count; j++)
            {
                Tile moveTile = moveTiles[j];

                if (routeTiles.Contains(moveTile)) continue;
                if (moveTile.currentOccupant != null) continue;

                List<Tile> possibleRouteTiles = MovementRoute_toTargetCard(moveTile);
                if (possibleRouteTiles.Contains(destinationTile) == false) continue;

                int distance = Utility.Chebyshev_Distance(moveTile.data.position, destinationTile.data.position);
                int totalScore = distance + possibleRouteTiles.Count;

                if (totalScore >= lowestScore) continue;

                nextRouteTile = moveTile;
                lowestScore = totalScore;
            }

            if (nextRouteTile == null) break;

            routeTile = nextRouteTile;
            routeTiles.Add(routeTile);

            if (routeTile == destinationTile) break;
        }
        return routeTiles;
    }

    private void Moveto_TargetCard()
    {
        if (_targetCard == null) return;

        Tile currentTile = _movement.currentTile;
        if (currentTile == TargetCard_DestinationTile(currentTile)) return;

        List<Tile> routeTiles = MovementRoute_toTargetCard();
        if (routeTiles.Count <= 0) return;

        Tile moveTile = routeTiles[0];
        _movement.Moveto_Tile(moveTile, _data.enemyScrObj.spawnOffset);
    }


    // Effect
    private void Activate_Effects()
    {
        OnEffectActivation?.Invoke();
    }
}
