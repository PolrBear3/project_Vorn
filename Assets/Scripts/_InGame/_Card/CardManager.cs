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
    private CardManager_Data _data = new(); // for save & load
    public CardManager_Data data => _data;

    private List<Card> _placedCards = new();
    public List<Card> placedCards => _placedCards;

    private CardManager_DragDropData _dragDropData;
    public CardManager_DragDropData dragDropData => _dragDropData;

    [Space(20)]
    [SerializeField] private ToolTip _placedCardHoverToolTip;


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

        endTurnBus.UnRegister(_placedCardHoverToolTip.UnToggle);
        endTurnBus.OnSequentialDelayFinish -= Hover_PlacedCard;

        manager.tileTargeting.OnToggleTargeting -= Hover_PlacedCard;
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
        endTurnBus.Register(2, Run_CardActions);

        endTurnBus.Register(0, _placedCardHoverToolTip.UnToggle);
        endTurnBus.OnSequentialDelayFinish += Hover_PlacedCard;

        manager.tileTargeting.OnToggleTargeting += Hover_PlacedCard;
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
    public List<Card> TileClosest_PlacedCards(Tile pivotTile, InteractableState targetAbility)
    {
        List<Card> placedCards = TileClosest_PlacedCards(pivotTile);

        for (int i = placedCards.Count - 1; i >= 0; i--)
        {
            if (placedCards[i].data.currentData.states.Contains(targetAbility)) continue;
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
    public List<Card> DistanceRanged_PlacedCards(Tile pivotTile, int distance, InteractableState targetAbility)
    {
        List<Card> placedCards = DistanceRanged_PlacedCards(pivotTile, distance);

        for (int i = placedCards.Count - 1; i >= 0; i--)
        {
            if (placedCards[i].data.currentData.states.Contains(targetAbility)) continue;
            placedCards.RemoveAt(i);
        }
        return placedCards;
    }


    // Cards
    public bool PlaceCard_OnTile(CardData placeCardData, Tile placeTile)
    {
        if (placeTile == null || placeTile.currentOccupant != null) return false;

        Card_ScrObj placingCard = placeCardData?.cardScrObj;
        if (placingCard == null) return false;

        GameObject cardPrefab = placingCard.placePrefab;
        if (cardPrefab == null) return false;

        HeroManager heroManager = GameManager.instance.heroManager;
        int cardManaCost = placingCard.manaCost;

        if (heroManager.Current_ManaCount() < cardManaCost)
        {
            // not enough mana panel animation ? 
            return false;
        }
        heroManager.Modify_CurrentManaCount(-cardManaCost);

        GameObject placeCardObj = Instantiate(cardPrefab, placeTile.transform.position, Quaternion.identity);
        if (placeCardObj.TryGetComponent(out Card card) == false)
        {
            Destroy(placeCardObj);
            return false;
        }

        placeCardObj.transform.SetParent(transform);
        placeTile.Set_Occupant(placeCardObj);

        _placedCards.Add(card);
        card.Set_Data(placeCardData, placeTile);

        StartCoroutine(card.placeUpdateActionBus.RunSequential_DelayBusEvents());

        Hover_PlacedCard();
        return true;
    }


    // Hover
    private List<Tile> HoverIndicate_Tiles(Card hoverCard, out string animationState)
    {
        if (hoverCard == null)
        {
            animationState = null;
            return null;
        }

        List<Tile> targetingTiles = new(hoverCard.targetingData.targetingTiles);
        if (targetingTiles.Count > 0)
        {
            animationState = UIAnimation.Available;
            return targetingTiles;
        }

        Tile hoverCardTile = hoverCard.placedTile;
        int interactRange = hoverCard.data.currentData.interactRange;

        List<Tile> interactRangeTiles = GameManager.instance.tileManager.Distanced_Tiles(hoverCardTile, interactRange);
        interactRangeTiles.Remove(hoverCardTile);

        animationState = UIAnimation.Toggle;
        return interactRangeTiles;
    }

    private void Hover_PlacedCard()
    {
        GameManager manager = GameManager.instance;

        if (manager.stageManager.endTurnEventBus.DelayBus_Running()) return;
        if (manager.tileTargeting.toggledSource != null) return;

        TileManager tileManager = manager.tileManager;
        Tile hoveringTile = tileManager.hoveringTile;

        Card placedCard = PlacedCard(hoveringTile);
        bool cardPlaced = placedCard != null;

        _placedCardHoverToolTip.Toggle(placedCard);

        if (hoveringTile == null || hoveringTile.currentOccupant == null)
        {
            tileManager.Reset_TileIndicators();
            return;
        }
        if (cardPlaced == false) return;

        List<Tile> indicateTiles = HoverIndicate_Tiles(placedCard, out string indicateStateString);
        foreach (Tile tile in indicateTiles)
        {
            tile.indicatorAnimController.Play_State(indicateStateString);
        }

        // ToolTip
        Card_ScrObj cardScrObj = placedCard.data.cardScrObj;

        _placedCardHoverToolTip.ToggleOn_CursorPoint(true);
        _placedCardHoverToolTip.Update_Contents(null, cardScrObj.contentSprite, cardScrObj.cardName, cardScrObj.cardDescription);
    }
    private void Hover_PlacedCard(bool tileTargetingToggled)
    {
        if (tileTargetingToggled)
        {
            _placedCardHoverToolTip.UnToggle();
            return;
        }
        Hover_PlacedCard();
    }


    // Actions
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
        Hero currentHero = GameManager.instance.heroManager.currentHero;
        if (currentHero != null && currentHero.data.currentData.currentHealth <= 0) yield break;

        List<Card> runActionCards = new(_placedCards);
        for (int i = 0; i < runActionCards.Count; i++)
        {
            Card card = runActionCards[i];
            if (card == null) continue;

            InteractionData cardData = card.data.currentData;

            StartCoroutine(card.Run_EndTurnActions());
            while (card != null && card.actionsRunning || cardData.dataUpdating) yield return null;
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
        if (selectedCard.data.currentData.states.Contains(InteractableState.Frozen)) return;

        bool toggled = manager.tileTargeting.Toggle_Targeting(selectedCard);
        if (toggled == false) return;

        // card actions run in targeting completed order
        _placedCards.Remove(selectedCard);
        _placedCards.Add(selectedCard);
    }
}