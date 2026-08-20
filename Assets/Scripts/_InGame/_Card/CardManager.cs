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
        manager.tileManager.tileSelectEventBus.UnRegister(Toggle_TileTargeting);

        EventBus_Controller endTurnBus = manager.stageManager.endTurnEventBus;

        endTurnBus.UnRegister(PlacedCards_Empty);
        endTurnBus.UnRegister(CardPlace_ActionRunning);
        endTurnBus.UnRegister(UnToggle_TileTargeting);
        endTurnBus.UnRegister(Run_CardActions);

        Input_Controller input = Input_Controller.instance;

        input.OnLeftClickPressed -= UnToggle_TileTargeting_onMissClick;
        input.OnRightClickPressed -= UnToggle_TileTargeting;
        input.OnLeftClickPressed -= Target_Tile;
    }


    // Data
    private void Set_Data()
    {
        GameManager manager = GameManager.instance;
        manager.tileManager.tileSelectEventBus.Register(0, Toggle_TileTargeting);

        EventBus_Controller endTurnBus = manager.stageManager.endTurnEventBus;

        endTurnBus.Register(PlacedCards_Empty);
        endTurnBus.Register(CardPlace_ActionRunning);
        endTurnBus.Register(0, UnToggle_TileTargeting);
        endTurnBus.Register(0, Run_CardActions);

        Input_Controller input = Input_Controller.instance;

        input.OnLeftClickPressed += UnToggle_TileTargeting_onMissClick;
        input.OnRightClickPressed += UnToggle_TileTargeting;
        input.OnLeftClickPressed += Target_Tile;
    }


    public bool PlacedCards_Empty()
    {
        return _placedCards.Count <= 0;
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
    public Card TileClosest_PlacedCard(Tile pivotTile)
    {
        if (_placedCards.Count <= 0) return null;

        Vector2 targetTilePos = pivotTile.data.position;

        int closestDistance = int.MaxValue;
        Card closestCard = null;

        for (int i = 0; i < _placedCards.Count; i++)
        {
            Card placedCard = _placedCards[i];
            Vector2 placedCardPos = placedCard.placedTile.data.position;

            int distance = Utility.Chebyshev_Distance(targetTilePos, placedCardPos);
            if (distance >= closestDistance) continue;

            closestDistance = distance;
            closestCard = placedCard;
        }
        return closestCard;
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

            if (placedCard.actionRunning == false) continue;
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

            StartCoroutine(card.Run_EndTurnActions());
            while (card != null && card.actionRunning || card.healthUpdating) yield return null;
        }
        yield break;
    }


    // Tile Targeting
    private Card TileTargeting_ToggledCard()
    {
        for (int i = 0; i < _placedCards.Count; i++)
        {
            Card card = _placedCards[i];

            if (card.tileTargeting.targetingToggled == false) continue;
            return card;
        }
        return null;
    }
    private bool TileTargeting_Complete(Card tileTargetingCard)
    {
        if (tileTargetingCard == null) return false;
        return tileTargetingCard.tileTargeting.targetingTiles.Count >= tileTargetingCard.data.currentData.targetSelectCount;
    }

    private void Update_TileTargeting_InfoText()
    {
        Cursor cursor = GameManager.instance.cursor;
        Card toggledCard = TileTargeting_ToggledCard();

        if (toggledCard == null)
        {
            cursor.Update_InfoText(null);
            return;
        }

        int maxTargetingCount = toggledCard.data.currentData.targetSelectCount;

        TileTargeting_Data tileTargeting = toggledCard.tileTargeting;
        int targetingTileCount = tileTargeting.targetingTiles.Count;

        string updateInfo = targetingTileCount < maxTargetingCount ? targetingTileCount + "/" + maxTargetingCount : null;
        cursor.Update_InfoText(updateInfo);
    }

    private void UnToggle_TileTargeting()
    {
        Card unToggleCard = TileTargeting_ToggledCard();
        if (unToggleCard == null) return;

        unToggleCard.tileTargeting.Toggle_Targeting(false);
        Update_TileTargeting_InfoText();
    }
    private void UnToggle_TileTargeting(bool isPressed)
    {
        if (isPressed == false) return;

        UnToggle_TileTargeting();
    }
    private void UnToggle_TileTargeting_onMissClick(bool isPressed)
    {
        if (isPressed == false) return;
        if (GameManager.instance.tileManager.hoveringTile != null) return;

        UnToggle_TileTargeting(true);
    }

    private void Toggle_TileTargeting()
    {
        GameManager manager = GameManager.instance;
        if (manager.stageManager.endTurnEventBus.DelayBus_Running()) return;

        Tile selectedTile = manager.tileManager.hoveringTile;
        if (selectedTile == null) return;

        Card toggledCard = TileTargeting_ToggledCard();
        if (toggledCard != null && toggledCard.placedTile != selectedTile) return; // target selecting

        Card selectedCard = PlacedCard(selectedTile);
        if (selectedCard == null) return;

        InteractionData data = selectedCard.data.currentData;
        if (data.targetSelectCount <= 0) return;

        bool toggled = selectedCard.tileTargeting.Toggle_Targeting();
        Update_TileTargeting_InfoText();

        if (toggled == false) return;

        _placedCards.Remove(selectedCard);
        _placedCards.Add(selectedCard);
    }
    private void Target_Tile(bool isPressed)
    {
        if (isPressed == false) return;

        Card toggledCard = TileTargeting_ToggledCard();
        if (toggledCard == null) return;

        GameManager manager = GameManager.instance;

        Tile selectedTile = manager.tileManager.hoveringTile;
        if (selectedTile == null || selectedTile == toggledCard.placedTile) return;

        int interactRange = toggledCard.data.currentData.interactRange;
        if (Utility.Chebyshev_Distance(toggledCard.placedTile.data.position, selectedTile.data.position) > interactRange) return;

        TileTargeting_Data tileTargeting = toggledCard.tileTargeting;

        tileTargeting.Target_Tile(selectedTile);
        Update_TileTargeting_InfoText();

        if (TileTargeting_Complete(toggledCard) == false) return;
        tileTargeting.Toggle_Targeting(false);
    }
}