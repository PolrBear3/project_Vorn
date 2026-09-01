using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardManager_DragDropData
{
    private CardData _draggingCardData;
    public CardData draggingCardData => _draggingCardData;

    private Vector2 _draggedTilePosition;
    public Vector2 draggedTilePosition => _draggedTilePosition;

    private bool _draggedOnClick;
    public bool draggedOnClick => _draggedOnClick;

    // New
    public CardManager_DragDropData(CardData draggingCardData, Vector2 draggedTilePos)
    {
        _draggingCardData = draggingCardData;
        _draggedTilePosition = draggedTilePos;
        _draggedOnClick = true;
    }

    // Data
    public void DragComplete()
    {
        _draggedOnClick = false;
    }
}

public class CardManager : MonoBehaviour
{
    private CardManager_Data _data = new();
    public CardManager_Data data => _data;

    private List<Card> _placedCards = new();
    public List<Card> placedCards => _placedCards;

    private CardManager_DragDropData _dragDropData;
    public CardManager_DragDropData dragDropData => _dragDropData;


    // MonoBehaviour
    private void Awake()
    {
        EventBus_GlobalController.Register(EventBus.AwakeLoad, Set_Data);
    }

    private void OnDestroy()
    {
        EventBus_GlobalController.UnRegister(EventBus.AwakeLoad, Set_Data);


        // from Set_Data
        GameManager manager = GameManager.instance;

        TileManager tileManager = manager.tileManager;

        tileManager.tileHoverEventBus.UnRegister(Hover_PlacedCard);
        tileManager.tileSelectEventBus.UnRegister(Toggle_TileTargeting);

        EventBus_Controller endTurnBus = manager.stageManager.endTurnEventBus;

        endTurnBus.UnRegister(CardPlace_ActionRunning);
        endTurnBus.UnRegister(Run_CardActions);
    }


    // Data
    private void Set_Data()
    {
        GameManager manager = GameManager.instance;

        TileManager tileManager = manager.tileManager;

        tileManager.tileHoverEventBus.Register(0, Hover_PlacedCard);
        tileManager.tileSelectEventBus.Register(0, Toggle_TileTargeting);

        EventBus_Controller endTurnBus = manager.stageManager.endTurnEventBus;

        endTurnBus.Register(CardPlace_ActionRunning);
        endTurnBus.Register(1, Run_CardActions);
    }


    public Card PlacedCard(Tile placedTile)
    {
        for (int i = 0; i < _placedCards.Count; i++)
        {
            Card card = _placedCards[i];

            if (card.placedTile != placedTile) continue;
            return card;
        }
        return null;
    }

    public Card TileClosest_PlacedCard(Tile pivotTile, List<Card> targetCards)
    {
        if (pivotTile == null || targetCards.Count <= 0) return null;

        Vector2 pivotTilePos = pivotTile.data.position;

        int closestDistance = int.MaxValue;
        List<Card> closestCards = new();

        for (int i = 0; i < targetCards.Count; i++)
        {
            Card targetCard = targetCards[i];
            Vector2 placedCardPos = targetCard.placedTile.data.position;

            int distance = Utility.Chebyshev_Distance(pivotTilePos, placedCardPos);
            if (distance > closestDistance) continue;

            if (distance == closestDistance)
            {
                closestCards.Add(targetCard);
                continue;
            }

            closestCards.Clear();
            closestCards.Add(targetCard);

            closestDistance = distance;
        }

        if (closestCards.Count <= 0) return null;
        return closestCards[UnityEngine.Random.Range(0, closestCards.Count)];
    }
    public Card TileClosest_PlacedCard(Tile pivotTile)
    {
        return TileClosest_PlacedCard(pivotTile, _placedCards);
    }

    public List<Card> TileClosest_PlacedCards(Tile pivotTile)
    {
        List<Card> placedCards = new(_placedCards);
        Vector2 pivotTilePos = pivotTile.data.position;

        placedCards.Sort((cardA, cardB) =>
        {
            int distanceA = Utility.Chebyshev_Distance(pivotTilePos, cardA.placedTile.data.position);
            int distanceB = Utility.Chebyshev_Distance(pivotTilePos, cardB.placedTile.data.position);

            return distanceA.CompareTo(distanceB);
        });
        return placedCards;
    }
    public List<Card> TileClosest_PlacedCards(Tile pivotTile, InteractableAbility targetAbility)
    {
        List<Card> placedCards = TileClosest_PlacedCards(pivotTile);

        for (int i = placedCards.Count - 1; i >= 0; i--)
        {
            if (placedCards[i].data.currentData.abilities.Contains(targetAbility)) continue;
            placedCards.RemoveAt(i);
        }
        return placedCards;
    }

