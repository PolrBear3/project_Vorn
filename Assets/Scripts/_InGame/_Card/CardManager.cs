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
    [Space(20)]
    [SerializeField] private GameObject _cardPrefab;


    private List<Card> _placedCards = new();
    public List<Card> placedCards => _placedCards;

    private CardManager_Data _data = new();
    public CardManager_Data data => _data;

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
    }


    // Data
    private void Set_Data()
    {

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

        GameObject placeCardObj = Instantiate(_cardPrefab, placeTile.transform.position, Quaternion.identity);
        placeCardObj.transform.SetParent(transform);

        placeTile.Set_Occupant(placeCardObj);

        if (placeCardObj.TryGetComponent(out Card placeCard) == false) return false;
        placeCard.Load(placeCardData, placeTile);

        _placedCards.Add(placeCard);
        _data.Add_PlacedData(placeCard);

        return true;
    }
}