    public List<Card> DistanceRanged_PlacedCards(Tile pivotTile, int distance)
    {
        Vector2 pivotTilePos = pivotTile.data.position;
        List<Card> placedCards = TileClosest_PlacedCards(pivotTile);

        for (int i = placedCards.Count - 1; i >= 0; i--)
        {
            if (Utility.Chebyshev_Distance(pivotTilePos, placedCards[i].placedTile.data.position) <= distance) continue;
            placedCards.RemoveAt(i);
        }
        return placedCards;
    }
    public List<Card> DistanceRanged_PlacedCards(Tile pivotTile, int distance, InteractableAbility targetAbility)
    {
        List<Card> placedCards = DistanceRanged_PlacedCards(pivotTile, distance);

        for (int i = placedCards.Count - 1; i >= 0; i--)
        {
            if (placedCards[i].data.currentData.abilities.Contains(targetAbility)) continue;
            placedCards.RemoveAt(i);
        }
        return placedCards;
    }


    // Cards
    public bool PlaceCard_OnTile(CardData placeCardData, Tile placeTile)
    {
        if (placeCardData == null || placeCardData.cardScrObj == null) return false;
        if (placeTile == null || placeTile.currentOccupant != null) return false;

        GameObject cardPrefab = placeCardData.cardScrObj.placePrefab;
        if (cardPrefab == null) return false;

        GameObject placeCardObj = Instantiate(cardPrefab, placeTile.transform.position, Quaternion.identity);
        placeCardObj.transform.SetParent(transform);

        placeTile.Set_Occupant(placeCardObj);

        if (placeCardObj.TryGetComponent(out Card placeCard) == false) return false;
        _placedCards.Add(placeCard);

        placeCard.Set_Data(placeCardData, placeTile);
        StartCoroutine(placeCard.placeUpdateActionBus.RunSequential_DelayBusEvents());

        return true;
    }
    
    private List<Tile> HoverIndicate_Tiles(Card hoverCard, out string indicateStateString)
    {
        if (hoverCard == null)
        {
            indicateStateString = null;
            return null;
        }

        List<Tile> targetingTiles = new(hoverCard.targetingData.targetingTiles);

        if (targetingTiles.Count > 0)
        {
            indicateStateString = UIAnimation.Available;
            return targetingTiles;
        }

        Tile hoverCardTile = hoverCard.placedTile;

        List<Tile> interactRangeTiles = GameManager.instance.tileManager.Distanced_Tiles(hoverCardTile, hoverCard.data.currentData.interactRange);
        interactRangeTiles.Remove(hoverCardTile);

        indicateStateString = UIAnimation.Toggle;
        return interactRangeTiles;
    }
    private void Hover_PlacedCard()
    {
        GameManager manager = GameManager.instance;

        if (manager.stageManager.endTurnEventBus.DelayBus_Running()) return;
        if (manager.tileTargeting.toggledSource != null) return;

        TileManager tileManager = manager.tileManager;
        Tile hoveringTile = tileManager.hoveringTile;

        if (hoveringTile == null)
        {
            tileManager.Reset_TileIndicators();
            return;
        }

        Card placedCard = PlacedCard(hoveringTile);
        if (placedCard == null) return;

        List<Tile> indicateTiles = HoverIndicate_Tiles(placedCard, out string indicateStateString);
        foreach (Tile tile in indicateTiles)
        {
            tile.indicatorAnimController.Play_State(indicateStateString);
        }
    }

    public bool CardPlace_ActionRunning()
    {
        for (int i = 0; i < _placedCards.Count; i++)
        {
            if (_placedCards[i].placeUpdateActionBus.DelayBus_Running()) return true;
        }
        return false;
    }
    public Tile ActionRunningCard_TargetingTile()
    {
        for (int i = 0; i < _placedCards.Count; i++)
        {
            Card placedCard = _placedCards[i];

            if (placedCard.actionsRunning == false) continue;
            return placedCard.targetingTile;
        }
        return null;
    }

    private IEnumerator Run_CardActions()
    {
        List<Card> runActionCards = new(_placedCards);

        for (int i = 0; i < runActionCards.Count; i++)
        {
            Card card = runActionCards[i];
            if (card == null) continue;

            InteractionData cardData = card.data.currentData;

            StartCoroutine(card.Run_EndTurnActions());
            while (card != null && card.actionsRunning || cardData.healthUpdating) yield return null;
        }
        yield break;
    }


    // Tile Targeting
    private void Toggle_TileTargeting()
    {
        GameManager manager = GameManager.instance;

        Tile selectedTile = manager.tileManager.hoveringTile;
        if (selectedTile == null) return;

        Card selectedCard = PlacedCard(selectedTile);
        if (selectedCard == null) return;

        bool toggled = manager.tileTargeting.Toggle_Targeting(selectedCard);
        if (toggled == false) return;

        // card actions run in targeting completed order
        _placedCards.Remove(selectedCard);
        _placedCards.Add(selectedCard);
    }
}